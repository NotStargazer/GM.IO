using System.Collections.Generic;
using GM.Data;
using UnityEngine;

namespace GM.Game
{
    public class BlockGrid
    {
        private Vector2Int _gridSize;
        private Block?[,] _grid;

        public Block?[,] Blocks => _grid;
        public Vector2Int Size => _gridSize;

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

        public void LockTetraBlock(ref TetraBlock tetraBlock, ref GameState gameState)
        {
            var block = tetraBlock.GetBlock();
            var uniqueLines = new List<int>();

            foreach (var position in tetraBlock.GetPositions())
            {
                Blocks[position.x, position.y] = block;

                if (!uniqueLines.Contains(position.y))
                {
                    uniqueLines.Add(position.y);
                }
            }

            uniqueLines.Sort();
            uniqueLines.Reverse();

            foreach (var uniqueLine in uniqueLines)
            {
                if (CheckLine(uniqueLine))
                {
                    var lineBlocks = new Block[_gridSize.x];

                    for (var x = 0; x < _gridSize.x; x++)
                    {
                        lineBlocks[x] = _grid[x, uniqueLine].Value;
                    }

                    gameState.LinesCleared.Add(new DestroyedLine
                    {
                        LineNumber = uniqueLine,
                        Blocks = lineBlocks
                    });
                    ClearLine(uniqueLine);
                }
            }

            tetraBlock = null;
        }

        private bool CheckLine(int line)
        {
            for (var x = 0; x < _gridSize.x; x++)
            {
                if (!Blocks[x, line].HasValue)
                {
                    return false;
                }
            }

            return true;
        }

        private void ClearLine(int line)
        {
            for (var x = 0; x < _gridSize.x; x++)
            {
                Blocks[x, line] = null;
            }
        }

        public void DropLines(DestroyedLine[] lines)
        {
            foreach (var line in lines)
            {
                for (var y = line.LineNumber; y < _gridSize.y - 1; y++)
                {
                    for (var x = 0; x < _gridSize.x; x++)
                    {
                        if (Blocks[x, y + 1].HasValue)
                        {
                            Blocks[x, y] = Blocks[x, y + 1];
                            Blocks[x, y + 1] = null;
                        }
                    }
                }
            }
        }
    }
}
