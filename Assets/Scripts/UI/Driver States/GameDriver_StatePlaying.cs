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
            GameData.GetInstance().PlayfieldUI.StopGame();
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            var gameState = _gameLogic.LogicUpdate(input);

            if (gameState != null)
            {
                GameData.GetInstance().PlayfieldUI.Set(gameState);

                if (gameState.GameOverCenter.HasValue)
                {
                    ui.StartRippleEffect(gameState.GameOverCenter.Value);
                    return DriverState.Gameover;
                }
            }

            return null;
        }
    }
}
