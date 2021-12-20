using System;
using GM.Data;
using UnityEngine;

namespace GM.UI.Playfield
{
    public class PlayfieldUI : MonoBehaviour
    {
        [SerializeField] private LevelCounter _levelCounter;
        [SerializeField] private GameTimer _gameTimer;
        [SerializeField] private GameAlert _gameAlert;

        public void StartGame()
        {
            _gameTimer.StartTimer();
        }

        public void Set(GameState state)
        {
            var levelState = state.LevelState;
            _levelCounter.Set(levelState.Level, levelState.Gravity);

            if (state.Alert.HasValue)
            {
                _gameAlert.Alert(state.Alert.Value);
            }
        }

        private void Awake()
        {
            if (!_levelCounter)
            {
                throw new ArgumentNullException(nameof(_levelCounter));
            }

            if (!_gameTimer)
            {
                throw new ArgumentNullException(nameof(_gameTimer));
            }

            if (!_gameAlert)
            {
                throw new ArgumentNullException(nameof(_gameAlert));
            }
        }
    }
}
