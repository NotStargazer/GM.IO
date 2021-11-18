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
            set => _duration = value;
        }

        public bool HasExpired(out float excess)
        {
            excess = Mathf.Clamp01(Time.time - _deadline);
            return Time.time > _deadline;
        }

        public void Start(float excess = 0)
        {
            _deadline = Time.time + _duration - excess;
        }

        public void ExtendThisFrame()
        {
            var remaining = _deadline - Time.time;
            _deadline = Time.time + remaining + Time.deltaTime;
        }
    }

    public struct Timers
    {
        public Timer SpawnTimer;
        public Timer DropTimer;
        public Timer LockTimer;

        public void SetTimers(ProgressionState state)
        {
            SpawnTimer.Duration = state.SpawnDuration;
            DropTimer.Duration = state.DropDuration;
            LockTimer.Duration = state.LockDuration;
        }
    }

    /*
     * ==>
     * => Set Timers
     * => Get new TetraBlock
     * => Update Main Input Logic
     * => [Wait for block lock]
     * == {
     * ===> Progression Controller
     * ===> Ranking Controller
     * ===> Update Playfield
     * == }
     * => Update Block
     * => Return State
     * <==
     */

    public class GameLogic : MonoBehaviour
    {
        private Timers _timers;
        private BlockGrid _grid;
        private GameState _state;
        private TetraBlock _tetraBlock;

        [SerializeField] private Randomizer _randomizer;
        [SerializeField] private PlayfieldRenderer _playfield;
        [SerializeField] private ProgressionController _progression;

        public void OnGameStart()
        {
            var instance = GameData.GetInstance();
            _grid = new BlockGrid(instance.GridSize, instance.ExcessHeight);
            _state = new GameState();
            //_randomizer.Initialize();
            _playfield.Initialize();
            _progression.Initialize();

            _timers = new Timers();
            _timers.SetTimers(_progression.CurrentState);
        }

        public GameState LogicUpdate(IInput input)
        {
            // => Get new TetraBlock
            if (_tetraBlock == null)
            {
                _playfield.RenderBlocks(true);

                if (_timers.SpawnTimer.HasExpired(out var spawnExcess))
                {
                    _tetraBlock = _randomizer.GetNext();
                    _timers.DropTimer.Start(spawnExcess);
                }

                if (_tetraBlock == null)
                {
                    return null;
                }

                _playfield.SetFallingProperties(_tetraBlock.GetBlock());
                _playfield.SetFallingPosition(_tetraBlock.GetPositions());
            }

            // => Update Main Input Logic
            if (input.ButtonDown(Actions.Rotation, out float rotation))
            {
                _tetraBlock.Rotate(Mathf.RoundToInt(rotation), _grid);

                //Syncro
                if (input.ButtonHold(Actions.Move, out float direction))
                {
                    _tetraBlock.Move(direction > 0 ? Direction.Right : Direction.Left, _grid);
                }

                _playfield.SetFallingPosition(_tetraBlock.GetPositions());
            }
            else if (input.ButtonDown(Actions.Move, out float direction))
            {
                _tetraBlock.Move(direction > 0 ? Direction.Right : Direction.Left, _grid);
                _playfield.SetFallingPosition(_tetraBlock.GetPositions());
            }

            if (input.ButtonHold(Actions.DropLock))
            {
                _timers.DropTimer.Duration = _progression.FastDropDuration;

                if (input.ButtonDown(Actions.DropLock))
                {
                    _timers.DropTimer.Start();
                }

                // => [Wait for block lock]
                if (_tetraBlock.Landed)
                {
                    OnLock();
                    _timers.SpawnTimer.Start();
                    _playfield.RenderBlocks();

                    // => Return State
                    return _state;
                    // <==
                }
            }
            else
            {
                _timers.DropTimer.Duration = _progression.CurrentState.DropDuration;
            }

            // => Update Blocks
            if (_tetraBlock.Landed)
            {
                _timers.DropTimer.ExtendThisFrame();

                if (_timers.LockTimer.HasExpired(out var lockExcess))
                {
                    OnLock();
                    _timers.SpawnTimer.Start(lockExcess);
                }
            }
            else
            {
                _timers.LockTimer.ExtendThisFrame();

                if (_timers.DropTimer.HasExpired(out var dropExcess))
                {
                    if (!_tetraBlock.Move(Direction.Down, _grid))
                    {
                        _timers.LockTimer.Start(dropExcess);
                        _timers.DropTimer.Start(dropExcess);
                    }
                    _playfield.SetFallingPosition(_tetraBlock.GetPositions());
                }
            }

            _playfield.RenderBlocks();

            // => Return State
            return _state;
            // <==
        }

        private void OnLock()
        {
            _grid.LockTetraBlock(ref _tetraBlock);
            _tetraBlock = null;
            _playfield.Blocks = _grid.Blocks;
            _playfield.ResetFalling();

            // ====> Progression Controller
            _timers.SetTimers(_progression.CurrentState);
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
