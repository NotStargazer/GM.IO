using System.Collections.Generic;
using GM.Data;
using UnityEngine;

namespace GM.Game
{
    public struct Block
    {
        public Vector4 TextureST;
        public Color Color;
    }

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    public class TetraBlock
    {
        private static readonly Dictionary<Direction, Vector2Int> DIRECTIONS = new Dictionary<Direction, Vector2Int>
        {
            {Direction.Left, new Vector2Int(-1, 0)},
            {Direction.Right, new Vector2Int(1, 0)},
            {Direction.Up, new Vector2Int(0, 1)},
            {Direction.Down, new Vector2Int(0, -1)}
        };

        private Vector2Int _position;

        private Vector4 _textureST;
        private Color _color;

        private int _rotationIndex;
        private List<BlockSublist> _rotationStates;

        public TetraBlock(BlockData tetraBlock, Vector4 textureST, Vector2Int gridSize)
        {
            _rotationStates = tetraBlock.RotationStates;
            _textureST = textureST;
            _color = tetraBlock.BlockColor;

            var highestInitial = 0;
            foreach (var position in _rotationStates[0].Blocks)
            {
                if (position.y > highestInitial)
                {
                    highestInitial = position.y;
                }
            }

            var yPos = gridSize.y - highestInitial;
            var xPos = (gridSize.x >> 1) - Mathf.CeilToInt(tetraBlock.GridSize / 2f);

            _position = new Vector2Int(xPos, yPos - 1);
        }

        /// <summary>
        /// Move the piece.
        /// </summary>
        /// <returns>True if piece collides.</returns>
        public bool Move(Direction direction, Grid grid)
        {
            _position += DIRECTIONS[direction];

            if (CheckCollisions(grid))
            {
                _position -= DIRECTIONS[direction];
                return true;
            }

            return false;
        }

        public void Rotate(int direction, Grid grid)
        {
            _rotationIndex += direction;

            if (CheckCollisions(grid))
            {
                _rotationIndex -= direction;
            }
        }

        public Block GetBlock()
        {
            return new Block()
            {
                Color = _color,
                TextureST = _textureST
            };
        }

        public Vector2Int[] GetPositions()
        {
            var positions = _rotationStates[_rotationIndex].Blocks.ToArray();

            for (var index = 0; index < positions.Length; index++)
            {
                positions[index] += _position;
            }

            return positions;
        }

        private bool CheckCollisions(Grid grid)
        {
            var positions = GetPositions();

            foreach (var position in positions)
            {
                if (grid.CheckCollision(position))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
