using UnityEngine;

namespace GM.Game
{
    public class BlockGrid
    {
        private Vector2Int _gridSize;
        private Block?[,] _grid;

        public Block?[,] Blocks => _grid;

        public BlockGrid(Vector2Int gridSize, int excess)
        {
            _gridSize = gridSize;
            _grid = new Block?[gridSize.x, gridSize.y + excess];
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
