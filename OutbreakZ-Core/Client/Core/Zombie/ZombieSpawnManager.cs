using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Mono.CSharp;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
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

        public class ZombieSettings
        {
            public string ZombieSkin { get; set; }
            public float WalkSpeed { get; set; }
            public float RunSpeed { get; set; }
            public int MaxHealth { get; set; }

            public ZombieSettings(string skin, int maxHealth, float walkSpeed, float runSpeed)
            {
                ZombieSkin = skin;
                MaxHealth = maxHealth;
                WalkSpeed = walkSpeed;
                RunSpeed = runSpeed;
            }
        }

        class ZombieContext : BaseScript
        {
            private readonly Ped _zombie;
            private readonly ZombieSettings _settings;

            private bool _enable = true;
            private readonly object _enableLock = new object();

            private bool GetEnable()
            {
                lock (_enableLock)
                {
                    return _enable;
                }
            }

            private const int ContextTickDelay = 300;

            private const float AttackAngleThreshold = 50.0f;
            private const int ZombieDamage = 3;
            private const float ZombieDistanceToRun = 30.0f;
            private const float ZombieDistanceToMelee = 2.5f;
            private const float ZombieDistanceToTakeDamage = 1.3f;
            private const int DelayBetweenAttacks = 3000;

            private const string ZombieMoveSet = "move_m@drunk@verydrunk";
            private const string ZombieAttackDictionary = "melee@unarmed@streamed_core_fps";
            private const string ZombieAttackAnim = "ground_attack_0_psycho";

            private bool _inRunState = false;
            private int _lastAttackTime = 0;

            public ZombieContext(Ped zombie)
            {
                _zombie = zombie;
                _settings = new ZombieSettings("", 400, 1.0f, 2.0f);
            }

            public ZombieContext(Ped zombie, ZombieSettings settings)
            {
                _zombie = zombie;
                _settings = settings;
            }

            public async Task Start()
            {
                _ = OnZombieDebug();
                _zombie.Task.StandStill(-1);

                while (GetEnable())
                {
                    await ZombieContextTick();
                    await Delay(ContextTickDelay);
                }
            }

            public async Task Finish()
            {
                lock (_enableLock)
                {
                    _enable = false;
                }

                await Task.FromResult(0);
            }

            private async Task OnZombieDebug()
            {
                while (GetEnable())
                {
                    if (Variables.ZombieDebug)
                    {
                        var ownerColor = Color.FromArgb(255, 0, 255, 0);
                        var notOwnerColor = Color.FromArgb(255, 255, 0, 0);
                        var markerColor = InOwnership() ? ownerColor : notOwnerColor;

                        var zombiePos = _zombie.Position;
                        World.DrawMarker(
                            MarkerType.VerticalCylinder,
                            zombiePos + new Vector3(0, 0, -1),
                            new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 2f),
                            markerColor);
                        await Delay(0);
                    }
                    else
                    {
                        await Delay(1000);
                    }
                }
            }


            public bool Works()
            {
                bool enable;
                lock (_enableLock)
                {
                    enable = _enable;
                }

                return enable;
            }

            public bool InOwnership()
            {
                return NetworkGetEntityOwner(_zombie.Handle) == PlayerId();
            }

            private async Task ZombieContextTick()
            {
                if (!_zombie.Exists() || _zombie.IsDead)
                {
                    await Finish();
                    return;
                }

                await Zombify();

                if (_zombie.IsRagdoll || _zombie.IsGettingUp)
                {
                    return;
                }

                var nearestPlayer = await FindNearestPlayer();
                float distanceToTarget = 0;
                if (nearestPlayer != null)
                {
                    distanceToTarget = Vector3.Distance(nearestPlayer.Character.Position, _zombie.Position);
                }

                bool isOwned = InOwnership();
                if (isOwned)
                {
                    // idle
                    if (nearestPlayer == null)
                    {
                        TaskStandStill(_zombie.Handle, -1);
                        return;
                    }

                    float speed = _settings.WalkSpeed;
                    if (_inRunState)
                    {
                        speed = _settings.RunSpeed;
                    }
                    else
                    {
                        if (distanceToTarget < ZombieDistanceToRun &&
                            HasEntityClearLosToEntity(_zombie.Handle, nearestPlayer.Character.Handle, 17))
                        {
                            _inRunState = true;
                            speed = _settings.RunSpeed;
                        }
                    }

                    TaskGoToEntity(
                        _zombie.Handle, nearestPlayer.Character.Handle,
                        -1, 0.0f, speed, 1073741824, 0
                    );
                }

                if (nearestPlayer == null) return;

                if (distanceToTarget < ZombieDistanceToMelee &&
                    !(nearestPlayer.Character.IsRagdoll || nearestPlayer.Character.IsGettingUp) &&
                    !(_zombie.IsRagdoll || _zombie.IsGettingUp))
                {
                    int currentTime = GetGameTimer();
                    if (currentTime - _lastAttackTime > DelayBetweenAttacks)
                    {
                        await PlayAttackAnim(_zombie.Handle);
                        _lastAttackTime = currentTime;

                        await Delay(500);

                        if (isOwned)
                        {
                            var targetPosition = nearestPlayer.Character.Position;
                            var zombiePosition = _zombie.Position;

                            var zombieForward = GetEntityForwardVector(_zombie.Handle);
                            var delta = targetPosition - zombiePosition;
                            delta.Normalize();
                            float dot = zombieForward.X * delta.X + zombieForward.Y * delta.Y;
                            var attackAngle = Math.Acos(dot) * (180.0f / Math.PI);

                            distanceToTarget = Vector3.Distance(zombiePosition, targetPosition);
                            if (!CheckPedInCombat(nearestPlayer.Character.Handle) &&
                                distanceToTarget < ZombieDistanceToTakeDamage && attackAngle < AttackAngleThreshold)
                            {
                                Vector3 force = new Vector3(0.5f, 0.5f, 0.0f);
                                KnockbackPed(nearestPlayer.Character.Handle, force);

                                var targetServerId = GetPlayerServerId(nearestPlayer.Handle);
                                TriggerServerEvent("ServerPlayer:DealDamageToPlayer", targetServerId, ZombieDamage);
                            }
                        }
                    }
                }
            }

            private bool CheckPedInCombat(int pedId)
            {
                return GetIsTaskActive(pedId, 130) || /* CTaskMeleeActionResult  */
                       GetIsTaskActive(pedId, 3); /* CTaskCombatRoll */
            }

            void KnockbackPed(int targetPed, Vector3 force)
            {
                SetPedToRagdoll(targetPed, 350, 500, 0, false, false, false);
                ApplyForceToEntity(targetPed, 1, force.X, force.Y, force.Z, 0, 0, 0, 0, false, true, true, false, true);
            }

            private async Task Zombify()
            {
                if (!HasAnimSetLoaded(ZombieMoveSet))
                {
                    RequestAnimSet(ZombieMoveSet);
                    while (!HasAnimSetLoaded(ZombieMoveSet))
                    {
                        await Delay(10);
                    }
                }

                // var isZombie = _zombie.State.Get("isZombie");
                // Debug.WriteLine($"isZombie: {isZombie}");

                // int netId = NetworkGetNetworkIdFromEntity(_zombie.Handle);
                // Debug.WriteLine($"netId: {netId}");

                SetPedMovementClipset(_zombie.Handle, ZombieMoveSet, 1.0f);

                StopPedSpeaking(_zombie.Handle, true);
                SetPedConfigFlag(_zombie.Handle, 104, true);
                StopCurrentPlayingSpeech(_zombie.Handle);
                StopCurrentPlayingAmbientSpeech(_zombie.Handle);
                DisablePedPainAudio(_zombie.Handle, true);
                SetBlockingOfNonTemporaryEvents(_zombie.Handle, true);
                SetPedFleeAttributes(_zombie.Handle, 0, false);
                SetPedCombatAttributes(_zombie.Handle, 46, true);
                StopPedRingtone(_zombie.Handle);

                Groups.AddPedInZombieGroup(_zombie.Handle);

                ApplyPedDamagePack(_zombie.Handle, "BigHitByVehicle", 0, 9);
                ApplyPedDamagePack(_zombie.Handle, "SCR_Dumpster", 0, 9);
                ApplyPedDamagePack(_zombie.Handle, "SCR_Torture", 0, 9);
            }

            private Task<CitizenFX.Core.Player> FindNearestPlayer()
            {
                CitizenFX.Core.Player nearestPlayer = null;
                float minDistance = float.MaxValue;

                foreach (var player in Players)
                {
                    if (_zombie.Handle == player.Handle)
                        continue;

                    float distance = Vector3.Distance(player.Character.Position, _zombie.Position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPlayer = player;
                    }
                }

                // if (nearestPlayer != null)
                // {
                // Debug.WriteLine($"Nearest Player: {nearestPlayer.Name}, distance ({minDistance})");
                // }

                return Task.FromResult(nearestPlayer);
            }

            private async Task PlayAttackAnim(int entity)
            {
                if (!HasAnimSetLoaded(ZombieAttackDictionary))
                {
                    RequestAnimSet(ZombieAttackDictionary);
                    while (!HasAnimSetLoaded(ZombieAttackDictionary))
                    {
                        await Delay(10);
                    }
                }

                TaskPlayAnim(entity, ZombieAttackDictionary, ZombieAttackAnim,
                    8.0f, 1.0f, -1, 48, 0.001f,
                    false, false, false);
            }
        }

        // private const string IsZombieStateName = "isZombie";

        // Dictionary<handle, context>
        private static readonly Dictionary<int, ZombieContext> ZombieContexts = new Dictionary<int, ZombieContext>();
        private static readonly object ZombieContextsLock = new object();

        private static readonly ZombieSettings[] ZombiePresets = new ZombieSettings[]
        {
            // male
            new ZombieSettings("a_m_m_hillbilly_01", 500, 0.5f, 1.5f),
            new ZombieSettings("a_m_m_hillbilly_02", 500, 0.5f, 1.5f),
            new ZombieSettings("a_m_m_skidrow_01", 400, 0.4f, 1.4f),
            new ZombieSettings("a_m_m_tramp_01", 450, 0.45f, 1.45f),
            new ZombieSettings("a_m_m_trampbeac_01", 420, 0.42f, 1.42f),
            new ZombieSettings("a_m_m_acult_01", 600, 0.6f, 1.6f),
            new ZombieSettings("a_m_m_og_boss_01", 700, 0.65f, 1.7f),
            new ZombieSettings("a_m_m_soucent_03", 480, 0.48f, 1.48f),
            new ZombieSettings("a_m_m_hasjew_01", 460, 0.46f, 1.46f),
            new ZombieSettings("a_m_m_tranvest_01", 500, 0.5f, 1.5f),
            new ZombieSettings("a_m_y_methhead_01", 550, 0.55f, 1.55f),
            new ZombieSettings("a_m_y_hippy_01", 520, 0.52f, 1.52f),
            new ZombieSettings("a_m_y_skater_01", 530, 0.53f, 1.53f),
            new ZombieSettings("a_m_y_juggalo_01", 580, 0.58f, 1.58f),
            new ZombieSettings("a_m_y_soucent_04", 490, 0.49f, 1.49f),
            new ZombieSettings("u_m_m_prolsec_01", 600, 0.6f, 1.6f),
            new ZombieSettings("a_m_o_acult_01", 620, 0.62f, 1.62f),
            new ZombieSettings("a_m_o_acult_02", 630, 0.63f, 1.63f),
            new ZombieSettings("a_m_y_acult_01", 610, 0.61f, 1.61f),
            new ZombieSettings("a_m_y_acult_02", 590, 0.59f, 1.59f),

            // female
            new ZombieSettings("a_f_m_trampbeac_01", 400, 0.4f, 1.4f),
            new ZombieSettings("a_f_m_tramp_01", 420, 0.42f, 1.42f),
            new ZombieSettings("a_f_m_skidrow_01", 430, 0.43f, 1.43f),
            new ZombieSettings("a_f_y_hippie_01", 450, 0.45f, 1.45f),
            new ZombieSettings("a_f_y_skater_01", 470, 0.47f, 1.47f)
        };

        private static readonly Random Random = new Random();

        private const int MaxOwnZombies = 5;

        // private const int MaxZombies = 20;
        private const int SpawnDelay = 1500;
        private const float SpawnMinRadius = 30.0f;
        private const float SpawnMaxRadius = 60.0f;

        private const float NearestPlayerScanRadius = 200.0f;

        private static bool _spawnEnabled;
        private static readonly object EnabledLock = new object();

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
                $"Spawner: {spawnerStatus} | Zombies: {zombieCount} | Owner: {owner}/{MaxOwnZombies} | Debug: {debug}";
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
                Debug.WriteLine($"ZombieNetId: {netId}");

                while (!NetworkGetEntityIsNetworked(zombie.Handle))
                {
                    Debug.WriteLine("!NetworkGetEntityIsNetworked");
                    await Delay(10);
                }

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

                var zombieNetId = NetworkGetEntityNetScriptId(zombie.Handle);
                Debug.WriteLine($"ZombieNetId: {zombieNetId}");
                return true;
            }

            return false;
        }

        private static ZombieSettings GetZombieSettings()
        {
            int index = Random.Next(ZombiePresets.Length);
            return ZombiePresets[index];
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
                int maxZombies = MaxOwnZombies;
                var localPlayerPosition = Game.PlayerPed.Position;
                var nearestPlayers = FindPlayersInRadius(localPlayerPosition, NearestPlayerScanRadius);

                Debug.WriteLine($"nearestPlayer: {nearestPlayers.Count}");

                if (nearestPlayers.Count > 0)
                {
                    maxZombies = nearestPlayers.Count * MaxOwnZombies;
                }

                bool needSpawn = ZombieInOwner() < MaxOwnZombies && ZombieCount() < maxZombies;

                if (needSpawn)
                {
                    Vector3 spawnPos = FindSafeSpawnLocation(Game.PlayerPed.Position, SpawnMinRadius, SpawnMaxRadius);
                    bool success = await SpawnZombie(spawnPos);
                }
            }

            await Delay(SpawnDelay);
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