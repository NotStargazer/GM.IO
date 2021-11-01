using System;
using System.Collections.Generic;
using System.Globalization;
using GM.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GM.Game
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

        [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private float _excessHeight;
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Material _borderMaterial;
        [SerializeField] private Material _blockMaterial;
        [SerializeField] private Mesh _cubeMesh;
        [SerializeField] private Texture _normalTexture;

        private Mesh _borderMesh;

        private Vector3[] _verts;
        private Vector2[] _uvs;
        private int[] _triangleIndices;

        //Block Properties
        private Vector3 _basePosition;
        private readonly Quaternion _baseRotation = new Quaternion(0, 0, 1, 0);
        private MaterialPropertyBlock _properties;

        private int _blockCount;
        private List<Matrix4x4> _transforms;
        private List<Vector4> _colors;
        private List<Vector4> _textureST;
        private List<Vector4> _outlineLines;
        private List<Vector4> _outlineCorners;

        public Block?[,] Blocks
        {
            set
            {
                _blockCount = value.Length;
                _transforms = new List<Matrix4x4>(_blockCount);
                _colors = new List<Vector4>(_blockCount);
                _textureST = new List<Vector4>(_blockCount);
                _outlineLines = new List<Vector4>(_blockCount);
                _outlineCorners = new List<Vector4>(_blockCount);

                for (var y = 0; y < _gridSize.y; y++)
                {
                    for (var x = 0; x < _gridSize.x; x++)
                    {
                        var val = value[x, y];

                        if (val.HasValue)
                        {
                            var block = val.Value;
                            var position = _basePosition + new Vector3(block.Position.x, block.Position.y);
                            Matrix4x4 matrix = Matrix4x4.zero;
                            matrix.SetTRS(position, _baseRotation, Vector3.one);
                            _transforms.Add(matrix);
                            _colors.Add(block.Color.linear);
                            _textureST.Add(block.TextureST);

                            var xg = x > 0;
                            var xl = x + 1 < _gridSize.x;
                            var yg = y > 0;
                            var yl = y + 1 < _gridSize.y;

                            _outlineLines.Add(new Vector4(
                                x: xg && !value[x - 1, y].HasValue ? 1 : 0,
                                y: xl && !value[x + 1, y].HasValue ? 1 : 0,
                                z: yg && !value[x, y - 1].HasValue ? 1 : 0,
                                w: yl && !value[x, y + 1].HasValue ? 1 : 0));

                            _outlineCorners.Add(new Vector4(
                                x: xg && yg && !value[x - 1, y - 1].HasValue ? 1 : 0,
                                y: xl && yg && !value[x + 1, y - 1].HasValue ? 1 : 0,
                                z: xg && yl && !value[x - 1, y + 1].HasValue ? 1 : 0,
                                w: xl && yl && !value[x + 1, y + 1].HasValue ? 1 : 0));
                        }
                    }
                }

                _properties.SetVectorArray(INSTANCE_COLORS, _colors);
                _properties.SetVectorArray(INSTANCE_ST, _textureST);
                _properties.SetVectorArray(INSTANCE_OUTLINEL, _outlineLines);
                _properties.SetVectorArray(INSTANCE_OUTLINEC, _outlineCorners);
            }
        }

        private void Awake()
        {
            if (!_borderMaterial)
            {
                throw new ArgumentNullException(nameof(_borderMaterial));
            }
            if (!_cubeMesh)
            {
                throw new ArgumentNullException(nameof(_cubeMesh));
            }
            if (!_normalTexture)
            {
                throw new ArgumentNullException(nameof(_normalTexture));
            }

            _basePosition = transform.position + new Vector3(0.5f, 0.5f);

            //Debug Blocks
            _properties = new MaterialPropertyBlock();
            var blockList = new Block?[_gridSize.x, _gridSize.y];

            for (var y = 0; y < _gridSize.y; y++)
            {
                for (var x = 0; x < _gridSize.x; x++)
                {
                    if ((x + y) / 2 % 2 == 1)
                    {
                        blockList[x, y] = null;
                        continue;
                    }

                    float hue;

                    if (x + y == 0)
                    {
                        hue = 0;
                    }
                    else
                    {
                        hue = (x + y) / (float)(_gridSize.x + _gridSize.y);
                    }

                    blockList[x, y] = new Block
                    {
                        Color = Color.HSVToRGB(hue, 1, 1),
                        Position = new Vector2Int(x, y),
                        TextureST = new Vector4(0.5f, 1f, 1f, 1f)
                    };
                }
            }

            Blocks = blockList;

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

            _verts = verts.ToArray();
            _uvs = uvs.ToArray();

            //Create Triangles
            var triangles = new List<int>(BORDER_VERT_COUNT * 3);

            AddSquares(ref triangles, 0, quarterVertCount, quarterVertCount);
            AddSquares(ref triangles, quarterVertCount, halfVertCount, quarterVertCount);
            AddSquares(ref triangles, halfVertCount, quarterVertCount, quarterVertCount);
            AddSquares(ref triangles, halfVertCount, halfVertCount, quarterVertCount);

            _triangleIndices = triangles.ToArray();

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

        private void OnValidate()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            Graphics.DrawMeshInstanced(
                mesh: _cubeMesh,
                submeshIndex: 0,
                material: _blockMaterial,
                matrices: _transforms.ToArray(),
                properties: _properties,
                castShadows: ShadowCastingMode.Off,
                receiveShadows: false,
                count: _transforms.Count,
                layer: (int)Layer.Outline);

            //Render Blocks
            //foreach (var block in Blocks)
            //{
            //    _properties.SetColor(BASE_COLOR, block.Color);
            //    _properties.SetTexture(BASE_MAP, block.Texture);

            //    Vector3 blockPos = new Vector3(block.Position.x, block.Position.y) + _basePosition;

            //    Graphics.DrawMesh(
            //        mesh: _cubeMesh,
            //        position: blockPos,
            //        rotation: _baseRotation,
            //        material: _blockMaterial,
            //        properties: _properties,
            //        layer: (int)Layer.Outline,
            //        camera: null,
            //        submeshIndex: 0,
            //        castShadows: false
            //    );
            //}

            /*
            var position = transform.position;

            foreach (var vert in _verts)
            {
                Debug.DrawLine(
                    new Vector3(0.25f, 0.25f) + vert + position,
                    new Vector3(-0.25f, -0.25f) + vert + position);
                Debug.DrawLine(
                    new Vector3(0.25f, -0.25f) + vert + position,
                    new Vector3(-0.25f, 0.25f) + vert + position);
            }
            for (var index = 0; index < _triangleIndices.Length; index += 3)
            {
                var tri1 = _triangleIndices[index];
                var tri2 = _triangleIndices[index + 1];
                var tri3 = _triangleIndices[index + 2];

                Debug.DrawLine(_verts[tri1] + position, _verts[tri2] + position);
                Debug.DrawLine(_verts[tri2] + position, _verts[tri3] + position);
                Debug.DrawLine(_verts[tri3] + position, _verts[tri1] + position);
            }
            */
        }

        private void OnDrawGizmos()
        {
            var position = transform.position;

            if (Application.IsPlaying(this))
            {
                if (_verts == null)
                {
                    return;
                }

                for (int i = 0; i < _verts.Length; i++)
                {
                    Handles.Label(position + _verts[i], (_uvs[i].y * (_gridSize.x + _gridSize.y + 4)).ToString(CultureInfo.InvariantCulture));
                }

                return;
            }

            //Grid
            Gizmos.color = Color.green;
            for (int x = 0; x < _gridSize.x; x++)
            {
                for (int y = 0; y < _gridSize.y; y++)
                {
                    Gizmos.DrawWireCube(position + new Vector3(x + 0.5f, y + 0.5f), Vector3.one);
                }
            }

            //Border
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(position + new Vector3(_gridSize.x, _gridSize.y) / 2, new Vector3(_gridSize.x, _gridSize.y, 1));
            Gizmos.DrawWireCube(position + new Vector3(_gridSize.x, _gridSize.y) / 2, new Vector3(_gridSize.x + 2, _gridSize.y + 2, 1));


        }
    }
}
