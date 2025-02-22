using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace OutbreakZCore.Client.Core.Zombie.States
{
    public class ChasingState : BaseState
    {
        private readonly Random Rand = new Random();
        private bool _goToEntityTask;
        private int _goToPositionTicks;
        private const int GoToPositionMaxTicks = 5;

        public ChasingState(ZombieContextData contextData) : base(contextData)
        {
        }

        public override void Enter()
        {
            Ped zombie = ContextData.Zombie;
            if (zombie == null) return;
            ChaseTarget();
        }

        public override void Execute()
        {
            if (_goToEntityTask)
            {
                if (ContextData.Zombie.IsStopped)
                {
                    GoToAroundTarget();
                }
            }
            else
            {
                if (_goToPositionTicks > GoToPositionMaxTicks)
                {
                    _goToPositionTicks = 0;
                    ChaseTarget();
                }
                else
                {
                    _goToPositionTicks++;   
                }
            }
        }

        public override void Exit()
        {
        }

        private void ChaseTarget()
        {
            _goToEntityTask = true;
            API.TaskGoToEntity(ContextData.Zombie.Handle, ContextData.Target.Character.Handle,
                -1, 0.0f, ContextData.ZombieSettings.RunSpeed, 1073741824, 0);
        }

        private void GoToAroundTarget()
        {
            _goToEntityTask = false;
            Vector3 targetPos = ContextData.Target.Character.Position + GetRandomOffset();
            API.TaskGoStraightToCoord(ContextData.Zombie.Handle, targetPos.X, targetPos.Y, targetPos.Z, 
                ContextData.ZombieSettings.RunSpeed, -1, 0f, 0f);
        }

        private Vector3 GetRandomOffset()
        {
            float offsetX = (float)(Rand.NextDouble() * 6 - 3);
            float offsetY = (float)(Rand.NextDouble() * 6 - 3);
            return new Vector3(offsetX, offsetY, 0);
        }
    }
}