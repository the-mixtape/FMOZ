using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace OutbreakZCore.Client.Core.Zombie.States
{
    public class WanderingState : BaseState
    {
        public WanderingState(ZombieContextData contextData) : base(contextData)
        {
        }
        
        public override void Enter()
        {
            Ped zombie = ContextData.Zombie;
            if (zombie == null) return;

            var position = zombie.Position;
            API.TaskWanderInArea(zombie.Handle, position.X, position.Y, position.Z, 10.0f, 1.0f, 1.0f);
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
        }
    }
}