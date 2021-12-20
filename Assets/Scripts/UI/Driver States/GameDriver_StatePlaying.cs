using GM.Data;
using GM.Game;

namespace GM.UI
{
    public class GameDriver_StatePlaying : IDriverState
    {
        private GameLogic _gameLogic;

        public void OnStateEnter(DriverState from)
        {
            _gameLogic = GameData.GetInstance().GameLogic;
            _gameLogic.OnGameStart();

            GameData.GetInstance().PlayfieldUI.StartGame();
        }

        public void OnStateExit(DriverState to)
        {
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            var gameState = _gameLogic.LogicUpdate(input);

            if (gameState != null)
            {
                GameData.GetInstance().PlayfieldUI.Set(gameState);
            }

            return null;
        }
    }
}
