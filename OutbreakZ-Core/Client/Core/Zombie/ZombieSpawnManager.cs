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
        class ZombieSettings
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

            private const int ContextTickDelay = 300;

            private const float AttackAngleThreshold = 50.0f;
            private const int ZombieDamage = 9;
            private const float ZombieDistanceToRun = 30.0f;
            private const float ZombieDistanceToMelee = 2.5f;
            private const float ZombieDistanceToTakeDamage = 1.3f;
            private const int DelayBetweenAttacks = 2500;

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
                _zombie.Task.StandStill(-1);

                bool enable;
                lock (_enableLock)
                {
                    enable = _enable;
                }

                while (enable)
                {
                    await ZombieContextTick();
                    await Delay(ContextTickDelay);

                    lock (_enableLock)
                    {
                        enable = _enable;
                    }
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


            public bool Works()
            {
                bool enable;
                lock (_enableLock)
                {
                    enable = _enable;
                }

                return enable;
            }

            private async Task ZombieContextTick()
            {
                if (!_zombie.Exists() && _zombie.IsDead)
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

                bool isOwned = NetworkGetEntityOwner(_zombie.Handle) == PlayerId();
                if (isOwned)
                {
                    // idle
                    if (nearestPlayer == null)
                    {
                        TaskStandStill(_zombie.Handle, -1);
                    }
                    else
                    {
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
                }

                if (distanceToTarget < ZombieDistanceToMelee && 
                    nearestPlayer != null && !(nearestPlayer.Character.IsRagdoll || nearestPlayer.Character.IsGettingUp) &&
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
                            if (distanceToTarget < ZombieDistanceToTakeDamage && attackAngle < AttackAngleThreshold)
                            {
                                Vector3 force = new Vector3(0.5f, 0.5f, 0.5f);
                                KnockbackPed(nearestPlayer.Character.Handle, force);
                                
                                var targetServerId = GetPlayerServerId(nearestPlayer.Handle);
                                TriggerServerEvent("ServerPlayer:DealDamageToPlayer", targetServerId, ZombieDamage);
                            }
                            
                        }
                    }
                }
            }
            
            void KnockbackPed(int targetPed, Vector3 force)
            {
                SetPedToRagdoll(targetPed, 350, 750, 0, false, false, false);
                ApplyForceToEntity(targetPed, 1, force.X, force.Y, force.Z, 0, 0, 0, 0, false, true, true, false, true);
            }

            private async Task Zombify()
            {
                RequestAnimSet(ZombieMoveSet);
                while (!HasAnimSetLoaded(ZombieMoveSet))
                {
                    await Delay(1);
                }

                SetPedMovementClipset(_zombie.Handle, ZombieMoveSet, 1.0f);

                StopPedSpeaking(_zombie.Handle, true);
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
                RequestAnimSet(ZombieAttackDictionary);
                while (!HasAnimSetLoaded(ZombieAttackDictionary))
                {
                    await Delay(10);
                }
            
                TaskPlayAnim(entity, ZombieAttackDictionary, ZombieAttackAnim,
                    8.0f, 1.0f, -1, 48, 0.001f,
                    false, false, false);
            }
        }

        // Dictionary<handle, context>
        private static readonly Dictionary<int, ZombieContext> LogicContexts = new Dictionary<int, ZombieContext>();
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

        public ZombieSpawnManager()
        {
        }

        public static async Task SpawnZombie(Vector3 position)
        {
            position.Z = World.GetGroundHeight(position);
            position.Z -= 0.9f;

            var zombieSettings = GetZombieSettings();

            var zombieHash = GetHashKey(zombieSettings.ZombieSkin);
            Model zombieModel = new Model(zombieHash);
            await zombieModel.Request(5000);
            if (zombieModel.IsLoaded)
            {
                Ped zombie = await World.CreatePed(zombieModel, position);
                SetEntityMaxHealth(zombie.Handle, zombieSettings.MaxHealth);
                SetEntityHealth(zombie.Handle, zombieSettings.MaxHealth);

                var context = new ZombieContext(zombie, zombieSettings);
                lock (ZombieContextsLock)
                {
                    LogicContexts.Add(zombie.Handle, context);
                }

                _ = context.Start();

                zombie.MarkAsNoLongerNeeded();

                // TaskWanderStandard(zombie.Handle, 10, 10); // walk anywhere

                // TaskWanderInArea(zombie.Handle, position.X, position.Y, position.Z, 50, 1, 0);
                // zombie.Task.GoTo(Game.Player.Character.Position, true);
            }
        }

        private static ZombieSettings GetZombieSettings()
        {
            int index = Random.Next(ZombiePresets.Length);
            return ZombiePresets[index];
        }


        [Tick]
        private async Task OnTick()
        {
            Ped[] allPeds = World.GetAllPeds();
            foreach (Ped ped in allPeds)
            {
                if (!IsPedAPlayer(ped.Handle) && ped.IsHuman)
                {
                    lock (ZombieContextsLock)
                    {
                        if (LogicContexts.ContainsKey(ped.Handle)) continue;

                        var context = new ZombieContext(ped);
                        LogicContexts.Add(ped.Handle, context);

                        _ = context.Start();
                    }
                }
            }

            lock (ZombieContextsLock)
            {
                List<int> toDelete = new List<int>();
                foreach (var pair in LogicContexts)
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
                    LogicContexts.Remove(key);
                }
            }

            await Delay(1000);
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