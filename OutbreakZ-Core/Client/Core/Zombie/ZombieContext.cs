using System;
using System.Drawing;
using System.Threading.Tasks;
using OutbreakZCore.Client.Config;
using CitizenFX.Core;
using OutbreakZCore.Client.Core.Zombie.States;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Zombie
{
    public class ZombieContext : BaseScript
    {
        private int _lastAttackTime;
        private StateMachine StateMachine { get; set; }
        private WanderingState WanderingState { get; set; }
        private ChasingState ChasingState { get; set; }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine();
            WanderingState = new WanderingState(_contextData);
            ChasingState = new ChasingState(_contextData);
        }

        private readonly ZombieContextData _contextData;

        private bool _enable = true;
        private readonly object _enableLock = new object();

        private bool GetEnable()
        {
            lock (_enableLock)
            {
                return _enable;
            }
        }

        public int GetEntity()
        {
            return Zombie.Handle;
        }
        
        // public void OnPlayerAttack(CitizenFX.Core.Player player)
        // {
        //     if (!InOwnership()) return;
        //     if (_contextData.Target == player) return;
        //     StateMachine.SetState(null);
        //     _contextData.Target = player;
        //     StateMachine.SetState(ChasingState);
        // }

        public ZombieContext(Ped zombie)
        {
            var defaultSettings = new ZombieSettings("", 400, 1.0f, 2.0f);
            _contextData = new ZombieContextData(zombie, defaultSettings);
            BeginPlay();
        }

        public ZombieContext(Ped zombie, ZombieSettings settings)
        {
            _contextData = new ZombieContextData(zombie, settings);
            BeginPlay();
        }

        private void BeginPlay()
        {
            InitializeStateMachine();
            SubscribeHandles();
        }

        private void SubscribeHandles()
        {
        }

        private Ped Zombie => _contextData.Zombie;
        private ZombieSettings Settings => _contextData.ZombieSettings;

        public async Task Start()
        {
            _ = OnZombieDebug();

            while (GetEnable())
            {
                await ZombieContextTick();
                await Delay(ClientConfig.ZombieContextTickDelay);
            }
        }

        private async Task Finish()
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
                if (ZombieSpawnManager.Variables.ZombieDebug)
                {
                    var ownerColor = Color.FromArgb(255, 0, 255, 0);
                    var notOwnerColor = Color.FromArgb(255, 255, 0, 0);
                    var markerColor = InOwnership() ? ownerColor : notOwnerColor;

                    var zombiePos = Zombie.Position;
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
            return NetworkGetEntityOwner(Zombie.Handle) == PlayerId();
        }

        private async Task ZombieContextTick()
        {
            if (!Zombie.Exists() || Zombie.IsDead)
            {
                await Finish();
                return;
            }

            await Zombify();

            if (Zombie.IsRagdoll || Zombie.IsGettingUp)
            {
                return;
            }
            
            if (InOwnership())
            {
                if (_contextData.Target == null)
                {
                    _contextData.Target = await FindTargetPlayer();
                }
                
                if (_contextData.Target == null)
                {
                    StateMachine.SetState(WanderingState);
                }
                else
                {
                    StateMachine.SetState(ChasingState);
                }

                StateMachine.Update();
            }

            var nearestPlayer = await FindPlayerForMelee();
            if (nearestPlayer == null) return;

            var distanceToTarget = Vector3.Distance(nearestPlayer.Character.Position, Zombie.Position);
            if (distanceToTarget < ClientConfig.ZombieDistanceToMelee &&
                !(nearestPlayer.Character.IsRagdoll || nearestPlayer.Character.IsGettingUp) &&
                !(Zombie.IsRagdoll || Zombie.IsGettingUp))
            {
                var currentTime = GetGameTimer();
                if (currentTime - _lastAttackTime > ClientConfig.ZombieDelayBetweenAttacks)
                {
                    await PlayAttackAnim(Zombie.Handle);
                    _lastAttackTime = currentTime;

                    await Delay(500);

                    if (InOwnership())
                    {
                        var targetPosition = nearestPlayer.Character.Position;
                        var zombiePosition = Zombie.Position;

                        var zombieForward = GetEntityForwardVector(Zombie.Handle);
                        var delta = targetPosition - zombiePosition;
                        delta.Normalize();
                        float dot = zombieForward.X * delta.X + zombieForward.Y * delta.Y;
                        var attackAngle = Math.Acos(dot) * (180.0f / Math.PI);

                        distanceToTarget = Vector3.Distance(zombiePosition, targetPosition);
                        if (!CheckPedInCombat(nearestPlayer.Character.Handle) &&
                            distanceToTarget < ClientConfig.ZombieDistanceToTakeDamage &&
                            attackAngle < ClientConfig.ZombieAttackAngleThreshold)
                        {
                            Vector3 force = new Vector3(0.5f, 0.5f, 0.0f);
                            KnockbackPed(nearestPlayer.Character.Handle, force);

                            var targetServerId = GetPlayerServerId(nearestPlayer.Handle);
                            TriggerServerEvent("Player.DealDamageToPlayer", targetServerId,
                                ClientConfig.ZombieDamage);
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
            if (!HasAnimSetLoaded(ClientConfig.ZombieMoveSet))
            {
                RequestAnimSet(ClientConfig.ZombieMoveSet);
                while (!HasAnimSetLoaded(ClientConfig.ZombieMoveSet))
                {
                    await Delay(10);
                }
            }

            SetPedMovementClipset(Zombie.Handle, ClientConfig.ZombieMoveSet, 1.0f);

            StopPedSpeaking(Zombie.Handle, true);
            SetPedConfigFlag(Zombie.Handle, 104, true);
            StopCurrentPlayingSpeech(Zombie.Handle);
            StopCurrentPlayingAmbientSpeech(Zombie.Handle);
            DisablePedPainAudio(Zombie.Handle, true);
            SetBlockingOfNonTemporaryEvents(Zombie.Handle, true);
            SetPedFleeAttributes(Zombie.Handle, 0, false);
            SetPedCombatAttributes(Zombie.Handle, 46, true);
            StopPedRingtone(Zombie.Handle);

            Groups.AddPedInZombieGroup(Zombie.Handle);

            ApplyPedDamagePack(Zombie.Handle, "BigHitByVehicle", 0, 9);
            ApplyPedDamagePack(Zombie.Handle, "SCR_Dumpster", 0, 9);
            ApplyPedDamagePack(Zombie.Handle, "SCR_Torture", 0, 9);
        }


        private Task<CitizenFX.Core.Player> FindPlayerForMelee()
        {
            CitizenFX.Core.Player nearestPlayer = null;
            var minDistance = float.MaxValue;

            foreach (var player in Players)
            {
                if (Zombie.Handle == player.Handle)
                    continue;

                float distance = Vector3.Distance(player.Character.Position, Zombie.Position);
                if (distance < ClientConfig.ZombieDistanceToMelee && distance < minDistance)
                {
                    nearestPlayer = player;
                    minDistance = distance;
                }
            }

            return Task.FromResult(nearestPlayer);
        }

        private Task<CitizenFX.Core.Player> FindTargetPlayer()
        {
            CitizenFX.Core.Player nearestPlayer = null;
            float minDistance = float.MaxValue;

            foreach (var player in Players)
            {
                if (Zombie.Handle == player.Handle)
                    continue;

                float distance = Vector3.Distance(player.Character.Position, Zombie.Position);

                if (distance < minDistance)
                {
                    if (distance <= ClientConfig.ZombieDistanceToTargetSeeReason)
                    {
                        float angleBetweenZombieAndPlayer =
                            GetAngleBetweenEntities(Zombie.Handle, player.Character.Handle);
                        if (angleBetweenZombieAndPlayer < ClientConfig.ZombieSeeAngle &&
                            HasEntityClearLosToEntity(Zombie.Handle, player.Character.Handle, 17))
                        {
                            minDistance = distance;
                            nearestPlayer = player;
                        }
                    }
                    else if (distance <= ClientConfig.ZombieDistanceToTargetCloselyReason)
                    {
                        minDistance = distance;
                        nearestPlayer = player;
                    }
                }
            }

            return Task.FromResult(nearestPlayer);
        }

        private static float GetAngleBetweenEntities(int ped1, int ped2)
        {
            Vector3 ped1Forward = GetEntityForwardVector(ped1);
            var ped1Position = GetEntityCoords(ped1, true);
            var ped2Position = GetEntityCoords(ped2, true);

            Vector3 toTarget = ped2Position - ped1Position;
            toTarget.Normalize();

            float dotProduct = ped1Forward.X * toTarget.X + ped1Forward.Y * toTarget.Y;
            float angleRadians = (float)Math.Acos(dotProduct);
            return angleRadians * (180f / (float)Math.PI);
        }

        private async Task PlayAttackAnim(int entity)
        {
            if (!HasAnimSetLoaded(ClientConfig.ZombieAttackDictionary))
            {
                RequestAnimSet(ClientConfig.ZombieAttackDictionary);
                while (!HasAnimSetLoaded(ClientConfig.ZombieAttackDictionary))
                {
                    await Delay(10);
                }
            }

            TaskPlayAnim(entity, ClientConfig.ZombieAttackDictionary, ClientConfig.ZombieAttackAnim,
                8.0f, 1.0f, -1, 48, 0.001f,
                false, false, false);
        }
    }
}