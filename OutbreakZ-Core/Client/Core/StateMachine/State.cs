namespace OutbreakZCore.Client.Core
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }
}