using System.Collections.Generic;
using GM.Data;

namespace GM.UI
{
    public enum DriverState
    {
        GameStart,
        Playing,
        Pregame,
        Gameover,
        Results,
    }

    public interface IDriverState
    {
        public void OnStateEnter(DriverState from);
        public void OnStateExit(DriverState to);
        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input);
    }

    public class GameDriver
    {
        private UIRoot _uiRoot;

        private Dictionary<DriverState, IDriverState> _states;

        private DriverState? _currentDriverState;

        internal GameDriver(UIRoot ui)
        {
            _uiRoot = ui;

            //Create States
            //Planned States

            _states = new Dictionary<DriverState, IDriverState>
            {
                {DriverState.Playing, new GameDriver_StatePlaying()},
            };
            /*
             * State Results
             * State Pregame
             * State Gameover
             */

            //Load initial state
        }

        internal void OnReceiveInputsWithUI(IInput input)
        {
            if (!_currentDriverState.HasValue)
            {
                if (GameData.HasInstance)
                {
                     _currentDriverState = GameData.GetInstance().InitialState;
                     _states[_currentDriverState.Value].OnStateEnter(DriverState.GameStart);
                }
                else
                {
                    return;
                }
            }

            var currentState = _currentDriverState.Value;
            var newState = _states[currentState].OnReceiveInputsWithUI(_uiRoot, input);

            if (newState.HasValue)
            {
                var nextState = newState.Value;
                var nextDriverState = _states[nextState];
                var currentDriverState = _states[currentState];

                currentDriverState.OnStateExit(nextState);
                nextDriverState.OnStateEnter(currentState);

                _currentDriverState = nextState;
            }
        }
    }
}

