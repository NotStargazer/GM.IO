using System.Collections.Generic;
using GM.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace GM.Game
{
    public class TetraBlockFactory : MonoBehaviour
    {
        private static readonly int INSTANCE_COLORS = Shader.PropertyToID("_Colors");
        private static readonly int INSTANCE_ST = Shader.PropertyToID("_MainTex_ST");
        private static readonly int INSTANCE_OUTLINEL = Shader.PropertyToID("_OutlinesL");
        private static readonly int INSTANCE_OUTLINEC = Shader.PropertyToID("_OutlinesC");

        [SerializeField] private TetraBlockData _blockData;
        [SerializeField] private int _pieceHistoryCount;
        [SerializeField] private int _pieceRetryCount;
        [SerializeField] private int _queueSize;
        [Range(0f, 1f)] [SerializeField] private float _queueSecondarySize;
        [SerializeField] private Color _holdLockColor;

        private Queue<BlockData> _queue;
        private Queue<BlockData> _history;
        private List<BlockData> _pool;
        private List<BlockData> _order;

        private Matrix4x4[] _queueTransforms;
        private Matrix4x4[] _holdTransforms;
        private Vector4[] _colors;
        private Vector4[] _textureST;
        private MaterialPropertyBlock _queueProperties;
        private MaterialPropertyBlock _holdProperties;

        private bool _holdLocked;
        private TetraBlock _holdBlock;

        private readonly Quaternion _baseRotation = new Quaternion(0, 0, 1, 0);
        private Vector3 _basePosition;

        public void Initialize(Vector4 textureST)
        {
            _basePosition = transform.position;

            _colors = new Vector4[_queueSize * 4];
            _textureST = new Vector4[_queueSize * 4];
            _queueProperties = new MaterialPropertyBlock();
            _holdProperties = new MaterialPropertyBlock();

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

            UpdateQueue(textureST);
        }

        public TetraBlock GetNext(BlockGrid grid, bool holdLock = false)
        {
            var oldLock = _holdLocked;
            _holdLocked = holdLock;

            if (_holdBlock != null && _holdLocked != oldLock)
            {
                UpdateHold(_holdBlock);
            }

            var newPiece = new TetraBlock(_queue.Dequeue(), new Vector4(0.5f, 1f), grid);
            EnqueueNew();
            UpdateQueue(new Vector4(0.5f, 1f));
            return newPiece;
        }

        public bool GetHold(ref TetraBlock tetraBlock, BlockGrid grid)
        {
            if (_holdLocked)
            {
                return false;
            }

            tetraBlock.Reset();

            if (_holdBlock == null)
            {
                _holdBlock = tetraBlock;
                tetraBlock = GetNext(grid, holdLock: true);
                return true;
            }

            _holdLocked = true;
            (_holdBlock, tetraBlock) = (tetraBlock, _holdBlock);
            UpdateHold(_holdBlock);
            return true;
        }

        public void Render()
        {
            var gameData = GameData.GetInstance();
            Graphics.DrawMeshInstanced(
                mesh: gameData.BlockMesh,
                submeshIndex: 0,
                material: gameData.BlockMaterial,
                matrices: _queueTransforms,
                properties: _queueProperties,
                castShadows: ShadowCastingMode.Off,
                receiveShadows: false,
                count: 4 * _queueSize);

            if (_holdTransforms != null)
            {
                Graphics.DrawMeshInstanced(
                    mesh: gameData.BlockMesh,
                    submeshIndex: 0,
                    material: gameData.BlockMaterial,
                    matrices: _holdTransforms,
                    properties: _holdProperties,
                    castShadows: ShadowCastingMode.Off,
                    receiveShadows: false,
                    count: 4);
            }
        }

        private void UpdateHold(TetraBlock tetraBlock)
        {
            var size = Vector3.one * _queueSecondarySize;
            var colors = new Vector4[4];
            var textureSTs = new Vector4[4];
            _holdTransforms = new Matrix4x4[4];

            var block = tetraBlock.GetBlock();
            var defaultState = tetraBlock.DefaultState;

            for (var holdIndex = 0; holdIndex < 4; holdIndex++)
            {
                colors[holdIndex] = _holdLocked ? _holdLockColor : block.Color.linear;
                textureSTs[holdIndex] = block.TextureST;

                var blockPos = defaultState[holdIndex];
                var matrix = Matrix4x4.identity;
                matrix.SetTRS(_basePosition - new Vector3(3, 0) + new Vector3(blockPos.x, blockPos.y + 0.5f) * _queueSecondarySize, _baseRotation, size);

                _holdTransforms[holdIndex] = matrix;
            }

            _holdProperties.SetVectorArray(INSTANCE_COLORS, colors);
            _holdProperties.SetVectorArray(INSTANCE_ST, textureSTs);
            _holdProperties.SetVectorArray(INSTANCE_OUTLINEL, new Vector4[4]);
            _holdProperties.SetVectorArray(INSTANCE_OUTLINEC, new Vector4[4]);
        }

        private void UpdateQueue(Vector4 textureST)
        {
            var transforms = new List<Matrix4x4>();

            var size = new Vector3(4, 2);
            var x = 0f;
            var prevOffset = size.x;
            var prevSize = 1f;
            var blockIndex = 0;

            foreach (var blockData in _queue)
            {
                var blocks = blockData.RotationStates[0].Blocks;
                var yNegation = int.MaxValue;

                foreach (var block in blocks)
                {
                    yNegation = Mathf.Min(yNegation, block.y);
                }

                foreach (var block in blocks)
                {
                    var xOffset = size.x / blockData.GridSize > 1 ? 1 : 0;

                    var matrix = new Matrix4x4();
                    matrix.SetTRS(_basePosition + new Vector3(block.x + xOffset, block.y - yNegation + 0.5f) * prevSize + new Vector3(x, 0),
                        _baseRotation,
                        Vector3.one * prevSize);
                    transforms.Add(matrix);
                    _colors[blockIndex] = blockData.BlockColor.linear;
                    _textureST[blockIndex] = textureST;
                    blockIndex++;
                }

                x += prevOffset + 0.5f * _queueSecondarySize;
                prevOffset = size.x * _queueSecondarySize;
                prevSize = _queueSecondarySize;
            }

            _queueTransforms = transforms.ToArray();

            _queueProperties.SetVectorArray(INSTANCE_COLORS, _colors);
            _queueProperties.SetVectorArray(INSTANCE_ST, _textureST);
            _queueProperties.SetVectorArray(INSTANCE_OUTLINEL, new Vector4[4 * _queueSize]);
            _queueProperties.SetVectorArray(INSTANCE_OUTLINEC, new Vector4[4 * _queueSize]);
        }

        private void EnqueueNew()
        {
            int rand;
            BlockData piece;

            for (var roll = 0; ; roll++)
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

        private void OnDrawGizmos()
        {
            var size = new Vector3(4, 2);
            var halfSize = new Vector3(2, 1);
            var curPos = transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(curPos + halfSize, size);

            curPos.x += size.x + 0.5f * _queueSecondarySize;

            for (var i = 1; i < _queueSize; i++)
            {
                Gizmos.DrawWireCube(curPos + halfSize * _queueSecondarySize, size * _queueSecondarySize);
                curPos.x += (size.x + 0.5f) * _queueSecondarySize;
            }
        }
    }
}
