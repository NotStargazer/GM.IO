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
        private List<BlockData> _order;

        public void Initialize()
        {
            _queue = new Queue<BlockData>(_queueSize);
            _history = new Queue<BlockData>(_pieceHistoryCount);
            _pool = new List<BlockData>(_blockData.TetraBlocks.Count * 5);
            _order = new List<BlockData>();

            for (var i = 0; i < _pool.Capacity; i++)
            {
                _pool.Add(_blockData.TetraBlocks[i % _blockData.TetraBlocks.Count]);
            }

            while (_history.Count < _pieceHistoryCount - _queueSize)
            {
                var foundExclusion = false;

                foreach (var blockData in _blockData.TetraBlocks)
                {
                    if (blockData.ExcludeFromGameStart)
                    {
                        foundExclusion = true;
                        _history.Enqueue(blockData);
                    }

                    if (_history.Count == _pieceHistoryCount - _queueSize)
                    {
                        break;
                    }
                }

                if (!foundExclusion)
                {
                    break;
                }
            }

            BlockData firstPiece;

            do
            {
                firstPiece = _blockData.TetraBlocks[Random.Range(0, _blockData.TetraBlocks.Count)];
            } while (firstPiece.ExcludeFromGameStart);

            _history.Enqueue(firstPiece);
            _queue.Enqueue(firstPiece);

            for (var i = 1; i < _queueSize; i++)
            {
                EnqueueNew();
            }
        }

        public void EnqueueNew()
        {
            int rand;
            BlockData piece;

            for (var roll = 0;; roll++)
            {
                rand = Random.Range(0, _pool.Count);
                piece = _pool[rand];
                if (!_history.Contains(piece) || roll >= _pieceRetryCount)
                {
                    break;
                }

                if (_order.Count > 0)
                {
                    _pool[rand] = _order[0];
                }
            }

            if (_order.Contains(piece))
            {
                _order.RemoveAt(_order.IndexOf(piece));
            }
            _order.Add(piece);

            _pool[rand] = _order[0];
            _history.Enqueue(piece);

            if (_history.Count >= _pieceHistoryCount)
            {
                _history.Dequeue();
            }

            _queue.Enqueue(piece);
        }

        public TetraBlock GetNext(BlockGrid grid)
        {
            var newPiece = new TetraBlock(_queue.Dequeue(), new Vector4(0.5f, 1f), grid);
            EnqueueNew();
            return newPiece;
        }
    }
}
