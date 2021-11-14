using GM.Data;
using GM.Game;
using UnityEngine;

namespace GM.UI
{
    public interface IUIRoot
    {

    }

    public class UIRoot : MonoBehaviour, IUIRoot
    {
        private GameDriver _gameDriver;
        private GameData _gameData;

        public void OnStartForGame()
        {
            _gameDriver = new GameDriver(this);
        }

        public void OnReceiveInputs(IInput input)
        {
            if (_gameData == null)
            {
                if (GameData.HasInstance)
                {
                    _gameData = GameData.GetInstance();
                }
                else
                {
                    return;
                }
            }

            _gameDriver.OnReceiveInputsWithUI(input);
        }
    }
}

