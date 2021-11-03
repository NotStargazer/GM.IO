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

        public TetraBlock(BlockData tetraBlock, Vector4 textureST)
        {
            _rotationStates = tetraBlock.RotationStates;
            _textureST = textureST;
            _color = tetraBlock.BlockColor;
        }

        public void Move(Direction direction, Block?[,] grid)
        {
            _position += DIRECTIONS[direction];
        }

        public void Rotate(int direction, Block[] grid)
        {
            _rotationIndex += direction;
        }

        public Block[] GetBlocks()
        {
            var blocks = new Block[4];

            for (var i = 0; i < 4; i++)
            {
                blocks[i].TextureST = _textureST;
                blocks[i].Color = _color;
            }

            return blocks;
        }
    }
}
