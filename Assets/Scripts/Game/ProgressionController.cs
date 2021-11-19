using UnityEngine;

namespace GM.Game
{
    public struct ProgressionState
    {
        public float SpawnDuration;
        public float DropDuration;
        public float LockDuration;
        public float LineDuration;
    }

    public class ProgressionController : MonoBehaviour
    {
        private const float FRAME = 0.0166666667f;

        public float FastDropDuration => FRAME;

        public ProgressionState CurrentState =>
            new ProgressionState
            {
                SpawnDuration = FRAME * 25,
                DropDuration = FRAME * 64,
                LockDuration = FRAME * 30,
                LineDuration = FRAME * 32
            };

        public void Initialize()
        {

        }

        public void IncrementLevel()
        {

        }
    }
}
