using System;
using System.Collections.Generic;
using UnityEngine;

namespace GM.Data
{
    [Serializable]
    public struct BlockSublist
    {
        public List<Vector2Int> Blocks;
    }

    [Serializable]
    public struct BlockData
    {
        public int ID;
        public int StateCount;
        public int GridSize;
        public List<BlockSublist> RotationStates;
        public Color BlockColor;
        public bool CanFloorKick;
    }

    [CreateAssetMenu(fileName = "TetraBlock", menuName = "GM/TetraBlock")]
    public class TetraBlockData : ScriptableObject
    {
        public List<BlockData> TetraBlocks;
    }
}