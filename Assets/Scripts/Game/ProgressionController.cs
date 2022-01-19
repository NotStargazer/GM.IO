using System;
using GM.Data;
using GM.UI.Playfield;
using UnityEngine;

namespace GM.Game
{
    [Serializable]
    public struct ProgressionDebug
    {
        public int LevelMultiplier;
        public bool AlwaysClearSection;
    }

    public class ProgressionController : MonoBehaviour
    {
        public static float SingleFrame => ProgressionData.FRAME;
        [SerializeField] private ProgressionDebug _progressionDebug;

        public ProgressionState CurrentState => GameData.GetInstance().ProgressionData.GetState(_level + _internalLevel);

        private int _internalLevel;
        private int _level;
        private float _startTime;

        private int _section;
        private int _sectionClears;
        private float _sectionStartTime;

        public void Initialize()
        {
            _startTime = _sectionStartTime = Time.time;
            _level = _internalLevel = 0;
            _section = _sectionClears = 0;
            GameData.GetInstance().ProgressionData.Reset();
        }

        public void IncrementLevel(ref GameState state, int? lines = null)
        {
            int bonusLevels;

            switch (lines)
            {
                case 3:
                    bonusLevels = 1;
                    break;
                case 4:
                    bonusLevels = 2;
                    break;
                default:
                    bonusLevels = 0;
                    break;
            }

            var levels = 1;

            if (lines.HasValue)
            {
                levels = lines.Value + bonusLevels;
            }

            var oldSectionLevel = _level % 100;
#if UNITY_EDITOR
            levels *= _progressionDebug.LevelMultiplier;
#endif
            _level += levels;

            if (oldSectionLevel > _level % 100)
            {
                if (lines > 0)
                {
                    GlobalResources.GetInstance().SoundController.PlaySFX(SFX.Section);
                    _section++;
                    CheckSectionClear(ref state);
                    _sectionStartTime = Time.time;
                }
                else
                {
                    _level = _section * 100 + 99;
                }
            }

            var currentState = CurrentState;

            state.LevelState = new LevelState
            {
                Level = _level,
                Gravity = SingleFrame / currentState.DropDuration * currentState.Gravity / 20
            };

            //Debug.Log($"Level: {_level + _internalLevel}, Section {_section}");
        }

        private void CheckSectionClear(ref GameState state)
        {
            //TODO: Section Requirements
            var sectionClear = Time.time - _sectionStartTime < 60;

#if UNITY_EDITOR
            sectionClear = sectionClear || _progressionDebug.AlwaysClearSection;
#endif

            if (sectionClear)
            {
                state.Alert = AlertType.SectionClear;
                _sectionClears++;
                _internalLevel += 100;
            }
        }
    }
}
