using CitizenFX.Core;

namespace OutbreakZCore.Client.Core
{
    public class StateMachine
    {
        private IState _currentState;
        
        public void SetState(IState newState)
        {
            if(_currentState == newState) return;
            
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void Update()
        {
            _currentState?.Execute();
        }
    }
}