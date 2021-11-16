using System;
using GM.Data;
using GM.UI;
using UnityEngine;

namespace GM.Game
{
    public struct Timer
    {
        private float _duration;
        private float _deadline;

        public float Duration
        {
            set => _duration = value + Time.time;
        }

        public bool HasExpired(out float excess)
        {
            excess = Mathf.Clamp01(Time.time - _deadline);
            return Time.time > _deadline;
        }

        public void Restart()
        {
            _deadline = Time.time + _duration;
        }
    }

    public struct Timers
    {
        public Timer DropTimer;
    }

    /*
     * ==>
     * => Get new TetraBlock
     * => Update Main Input Logic
     * => [Wait for block lock]
     * <== : => Progression Controller
     * => Ranking Controller
     * => Update Playfield
     * => Return State
     * <==
     */

    public class GameLogic : MonoBehaviour
    {
        private Grid _grid;
        private GameState _state;

        [SerializeField] private Randomizer _randomizer;
        private TetraBlock _tetraBlock;

        [SerializeField] private PlayfieldRenderer _playfield;

        public void Initialize()
        {
            var gridSize = GameData.GetInstance().GridSize;
            _grid = new Grid(gridSize);
            _state = new GameState();
            _playfield.Initialize();
        }

        public GameState LogicUpdate(IInput input)
        {
            // => Get new TetraBlock
            if (_tetraBlock == null)
            {
                _tetraBlock = _randomizer.GetNext();

                if (_tetraBlock == null)
                {
                    return null;
                }

                _playfield.SetFallingProperties(_tetraBlock.GetBlock());
                _playfield.SetFallingPosition(_tetraBlock.GetPositions());
            }

            // => Update Main Input Logic
            if (input.ButtonDown(Actions.Move, out float direction))
            {
                _tetraBlock.Move(direction > 0 ? Direction.Right : Direction.Left, _grid);
                _playfield.SetFallingPosition(_tetraBlock.GetPositions());
            }

            if (input.ButtonDown(Actions.DropLock))
            {
                if (_tetraBlock.Move(Direction.Down, _grid))
                {
                    _grid.LockTetraBlock(ref _tetraBlock);
                    _playfield.Blocks = _grid.Blocks;
                }
                else
                {
                    _playfield.SetFallingPosition(_tetraBlock.GetPositions());
                }
            }

            _playfield.RenderBlocks();

            // => Return State
            return _state;
            // <==
        }

        private void Awake()
        {
            if (!_randomizer)
            {
                throw new ArgumentNullException(nameof(_randomizer));
            }

            if (!_playfield)
            {
                throw new ArgumentNullException(nameof(_playfield));
            }
        }
    }
}
