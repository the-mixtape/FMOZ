namespace OutbreakZCore.Client.Core.Zombie.States
{
    public class BaseState : IState
    {
        protected ZombieContextData ContextData { get; }

        public BaseState(ZombieContextData contextData)
        {
            ContextData = contextData;
        }

        public virtual void Enter()
        {
        }

        public virtual void Execute()
        {
        }

        public virtual void Exit()
        {
        }
    }
}