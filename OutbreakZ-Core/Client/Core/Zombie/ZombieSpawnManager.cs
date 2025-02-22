using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Client.Config;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Zombie
{
    public class ZombieSpawnManager : BaseScript
    {
        public static class Variables
        {
            private static object _lock = new object();
            private static bool _zombieDebug;

            public static void ToggleZombieDebug()
            {
                lock (_lock)
                {
                    _zombieDebug = !_zombieDebug;
                }
            }

            public static bool ZombieDebug
            {
                get
                {
                    lock (_lock)
                    {
                        return _zombieDebug;
                    }
                }
            }
        }

        // Dictionary<handle, context>
        private static readonly Dictionary<int, ZombieContext> ZombieContexts = new Dictionary<int, ZombieContext>();
        private static readonly object ZombieContextsLock = new object();

        private static readonly Random Random = new Random();
        private static bool _spawnEnabled;
        private static readonly object EnabledLock = new object();

        public ZombieSpawnManager()
        {
            EventHandlers["gameEventTriggered"] += new Action<string, dynamic>(OnGameEventTriggered);
        }

        public static bool Disable()
        {
            return SetSpawnEnabled(false);
        }

        public static bool Enable()
        {
            return SetSpawnEnabled(true);
        }

        public static string GetZombieStats()
        {
            bool spawnEnabled = false;
            lock (EnabledLock)
            {
                spawnEnabled = _spawnEnabled;
            }

            string spawnerStatus = spawnEnabled ? "Enabled" : "Disabled";

            int zombieCount = 0;
            int owner = 0;

            lock (ZombieContextsLock)
            {
                zombieCount = ZombieContexts.Count;
                if (zombieCount > 0)
                {
                    owner = ZombieInOwner();
                }
            }

            bool debug = Variables.ZombieDebug;
            return
                $"Spawner: {spawnerStatus} | Zombies: {zombieCount} | Owner: {owner}/{ClientConfig.MaxOwnZombies} | Debug: {debug}";
        }

        private void OnGameEventTriggered(string eventName, dynamic args)
        {
            if (eventName != "CEventNetworkEntityDamage") return;

            int victimEntity = int.Parse(args[0].ToString());
            int attackerEntity = int.Parse(args[1].ToString());

            if (IsEntityDead(victimEntity) || !DoesEntityExist(victimEntity) || !IsEntityAPed(victimEntity) ||
                IsEntityDead(attackerEntity) || !DoesEntityExist(attackerEntity) ||
                !IsEntityAPed(attackerEntity)) return;

            ZombieContext zombieContext = null;
            lock (ZombieContextsLock)
            {
                foreach (var pair in ZombieContexts)
                {
                    if (pair.Key == victimEntity)
                    {
                        zombieContext = pair.Value;
                        break;
                    }
                }
            }

            if (zombieContext == null || !zombieContext.Works()) return;

            CitizenFX.Core.Player attackerPlayer = null;
            foreach (CitizenFX.Core.Player player in Players)
            {
                if (player.Character.Handle == attackerEntity)
                {
                    attackerPlayer = player;
                    break;
                }
            }

            if (attackerPlayer != null && attackerPlayer.IsAlive)
            {
                zombieContext.OnPlayerAttack(attackerPlayer);
            }
        }

        private static int ZombieInOwner()
        {
            int owner = 0;
            lock (ZombieContextsLock)
            {
                foreach (var pair in ZombieContexts)
                {
                    var context = pair.Value;
                    if (context.InOwnership())
                    {
                        owner++;
                    }
                }
            }

            return owner;
        }

        private static int ZombieCount()
        {
            lock (ZombieContextsLock)
            {
                return ZombieContexts.Count;
            }
        }

        private static bool SetSpawnEnabled(bool value)
        {
            bool success = false;

            lock (EnabledLock)
            {
                if (_spawnEnabled != value)
                {
                    _spawnEnabled = value;
                    success = true;
                }
            }

            return success;
        }

        private static bool inSpawnProccess = false;

        public static async Task<bool> SpawnZombie(Vector3 position)
        {
            inSpawnProccess = true;
            var zombieSettings = GetZombieSettings();
            var zombieHash = GetHashKey(zombieSettings.ZombieSkin);
            Model zombieModel = new Model(zombieHash);
            await zombieModel.Request(5000);
            if (zombieModel.IsLoaded)
            {
                // TriggerServerEvent("Server:ZombieSpawnManager:SpawnZombie", zombieSettings.ZombieSkin, position.X, position.Y, position.Z);
                // return true;

                Ped zombie = await World.CreatePed(zombieModel, position);
                if (zombie == null) return false;

                NetworkRegisterEntityAsNetworked(zombie.Handle);
                int netId = PedToNet(zombie.Handle);
                // Debug.WriteLine($"ZombieNetId: {netId}");

                // while (!NetworkGetEntityIsNetworked(zombie.Handle))
                // {
                //     Debug.WriteLine("!NetworkGetEntityIsNetworked");
                //     await Delay(10);
                // }

                SetEntityMaxHealth(zombie.Handle, zombieSettings.MaxHealth);
                SetEntityHealth(zombie.Handle, zombieSettings.MaxHealth);

                lock (ZombieContextsLock)
                {
                    var context = new ZombieContext(zombie, zombieSettings);
                    ZombieContexts.Add(zombie.Handle, context);
                    _ = context.Start();
                }

                zombie.State.Set("isZombie", "hux", true);
                zombie.MarkAsNoLongerNeeded();

                // var zombieNetId = NetworkGetEntityNetScriptId(zombie.Handle);
                // Debug.WriteLine($"ZombieNetId: {zombieNetId}");
                return true;
            }

            return false;
        }

        private static ZombieSettings GetZombieSettings()
        {
            int index = Random.Next(ClientConfig.ZombiePresets.Length);
            return ClientConfig.ZombiePresets[index];
        }


        [Tick]
        private async Task OnFindZombieTick()
        {
            // Debug.WriteLine($"Players count: {Players.Count()}");

            Ped[] allPeds = World.GetAllPeds();
            foreach (Ped ped in allPeds)
            {
                // bool isZombie = IsZombie(ped);
                // Debug.WriteLine($"IsZombie: {isZombie}");

                if (!IsPedAPlayer(ped.Handle) && ped.IsHuman && ped.IsAlive)
                {
                    lock (ZombieContextsLock)
                    {
                        if (ZombieContexts.ContainsKey(ped.Handle)) continue;

                        var context = new ZombieContext(ped);
                        ZombieContexts.Add(ped.Handle, context);

                        _ = context.Start();
                    }
                }
            }

            lock (ZombieContextsLock)
            {
                List<int> toDelete = new List<int>();
                foreach (var pair in ZombieContexts)
                {
                    var ped = pair.Key;
                    var context = pair.Value;
                    if (!context.Works())
                    {
                        toDelete.Add(ped);
                    }
                }

                foreach (var key in toDelete)
                {
                    ZombieContexts.Remove(key);
                }
            }

            await Delay(1000);
        }

        [Tick]
        private async Task OnSpawnTick()
        {
            if (_spawnEnabled)
            {
                int maxZombies = ClientConfig.MaxOwnZombies;
                var localPlayerPosition = Game.PlayerPed.Position;
                var nearestPlayers =
                    FindPlayersInRadius(localPlayerPosition, ClientConfig.ZombieNearestPlayerScanRadius);

                // Debug.WriteLine($"nearestPlayer: {nearestPlayers.Count}");

                if (nearestPlayers.Count > 0)
                {
                    maxZombies = nearestPlayers.Count * ClientConfig.MaxOwnZombies;
                }

                bool needSpawn = ZombieInOwner() < ClientConfig.MaxOwnZombies && ZombieCount() < maxZombies;

                if (needSpawn)
                {
                    Vector3 spawnPos = FindSafeSpawnLocation(Game.PlayerPed.Position,
                        ClientConfig.SpawnZombieMinRadius, ClientConfig.SpawnZombieMaxRadius);
                    bool success = await SpawnZombie(spawnPos);
                }
            }

            await Delay(ClientConfig.ZombieSpawnDelay);
        }

        private List<CitizenFX.Core.Player> FindPlayersInRadius(Vector3 root, float radius)
        {
            List<CitizenFX.Core.Player> playersInRaduis = new List<CitizenFX.Core.Player>();

            foreach (CitizenFX.Core.Player player in Players)
            {
                var ped = player.Character;
                if (ped == null) continue;

                float distance = Vector3.Distance(root, ped.Position);
                if (distance <= radius)
                {
                    playersInRaduis.Add(player);
                }
            }

            return playersInRaduis;
        }

        public static Vector3 FindSafeSpawnLocation(Vector3 origin, float minRadius, float maxRadius)
        {
            Vector3 spawnPos = origin + GetRandomOffset(minRadius, maxRadius);
            spawnPos.Z = World.GetGroundHeight(spawnPos);
            spawnPos.Z -= 0.9f;

            Vector3 savePos = spawnPos;

            bool found = GetSafeCoordForPed(spawnPos.X, spawnPos.Y, spawnPos.Z, true, ref savePos, 1); //16
            var resultPos = found ? savePos : spawnPos;

            return resultPos;
        }

        public static bool IsZombie(Ped ped)
        {
            return ped.State.Get("isZombie") as bool? ?? false;
        }

        private static Vector3 GetRandomOffset(float minRadius, float maxRadius)
        {
            float angle = (float)(Random.NextDouble() * Math.PI * 2);
            float distance = minRadius + (float)(Random.NextDouble() * (maxRadius - minRadius));

            var spawnPosition = new Vector3((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance, 0);
            return spawnPosition;
        }

        // private async Task UpdateControlledPeds(List<Ped> peds)
        // {
        //     if (peds.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     // Debug.WriteLine($"Players count: {Players.Count()}");
        //
        //     foreach (Ped ped in peds)
        //     {
        //         // Debug.WriteLine($"IsZombie {Groups.IsPedInZombieGroup(ped.Handle)}");
        //         if (ped.Exists() && ped.IsAlive && !ped.IsRagdoll && !ped.IsGettingUp && ped.IsHuman)
        //         {
        //             await Zombify(ped);
        //             await PlayAttackAnim(ped);
        //             // HasEntityClearLosToEntity(ped.Handle,)
        //         }
        //     }
        //     // await Delay(500)

        // private static async Task InitZombieAttributes(int zombiePed)
        // {
        //     RequestAnimSet(ZombieMoveSet);
        //     while (!HasAnimSetLoaded(ZombieMoveSet))
        //     {
        //         await Delay(1);
        //     }
        //
        //     SetPedMovementClipset(zombiePed, ZombieMoveSet, 1.0f);
        //
        //     Relationships.AddPedInZombieGroup(zombiePed);
        //     SetPedAsEnemy(zombiePed, true);
        //     SetPedCombatAttributes(zombiePed, 46, true);
        //     SetPedCombatAbility(zombiePed, 100);
        //     SetPedCombatMovement(zombiePed, 3);
        //     SetPedCombatRange(zombiePed, 1);
        //
        //     SetPedCanRagdollFromPlayerImpact(zombiePed, false); // Works
        //     SetPedEnableWeaponBlocking(zombiePed, true); //Works
        //     DisablePedPainAudio(zombiePed, true); //Works
        //     // SetPedFleeAttributes(zombiePed);
        //     StopPedSpeaking(zombiePed, true); // Works
        //     SetPedDiesWhenInjured(zombiePed, false); // Works
        //     StopPedRingtone(zombiePed); //Maybe dont works
        //     SetPedMute(zombiePed); // Works
        //     ApplyPedDamagePack(zombiePed, ZombieDamagePack, 0, ZombieDamagePackMultiplier);
        // }


        // private const int PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED = 100;
        // private const int PED_FLAG_IS_INJURED = 166;
        // private const int PED_FLAG_FORCE_DIRECT_ENTRY = 170;
        //
        // private const int ZombieHealth = 100;
        // private const float ZombieDistanceTargetToPlayer = 30;
        // private const float ZombieDistanceAttack = 1.3f;
        //
        // private const float ZombieRunPercentage = 0.75f;
        // private const float ZombieWalkSpeed = 1;
        // private const float ZombieRunSpeed = 2;
        //
        // private const string ZombieAttackDictionary = "melee@unarmed@streamed_core_fps";
        // private const string ZombieAttackAnim = "ground_attack_0_psycho";
        // private const string ZombieMoveSet = "move_m@drunk@verydrunk";
        // private const int ZombieDamageAmount = 20;
        //
        // private const bool ZombieCanRagdollByShots = true;
        // private const bool ZombieInstantDeathByHeadshot = true;
        //
        // private const bool ZombieDebug = true;
        //
        // private static readonly Random Random = new Random();


        // private async Task CheckPeds()
        // {
        //     int pedHandle = -1;
        //     int handle = FindFirstPed(ref pedHandle);
        //     bool success = (handle != -1);
        //
        //     int playerPedId = PlayerPedId();
        //     
        //     while (success)
        //     {
        //         await Delay(10);
        //         
        //         bool isPedHuman = IsPedHuman(pedHandle);
        //         bool isPedNonPlayer = !IsPedAPlayer(pedHandle);
        //         bool isPedAlive = !IsPedDeadOrDying(pedHandle, true);
        //         
        //         // if (isPedHuman && isPedNonPlayer && !isPedAlive && 
        //             // !GetPedConfigFlag(pedHandle, PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED, true))
        //         // {
        //             // if (GetPedSourceOfDeath(pedHandle) == playerPedId)
        //             // {
        //                 in this place can +exp
        //                 // SetPedConfigFlag(pedHandle, PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED, true);
        //             // }
        //         // }
        //         
        //         if (isPedHuman && isPedNonPlayer && isPedAlive)
        //         {
        //             // if (GetRelationshipBetweenPeds(pedHandle, playerPedId) != 0)
        //             // {
        //                 // ClearPedTasks(pedHandle);
        //                 // ClearPedSecondaryTask(pedHandle);
        //                 // ClearPedTasksImmediately(pedHandle);
        //                 // TaskWanderStandard(pedHandle, 10, 10); // walk anywhere
        //                 // SetEntityHealth(pedHandle, ZombieHealth);
        //                 // SetPedConfigFlag(pedHandle, PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED, false);
        //             // }
        //
        //             Vector3 playerCoords = GetEntityCoords(playerPedId, false);
        //             Vector3 pedsCoords = GetEntityCoords(pedHandle, false);
        //             float distance = GetDistanceBetweenCoords(
        //                 playerCoords.X, playerCoords.Y, playerCoords.Z, 
        //                 pedsCoords.X, pedsCoords.Y, pedsCoords.Z, true);
        //
        //             bool isPlayerAlive = GetEntityHealth(playerPedId) != 0;
        //             
        //             // follow the player
        //             if (distance <= ZombieDistanceTargetToPlayer &&
        //                 // !GetPedConfigFlag(pedHandle, PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED, false) &&
        //                 isPlayerAlive)
        //             {
        //                 // SetPedConfigFlag(pedHandle, PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED, true);
        //                 // ClearPedTasks(pedHandle);
        //
        //                 if (!IsPedMovingTowardsEntity(pedHandle, playerPedId))
        //                 {
        //                     float pedSpeed = GetRandomZombieSpeed();
        //                     TaskGoToEntity(pedHandle, playerPedId, -1, 0.0f, pedSpeed, 1073741824, 0);
        //                 }
        //             }
        //             
        //             if (distance <= ZombieDistanceAttack && !IsPedRagdoll(pedHandle) && !IsPedGettingUp(pedHandle))
        //             {
        //                 if (!isPlayerAlive)
        //                 {
        //                     // ClearPedTasks(pedHandle);
        //                     TaskWanderStandard(pedHandle, 10, 10);
        //                     // SetPedConfigFlag(pedHandle, PED_FLAG_HAS_DEAD_PED_BEEN_REPORTED, false);
        //                 }
        //                 else
        //                 {
        //                     RequestAnimSet(ZombieAttackDictionary);
        //                     while (!HasAnimSetLoaded(ZombieAttackDictionary))
        //                     {
        //                         await Delay(10);
        //                     }
        //                     
        //                     TaskPlayAnim(pedHandle, ZombieAttackDictionary, ZombieAttackAnim, 
        //                         8.0f, 1.0f, -1, 48, 0.001f, 
        //                         false, false, false);
        //                     ApplyDamageToPed(playerPedId, ZombieDamageAmount, false);
        //                 }
        //             }
        //             
        //             if (!NetworkGetEntityIsNetworked(pedHandle))
        //             {
        //                 DeletePed(ref pedHandle);
        //             }
        //             else
        //             {
        //                 ZombiePedAttributes(pedHandle);
        //             }
        //             
        //             RequestAnimSet(ZombieMoveSet);
        //             while (!HasAnimSetLoaded(ZombieMoveSet))
        //             {
        //                 await Delay(1);
        //             }
        //             SetPedMovementClipset(pedHandle, ZombieMoveSet, 1.0f);
        //
        //             if (ZombieDebug)
        //             {
        //                 World.DrawMarker(MarkerType.VerticalCylinder, pedsCoords + new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 2f), Color.FromArgb(255, 255, 255, 255));
        //             }
        //         }
        //         
        //         success = FindNextPed(handle, ref pedHandle);
        //     }
        //     
        //     EndFindPed(handle);
        //     await Delay(500);
        // }
        //
        // private float GetRandomZombieSpeed()
        // {
        //     return Random.NextDouble() < ZombieRunPercentage ? ZombieRunSpeed : ZombieWalkSpeed;
        // }
        //
        // private bool IsPedMovingTowardsEntity(int pedHandle, int targetEntity)
        // {
        //     int goToEntityTask = GetHashKey("TASK_GO_TO_ENTITY");
        //     return GetScriptTaskStatus(pedHandle, 0) == 1; // 1 = TASK_STATUS_PENDING
        // }

        // private void ZombiePedAttributes(int zombiePed)
        // {
        //     // if (!ZombieCanRagdollByShots) { SetPedRagdollBlockingFlags(zombiePed, 1); }
        //     SetPedCanRagdollFromPlayerImpact(zombiePed, false); // Works
        //     // SetPedSuffersCriticalHits(zombiePed, ZombieInstantDeathByHeadshot); //Works
        //     SetPedEnableWeaponBlocking(zombiePed, true); //Works
        //     DisablePedPainAudio(zombiePed, true); //Works
        //     StopPedSpeaking(zombiePed, true); // Works
        //     SetPedDiesWhenInjured(zombiePed, false); // Works
        //     StopPedRingtone(zombiePed); //Maybe dont works
        //     SetPedMute(zombiePed); // Works
        //     SetPedIsDrunk(zombiePed, true); //Maybe
        //     SetPedConfigFlag(zombiePed, PED_FLAG_IS_INJURED, false); // Maybe dont works
        //     SetPedConfigFlag(zombiePed, PED_FLAG_FORCE_DIRECT_ENTRY, false); // Maybe dont works
        //     SetBlockingOfNonTemporaryEvents(zombiePed, true); // Works
        //     SetPedCanEvasiveDive(zombiePed, false); // Works
        //     RemoveAllPedWeapons(zombiePed, true); // Works
        //     ApplyPedDamagePack(zombiePed, ZombieDamagePack, 0, ZombieDamagePackMultiplier);
        // }
    }
}