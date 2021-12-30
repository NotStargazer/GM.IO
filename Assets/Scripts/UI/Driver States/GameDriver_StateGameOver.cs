using GM.Data;
using GM.Game;
using UnityEngine;

namespace GM.UI
{
    public class GameDriver_StateGameOver : IDriverState
    {
        private GameLogic _gameLogic;

        public void OnStateEnter(DriverState from)
        {
            Debug.Log("Game Is Over");
            _gameLogic = GameData.GetInstance().GameLogic;
        }

        public void OnStateExit(DriverState to)
        {
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            _gameLogic.Render();

            return null;
        }
    }
}
