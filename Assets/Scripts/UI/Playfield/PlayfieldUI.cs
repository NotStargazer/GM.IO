using System;
using GM.Data;
using UnityEngine;

namespace GM.UI.Playfield
{
    public class PlayfieldUI : MonoBehaviour
    {
        [SerializeField] private LevelCounter _levelCounter;
        [SerializeField] private GameTimer _gameTimer;

        public void Set(GameState state)
        {
            var levelState = state.LevelState;
            _levelCounter.Set(levelState.Level, levelState.Gravity);
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
        }
    }
}
