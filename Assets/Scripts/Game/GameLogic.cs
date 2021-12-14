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
        private bool _running;

        public float Duration
        {
            set => _duration = value;
        }

        public bool HasExpired(out float excess)
        {
            excess = Mathf.Clamp01(Time.time - _deadline);
            return Time.time >= _deadline;
        }

        public bool HasStarted()
        {
            return _running;
        }

        public void Start(float excess = 0)
        {
            _running = true;
            _deadline = Time.time + _duration - excess;
        }

        public void Stop()
        {
            _running = false;
        }

        public void Enable()
        {
            _running = true;
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
        public Timer LineTimer;
        public Timer AutoShiftTimer;
        public Timer ShiftCooldownTimer;

        public void SetTimers(ProgressionState state, bool lineClear = false)
        {
            SpawnTimer.Duration = lineClear ? state.LineClearSpawnDuration : state.SpawnDuration;
            DropTimer.Duration = state.DropDuration;
            LockTimer.Duration = state.LockDuration;
            LineTimer.Duration = state.LineDuration;
            AutoShiftTimer.Duration = state.AutoShiftDuration;
            ShiftCooldownTimer.Duration = ProgressionController.SingleFrame;
        }
    }

    /*
     * ==>
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

        [SerializeField] private TetraBlockFactory _tetraBlockFactory;
        [SerializeField] private PlayfieldRenderer _playfield;
        [SerializeField] private ProgressionController _progression;

        public void OnGameStart()
        {
            var instance = GameData.GetInstance();
            _grid = new BlockGrid(instance.GridSize, instance.ExcessHeight);
            _state = new GameState();
            _progression.Initialize();
            _tetraBlockFactory.Initialize(new Vector4(0.5f, 1f));
            _playfield.Initialize();

            _timers = new Timers();
            _timers.SetTimers(_progression.CurrentState);
            _timers.SpawnTimer.Duration = 0;
            _timers.SpawnTimer.Start();
        }

        public GameState LogicUpdate(IInput input)
        {
            var currentFrameDropDuration = _progression.CurrentState.DropDuration;

            // => Get new TetraBlock
            if (_tetraBlock == null)
            {
                _playfield.RenderBlocks();
                _tetraBlockFactory.Render();

                if (_timers.LineTimer.HasStarted())
                {
                    if (_timers.LineTimer.HasExpired(out var lineExcess))
                    {
                        _timers.SpawnTimer.Start(lineExcess);
                        _timers.LineTimer.Stop();
                        _grid.DropLines(_state.LinesCleared.ToArray());
                        _playfield.Blocks = _grid.Blocks;
                    }
                }
                else if (_timers.SpawnTimer.HasExpired(out var spawnExcess))
                {
                    _tetraBlock = _tetraBlockFactory.GetNext(_grid);
                    _timers.DropTimer.Start(spawnExcess + ProgressionController.SingleFrame);
                    _timers.LockTimer.Start(spawnExcess);
                    _timers.ShiftCooldownTimer.Start();
                    _progression.IncrementLevel();
                    _state.Reset();

                    //Pre-Hold
                    if (input.ButtonHold(Actions.Hold))
                    {
                        _tetraBlockFactory.GetHold(ref _tetraBlock, _grid);
                    }

                    //Pre-Rotation
                    if (input.ButtonHold(Actions.Rotation, out float preRotate))
                    {
                        _tetraBlock.Rotate(Mathf.RoundToInt(preRotate), _grid);
                    }

                    _tetraBlock.PerformChecks(_grid);
                }

                if (input.ButtonDown(Actions.Move))
                {
                    _timers.AutoShiftTimer.Start();
                }

                if (input.ButtonUp(Actions.Move))
                {
                    _timers.AutoShiftTimer.Stop();
                }

                if (_tetraBlock == null)
                {
                    return null;
                }

                _playfield.SetFallingProperties(_tetraBlock.GetBlock());
                _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);
            }

            // => Update Main Input Logic
            if (input.ButtonDown(Actions.Hold))
            {
                _tetraBlockFactory.GetHold(ref _tetraBlock, _grid);

                _timers.DropTimer.Start(Time.deltaTime + ProgressionController.SingleFrame);
                _timers.LockTimer.Start(Time.deltaTime);
                _timers.ShiftCooldownTimer.Start();
                _timers.AutoShiftTimer.Enable();

                //Pre-Rotation
                if (input.ButtonHold(Actions.Rotation, out float preRotate))
                {
                    _tetraBlock.Rotate(Mathf.RoundToInt(preRotate), _grid);
                }

                _tetraBlock.PerformChecks(_grid);

                _playfield.SetFallingProperties(_tetraBlock.GetBlock());
                _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);
            }

            if (input.ButtonDown(Actions.SonicDrop))
            {
                for (var i = 0; i < _grid.Size.y; i++)
                {
                    if (_tetraBlock.Move(Direction.Down, _grid))
                    {
                        break;
                    }
                }

                _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);
            }

            if (input.ButtonDown(Actions.Rotation, out float rotation))
            {
                var wasLanded = _tetraBlock.Landed;
                _tetraBlock.Rotate(Mathf.RoundToInt(rotation), _grid);
                _timers.AutoShiftTimer.Enable();

                //Syncro
                if (input.ButtonHold(Actions.Move, out float direction) && wasLanded)
                {
                    _tetraBlock.Move(direction > 0 ? Direction.Right : Direction.Left, _grid);  
                }

                _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);

                if (_tetraBlock.Landed != wasLanded)
                {
                    _timers.DropTimer.Duration = currentFrameDropDuration - ProgressionController.SingleFrame - Time.deltaTime;
                    _timers.DropTimer.Start();
                }
            }
            else if (input.ButtonDown(Actions.Move, out float direction))
            {
                var wasLanded = _tetraBlock.Landed;
                _tetraBlock.Move(direction > 0 ? Direction.Right : Direction.Left, _grid);
                _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);
                _timers.AutoShiftTimer.Start();

                if (_tetraBlock.Landed != wasLanded)
                {
                    _timers.DropTimer.Duration = currentFrameDropDuration - ProgressionController.SingleFrame - Time.deltaTime;
                    _timers.DropTimer.Start();
                }
            }

            //Delayed Auto Shift
            if (input.ButtonHold(Actions.Move, out float autoDirection))
            {
                if (input.ButtonUp(Actions.Move))
                {
                    _timers.AutoShiftTimer.Start();
                }

                if (_timers.AutoShiftTimer.HasExpired(out _))
                {
                    if (_timers.ShiftCooldownTimer.HasExpired(out var shiftExcess) && _timers.AutoShiftTimer.HasStarted())
                    {
                        var wasLanded = _tetraBlock.Landed;

                        if (_tetraBlock.Move(autoDirection > 0 ? Direction.Right : Direction.Left, _grid))
                        {
                            _timers.AutoShiftTimer.Stop();
                        }
                        _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);
                        _timers.ShiftCooldownTimer.Start(_timers.ShiftCooldownTimer.HasStarted() ? shiftExcess : 0);

                        if (_tetraBlock.Landed != wasLanded)
                        {
                            _timers.DropTimer.Duration = currentFrameDropDuration - ProgressionController.SingleFrame - Time.deltaTime;
                            _timers.DropTimer.Start();
                        }
                    }
                }
                else
                {
                    _timers.ShiftCooldownTimer.Stop();
                }
            }

            if (input.ButtonHold(Actions.DropLock))
            {
                _timers.DropTimer.Duration = ProgressionController.SingleFrame;

                if (input.ButtonDown(Actions.DropLock))
                {
                    _timers.DropTimer.Start(Time.deltaTime + ProgressionController.SingleFrame);
                }

                // => [Wait for block lock]
                if (_tetraBlock.Landed)
                {
                    OnLock();
                    _timers.SpawnTimer.Start();
                    _playfield.RenderBlocks();
                    _tetraBlockFactory.Render();

                    // => Return State
                    return _state;
                    // <==
                }
            }
            else
            {
                _timers.DropTimer.Duration = currentFrameDropDuration;
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

                // => [Wait for block lock]
                if (_timers.DropTimer.HasExpired(out var dropExcess))
                {
                    _timers.DropTimer.Duration = currentFrameDropDuration;
                    var didDrop = false;

                    for (var i = 0; i < _progression.CurrentState.Gravity; i++)
                    {
                        if (!_tetraBlock.Move(Direction.Down, _grid))
                        {
                            didDrop = true;
                        }
                    }

                    if (didDrop)
                    {
                        if (_tetraBlock.IsAtLowestPoint)
                        {
                            _timers.LockTimer.Start(dropExcess);
                        }

                        _timers.DropTimer.Start(dropExcess);
                    }
                    _playfield.SetFallingPosition(_tetraBlock.GetPositions(), _grid);
                }
            }

            _playfield.RenderBlocks();
            _tetraBlockFactory.Render();

            // => Return State
            return _state;
            // <==
        }

        private void OnLock()
        {
            _grid.LockTetraBlock(ref _tetraBlock, ref _state);
            var lineCount = _state.LinesCleared.Count;

            if (lineCount > 0)
            {
                _progression.IncrementLevel(lineCount);
                _timers.LineTimer.Start();
            }

            _tetraBlock = null;
            _playfield.Blocks = _grid.Blocks;
            _playfield.ResetFalling();

            // ====> Progression Controller
            _timers.SetTimers(_progression.CurrentState, lineCount > 0);
        }

        private void Awake()
        {
            if (!_tetraBlockFactory)
            {
                throw new ArgumentNullException(nameof(_tetraBlockFactory));
            }

            if (!_playfield)
            {
                throw new ArgumentNullException(nameof(_playfield));
            }
        }
    }
}
