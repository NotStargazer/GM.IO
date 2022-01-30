using GM.Data;
using GM.Game;

namespace GM.UI
{
    public class GameDriver_StatePlaying : IDriverState
    {
        private BGM _currentBGM;

        private GameLogic _gameLogic;

        public void OnStateEnter(DriverState from)
        {
            _gameLogic = GameData.GetInstance().GameLogic;
            var state = _gameLogic.OnGameStart();

            GameData.GetInstance().PlayfieldUI.StartGame();
            GlobalResources.GetInstance().SoundController.StartMusic(state.ProgressionAssets.Value.Music);
        }

        public void OnStateExit(DriverState to)
        {
            GameData.GetInstance().PlayfieldUI.StopGame();
            GlobalResources.GetInstance().SoundController.StopMusic();
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            var gameState = _gameLogic.LogicUpdate(input);

            if (gameState != null)
            {
                var playfieldUI = GameData.GetInstance().PlayfieldUI;
                playfieldUI.Set(gameState);

                if (gameState.GameOverCenter.HasValue)
                {
                    ui.StartRippleEffect(gameState.GameOverCenter.Value);
                    return DriverState.Gameover;
                }

                if (gameState.ProgressionAssets.HasValue)
                {
                    var assets = gameState.ProgressionAssets.Value;
                    if (assets.Music != _currentBGM)
                    {
                        _currentBGM = assets.Music;
                        GlobalResources.GetInstance().SoundController.SwitchMusic(assets.Music);
                    }
                }

                if (gameState.LinesCleared.Count > 0)
                {
                    playfieldUI.VisualEffectController.PlayLineBreakVFX(gameState.LinesCleared);
                }
            }

            return null;
        }
    }
}
