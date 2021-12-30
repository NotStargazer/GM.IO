using GM.Data;
using GM.UI.Rendering;
using UnityEngine;

namespace GM.UI
{
    public interface IUIRoot
    {
        public void StartRippleEffect(Vector2 focalPoint);
    }

    public class UIRoot : MonoBehaviour, IUIRoot
    {
        private GameDriver _gameDriver;
        private GameData _gameData;

        public void OnStartForGame()
        {
            _gameDriver = new GameDriver(this);
        }

        [SerializeField] private RippleEffectController _rippleEffectController;

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

        public void StartRippleEffect(Vector2 focalPoint)
        {
            _rippleEffectController.StartRipple(focalPoint);
        }
    }
}

