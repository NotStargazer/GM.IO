using System.Collections.Generic;
using GM.Data;
using UnityEngine;

namespace GM.Game
{
    public class TetraBlockFactory : MonoBehaviour
    {
        [SerializeField] private TetraBlockData _blockData;
        [SerializeField] private int _pieceHistoryCount;
        [SerializeField] private int _pieceRetryCount;
        [SerializeField] private int _queueSize;

        private Queue<BlockData> _queue;
        private Queue<BlockData> _history;
        private List<BlockData> _pool;

        public void Initialize()
        {
            _queue = new Queue<BlockData>(_queueSize);
            _history = new Queue<BlockData>(_pieceHistoryCount);
            _pool = new List<BlockData>(_blockData.TetraBlocks.Count * 5);

            for (var i = 0; i < _pool.Count; i++)
            {
                _pool.Add(_blockData.TetraBlocks[i % _blockData.TetraBlocks.Count]);
            }

            while (_history.Count == _pieceHistoryCount - _queueSize)
            {
                foreach (var blockData in _blockData.TetraBlocks)
                {
                    if (blockData.ExcludeFromGameStart)
                    {
                        _history.Enqueue(blockData);
                    }

                    if (_history.Count == _pieceHistoryCount - 1)
                    {
                        break;
                    }
                }
            }

            BlockData firstPiece;

            do
            {
                firstPiece = _blockData.TetraBlocks[Random.Range(0, _blockData.TetraBlocks.Count)];
            } while (firstPiece.ExcludeFromGameStart);

            _history.Enqueue(firstPiece);
        }

        public TetraBlock GetNext(BlockGrid grid)
        {
            return new TetraBlock(_queue.Dequeue(), new Vector4(0.5f, 1f), grid);
        }
    }
}
