using GM.Data;
using UnityEngine;

namespace GM.Game
{
    public class Randomizer : MonoBehaviour
    {
        [SerializeField] private TetraBlockData _blockData;
        [SerializeField] private int _pieceHistoryCount;
        [SerializeField] private int _pieceRetryCount;

        public TetraBlock GetNext(BlockGrid grid)
        {
            return new TetraBlock(
                _blockData.TetraBlocks[Random.Range(0, _blockData.TetraBlocks.Count)],
                new Vector4(0.5f, 1),
                grid);
        }
    }
}
