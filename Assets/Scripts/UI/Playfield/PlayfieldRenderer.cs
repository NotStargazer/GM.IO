using GM.Game;
using GM.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GM.UI
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PlayfieldRenderer : MonoBehaviour
    {
        private static readonly int INSTANCE_COLORS = Shader.PropertyToID("_Colors");
        private static readonly int INSTANCE_ST = Shader.PropertyToID("_MainTex_ST");
        private static readonly int INSTANCE_OUTLINEL = Shader.PropertyToID("_OutlinesL");
        private static readonly int INSTANCE_OUTLINEC = Shader.PropertyToID("_OutlinesC");

        private const int BORDER_VERT_COUNT = 16;
        
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Material _borderMaterial;
        [Range(0, 1)] [SerializeField] private float _ghostTransparency;

        //Border Properties
        private Mesh _borderMesh;
        private Vector2Int _gridSize;

        //Block Properties
        private Vector3 _basePosition;
        private readonly Quaternion _baseRotation = new Quaternion(0, 0, 1, 0);

        //Block Instancing
        private int _blockCount;
        private Matrix4x4[] _staticTransforms;
        private Matrix4x4[] _fallingTransforms;
        private Matrix4x4[] _ghostTransforms;
        private Vector4[] _colors;
        private Vector4[] _textureST;
        private Vector4[] _outlineLines;
        private Vector4[] _outlineCorners;
        private MaterialPropertyBlock _staticProperties;
        private MaterialPropertyBlock _fallingProperties;
        private MaterialPropertyBlock _ghostProperties;

        public Block?[,] Blocks
        {
            set
            {
                var currentIndex = 0;
                
                _blockCount = _gridSize.x * _gridSize.y;
                _staticTransforms = new Matrix4x4[_blockCount];
                _colors = new Vector4[_blockCount];
                _textureST = new Vector4[_blockCount];
                _outlineLines = new Vector4[_blockCount];
                _outlineCorners = new Vector4[_blockCount];

                for (var y = 0; y < _gridSize.y; y++)
                {
                    for (var x = 0; x < _gridSize.x; x++)
                    {
                        var val = value[x, y];

                        if (val.HasValue)
                        {
                            var block = val.Value;
                            var position = _basePosition + new Vector3(x, y);
                            Matrix4x4 matrix = Matrix4x4.zero;
                            matrix.SetTRS(position, _baseRotation, Vector3.one);
                            _staticTransforms[currentIndex] = matrix;
                            _colors[currentIndex] = block.Color.linear;
                            _textureST[currentIndex] = block.TextureST;

                            var xg = x > 0;
                            var xl = x + 1 < _gridSize.x;
                            var yg = y > 0;
                            var yl = y + 1 < _gridSize.y;

                            _outlineLines[currentIndex] = new Vector4(
                                x: xg && !value[x - 1, y].HasValue ? 1 : 0,
                                y: xl && !value[x + 1, y].HasValue ? 1 : 0,
                                z: yg && !value[x, y - 1].HasValue ? 1 : 0,
                                w: yl && !value[x, y + 1].HasValue ? 1 : 0);

                            _outlineCorners[currentIndex] = new Vector4(
                                x: xg && yg && !value[x - 1, y - 1].HasValue ? 1 : 0,
                                y: xl && yg && !value[x + 1, y - 1].HasValue ? 1 : 0,
                                z: xg && yl && !value[x - 1, y + 1].HasValue ? 1 : 0,
                                w: xl && yl && !value[x + 1, y + 1].HasValue ? 1 : 0);

                            currentIndex++;
                        }
                    }
                }
                
                _staticProperties.SetVectorArray(INSTANCE_COLORS, _colors);
                _staticProperties.SetVectorArray(INSTANCE_ST, _textureST);
                _staticProperties.SetVectorArray(INSTANCE_OUTLINEL, _outlineLines);
                _staticProperties.SetVectorArray(INSTANCE_OUTLINEC, _outlineCorners);
            }
        }

        public void Initialize()
        {
            if (!_borderMaterial)
            {
                throw new ArgumentNullException(nameof(_borderMaterial));
            }

            //Initialize Variables
            _basePosition = transform.position + new Vector3(0.5f, 0.5f);
            _staticProperties = new MaterialPropertyBlock();
            _fallingProperties = new MaterialPropertyBlock();
            _ghostProperties = new MaterialPropertyBlock();

            var gameData = GameData.GetInstance();
            _gridSize = gameData.GridSize;

            //Define Verts & UVs
            const int halfVertCount = BORDER_VERT_COUNT / 2;
            const int quarterVertCount = BORDER_VERT_COUNT / 4;
            const int eitherVertCount = BORDER_VERT_COUNT / 8;

            var verts = new List<Vector3>(new Vector3[BORDER_VERT_COUNT + quarterVertCount]);
            var uvs = new List<Vector2>(new Vector2[BORDER_VERT_COUNT + quarterVertCount]);

            var borderLength = _gridSize.x + _gridSize.y + 4f;

            var uvYCords = new[]
            {
                0 / borderLength,
                (_gridSize.x + 2) / borderLength,
                (_gridSize.x + _gridSize.y + 4) / borderLength,
                (_gridSize.y + 2) / borderLength,
            };

            for (var outerVert = 0; outerVert < quarterVertCount; outerVert++)
            {
                var innerVert = outerVert + quarterVertCount;
                var outerBackVert = outerVert + halfVertCount;
                var innerBackVert = innerVert + halfVertCount;
                var uvLoopVerts = outerVert + BORDER_VERT_COUNT;

                var isRight = outerVert > 0 && outerVert < quarterVertCount - 1;
                var isUpper = outerVert >= eitherVertCount;

                var innerX = isRight ? _gridSize.x : 0;
                var innerY = isUpper ? _gridSize.y : 0;
                var indent = new Vector3(isRight ? 1f : -1f, isUpper ? 1f : -1f);

                verts[outerVert] = new Vector3(innerX, innerY, -0.5f) + indent;
                uvs[outerVert] = new Vector2(0f, uvYCords[outerVert]);
                verts[innerVert] = new Vector3(innerX, innerY, -0.5f);
                uvs[innerVert] = new Vector2(0.25f, uvYCords[outerVert]);
                verts[outerBackVert] = new Vector3(innerX, innerY, 0.5f) + indent;
                uvs[outerBackVert] = new Vector2(0.75f, uvYCords[outerVert]);
                verts[innerBackVert] = new Vector3(innerX, innerY, 0.5f);
                uvs[innerBackVert] = new Vector2(0.5f, uvYCords[outerVert]);

                //UV Verts
                verts[uvLoopVerts] = new Vector3(innerX, innerY, -0.5f) + indent;
                uvs[uvLoopVerts] = new Vector2(1f, uvYCords[outerVert]);
            }

            //Create Triangles
            var triangles = new List<int>(BORDER_VERT_COUNT * 3);

            AddSquares(ref triangles, 0, quarterVertCount, quarterVertCount);
            AddSquares(ref triangles, quarterVertCount, halfVertCount, quarterVertCount);
            AddSquares(ref triangles, halfVertCount, quarterVertCount, quarterVertCount);
            AddSquares(ref triangles, halfVertCount, halfVertCount, quarterVertCount);

            _borderMesh = new Mesh();
            _borderMesh.SetVertices(verts);
            _borderMesh.SetUVs(0, uvs);
            _borderMesh.SetTriangles(triangles, 0);

            _filter.mesh = _borderMesh;
            _renderer.material = _borderMaterial;
        }

        private void AddSquares(ref List<int> triangles, int vertStartIndex, int jump, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var vertIndex = index + vertStartIndex;
                var vertNextIndex = index == count - 1 ? vertStartIndex : vertIndex + 1;
                var vertJumpIndex = vertIndex + jump;
                var vertNextJumpIndex = index == count - 1 ? vertStartIndex + jump : vertJumpIndex + 1;

                triangles.Add(vertIndex);
                triangles.Add(vertNextIndex);
                triangles.Add(vertJumpIndex);

                triangles.Add(vertNextIndex);
                triangles.Add(vertJumpIndex);
                triangles.Add(vertNextJumpIndex);
            }
        }

        public void SetFallingProperties(Block block)
        {
            var blockColors = new Vector4[4];
            var ghostColors = new Vector4[4];
            var textureST = new Vector4[4];
            var outlineL = new Vector4[4];
            var outlineC = new Vector4[4];

            _ghostProperties.SetVectorArray(INSTANCE_OUTLINEL, outlineL);
            _ghostProperties.SetVectorArray(INSTANCE_OUTLINEC, outlineC);

            for (var blockIndex = 0; blockIndex < 4; blockIndex++)
            {
                var col = block.Color.linear;
                blockColors[blockIndex] = col;
                col.a = _ghostTransparency;
                ghostColors[blockIndex] = col;
                textureST[blockIndex] = block.TextureST;
            }

            _fallingProperties.SetVectorArray(INSTANCE_COLORS, blockColors);
            _ghostProperties.SetVectorArray(INSTANCE_COLORS, ghostColors);

            _fallingProperties.SetVectorArray(INSTANCE_ST, textureST);
            _ghostProperties.SetVectorArray(INSTANCE_ST, textureST);

            _fallingProperties.SetVectorArray(INSTANCE_OUTLINEL, outlineL);
            _fallingProperties.SetVectorArray(INSTANCE_OUTLINEC, outlineC);
        }

        public void SetFallingPosition(Vector2Int[] positions, BlockGrid grid)
        {
            var newTransforms = new List<Matrix4x4>();
            var ghostTransforms = new List<Matrix4x4>();

            var offset = 1;
            var didCollide = false;

            while (!didCollide)
            {
                foreach (var position in positions)
                {
                    if (grid.CheckCollision(new Vector2Int(position.x, position.y - offset)))
                    {
                        didCollide = true;
                        offset--;
                        break;
                    }
                }

                if (!didCollide)
                {
                    offset++;
                }
            }

            foreach (var position in positions)
            {
                if (position.y < _gridSize.y)
                {
                    var matrix = new Matrix4x4();
                    matrix.SetTRS(new Vector3(position.x, position.y) + _basePosition, _baseRotation, Vector3.one);
                    newTransforms.Add(matrix);
                }

                var ghost = new Matrix4x4();
                ghost.SetTRS(new Vector3(position.x, position.y - offset) + _basePosition, _baseRotation, Vector3.one);
                ghostTransforms.Add(ghost);
            }

            _ghostTransforms = ghostTransforms.ToArray();
            _fallingTransforms = newTransforms.ToArray();
        }

        public void ResetFalling()
        {
            _fallingTransforms = null;
        }

        public void RenderBlocks(bool renderGhost = true)
        {
            var gameData = GameData.GetInstance();

            if (_blockCount != 0)
            {
                Graphics.DrawMeshInstanced(
                    mesh: gameData.BlockMesh,
                    submeshIndex: 0,
                    material: gameData.BlockMaterial,
                    matrices: _staticTransforms,
                    properties: _staticProperties,
                    castShadows: ShadowCastingMode.Off,
                    receiveShadows: false,
                    count: _staticTransforms.Length);
            }

            if (_fallingTransforms != null)
            {
                Graphics.DrawMeshInstanced(
                    mesh: gameData.BlockMesh,
                    submeshIndex: 0,
                    material: gameData.BlockMaterial,
                    matrices: _fallingTransforms,
                    properties: _fallingProperties,
                    castShadows: ShadowCastingMode.Off,
                    receiveShadows: false,
                    count: _fallingTransforms.Length);
            }
            else
            {
                return;
            }

            if (renderGhost && _ghostTransforms != null)
            {
                Graphics.DrawMeshInstanced(
                    mesh: gameData.BlockMesh,
                    submeshIndex: 0,
                    material: gameData.BlockMaterial,
                    matrices: _ghostTransforms,
                    properties: _ghostProperties,
                    castShadows: ShadowCastingMode.Off,
                    receiveShadows: false,
                    count: _ghostTransforms.Length);
            }
        }

        private void OnValidate()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
        }

        private void OnDrawGizmos()
        {
            var gameData = FindObjectOfType<GameData>();
            var gridSize = gameData.GridSize;
            var position = transform.position;

            //Grid
            Gizmos.color = Color.green;
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Gizmos.DrawWireCube(position + new Vector3(x + 0.5f, y + 0.5f), Vector3.one);
                }
            }

            //Border
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(position + new Vector3(gridSize.x, gridSize.y) / 2, new Vector3(gridSize.x, gridSize.y, 1));
            Gizmos.DrawWireCube(position + new Vector3(gridSize.x, gridSize.y) / 2, new Vector3(gridSize.x + 2, gridSize.y + 2, 1));
        }
    }
}
