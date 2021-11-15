using System;
using GM.Data;
using GM.UI;
using UnityEngine;

namespace GM.Game
{
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
        private GameState _state;

        [SerializeField] private Randomizer _randomizer;
        private TetraBlock _tetraBlock;

        [SerializeField] private PlayfieldRenderer _playfield;

        private Block?[,] _grid;

        public void Initialize()
        {
            var gridSize = GameData.GetInstance().GridSize;
            _grid = new Block?[gridSize.x, gridSize.y];
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
                _tetraBlock.Move(Direction.Down, _grid);
                _playfield.SetFallingPosition(_tetraBlock.GetPositions());
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
