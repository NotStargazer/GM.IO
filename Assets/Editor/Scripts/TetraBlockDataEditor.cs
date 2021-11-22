using System.Collections.Generic;
using GM.Data;
using UnityEditor;
using UnityEngine;

namespace GM.Editor
{
    [CustomEditor(typeof(TetraBlockData))]
    public class TetraBlockDataEditor : UnityEditor.Editor
    {
        private int _currentPieceIndex;
        private int _stateIndex;

        public override void OnInspectorGUI()
        {
            var data = (TetraBlockData)target;
            Undo.RecordObject(data, "TetraBlock Editor");

            data.TetraBlocks ??= new List<BlockData>();
            var tetraBlockMaxIndex = Mathf.Clamp(data.TetraBlocks.Count - 1, 0, data.TetraBlocks.Count - 1);

            GUILayout.Label($"TetraBlock: {_currentPieceIndex + 1} Total: {data.TetraBlocks.Count}");
            GUILayout.Label($"Rotation State: {_stateIndex + 1}");

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Previous Piece", GUILayout.Width(150)))
                {
                    _currentPieceIndex -= 1;
                    _currentPieceIndex = _currentPieceIndex < 0 ? tetraBlockMaxIndex : _currentPieceIndex;
                    _stateIndex = 0;
                }

                if (GUILayout.Button("Next Piece", GUILayout.Width(150)))
                {
                    if (data.TetraBlocks.Count > 0)
                    {
                        _currentPieceIndex += 1;
                        _currentPieceIndex %= data.TetraBlocks.Count;
                        _stateIndex = 0;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("New Piece", GUILayout.Width(150)))
                {
                    data.TetraBlocks.Add(new BlockData
                    {
                        ID = data.TetraBlocks.Count,
                        RotationStates = new List<BlockSublist>(),
                        BlockColor = Color.red,
                        GridSize = 2,
                        StateCount = 1
                    });
                    _currentPieceIndex = data.TetraBlocks.Count - 1;
                }
                if (GUILayout.Button("Delete Piece", GUILayout.Width(150)))
                {
                    if (data.TetraBlocks.Count > 0)
                    {
                        data.TetraBlocks.RemoveAt(_currentPieceIndex);
                        if (data.TetraBlocks.Count == 0)
                        {
                            return;
                        }
                        _currentPieceIndex %= data.TetraBlocks.Count;

                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (data.TetraBlocks.Count == 0)
            {
                return;
            }

            var tetra = data.TetraBlocks;

            var blockData = tetra[_currentPieceIndex];

            Texture2D blockColor = new Texture2D(50, 50);
            var bcol = new Color[50 * 50];
            Texture2D emptyColor = new Texture2D(50, 50);
            var ecol = new Color[50 * 50];
            for (int i = 0; i < bcol.Length; i++)
            {
                bcol[i] = blockData.BlockColor;
                ecol[i] = Color.black;
            }

            blockColor.SetPixels(bcol);
            emptyColor.SetPixels(ecol);
            blockColor.Apply();
            emptyColor.Apply();

            //Value Editor
            blockData.Name = EditorGUILayout.TextField("Name", blockData.Name);
            blockData.BlockColor = EditorGUILayout.ColorField("Block Color", blockData.BlockColor);
            blockData.GridSize = Mathf.Clamp(EditorGUILayout.IntField("Grid Size", blockData.GridSize), 2, 4);
            blockData.StateCount = Mathf.Clamp(EditorGUILayout.IntField("Rotation State Count", blockData.StateCount), 1, 4);
            blockData.CanFloorKick = EditorGUILayout.Toggle("Can Piece Floor Kick", blockData.CanFloorKick);
            blockData.ExcludeFromGameStart = EditorGUILayout.Toggle("Excluded from game start", blockData.ExcludeFromGameStart);

            //State Editor
            if (blockData.RotationStates.Count != blockData.StateCount)
            {
                while (blockData.RotationStates.Count != blockData.StateCount)
                {
                    if (blockData.RotationStates.Count < blockData.StateCount)
                    {
                        blockData.RotationStates.Add(new BlockSublist
                        {
                            Blocks = new List<Vector2Int>
                            {
                                new Vector2Int(0, 0),
                                new Vector2Int(1, 0),
                                new Vector2Int(0, 1),
                                new Vector2Int(1, 1)
                            }
                        });
                    }
                    else if (blockData.RotationStates.Count > blockData.StateCount)
                    {
                        blockData.RotationStates.RemoveAt(blockData.RotationStates.Count - 1);
                    }

                    _stateIndex %= blockData.RotationStates.Count;
                }
            }

            tetra[_currentPieceIndex] = blockData;

            var blockDataRotationState = blockData.RotationStates[_stateIndex];

            for (var y = blockData.GridSize - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();
                for (var x = 0; x < blockData.GridSize; x++)
                {
                    if (blockDataRotationState.Blocks.Contains(new Vector2Int(x, y)))
                    {
                        if (GUILayout.Button(blockColor, GUILayout.Width(75f), GUILayout.Height(75f)))
                        {
                            blockDataRotationState.Blocks.Remove(new Vector2Int(x, y));
                            tetra[_currentPieceIndex] = blockData;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(emptyColor, GUILayout.Width(75f), GUILayout.Height(75f)))
                        {
                            blockDataRotationState.Blocks.Add(new Vector2Int(x, y));
                            if (blockDataRotationState.Blocks.Count > 4)
                            {
                                blockDataRotationState.Blocks.RemoveAt(0);
                                tetra[_currentPieceIndex] = blockData;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Previous Rotation", GUILayout.Width(150)))
                {
                    _stateIndex -= 1;
                    _stateIndex = _stateIndex < 0 ? blockData.StateCount - 1 : _stateIndex;
                }

                if (GUILayout.Button("Next Rotation", GUILayout.Width(150)))
                {
                    _stateIndex += 1;
                    _stateIndex %= blockData.StateCount;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorUtility.SetDirty(data);
        }
    }
}
