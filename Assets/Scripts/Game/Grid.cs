using UnityEngine;

namespace GM.Game
{
    public class Grid
    {
        private Vector2Int _gridSize;
        private Block?[,] _grid;

        public Block?[,] Blocks => _grid;

        public Grid(Vector2Int gridSize)
        {
            _gridSize = gridSize;
            _grid = new Block?[gridSize.x, gridSize.y];
        }

        public bool CheckCollision(Vector2Int position)
        {
            if (position.x < 0
                || position.x >= _gridSize.x
                || position.y < 0)
            {
                return true;
            }

            if (_grid[position.x, position.y].HasValue)
            {
                return true;
            }

            return false;
        }

        public void LockTetraBlock(ref TetraBlock tetraBlock)
        {
            var block = tetraBlock.GetBlock();

            foreach (var position in tetraBlock.GetPositions())
            {
                Blocks[position.x, position.y] = block;
            }

            tetraBlock = null;
        }
    }
}
