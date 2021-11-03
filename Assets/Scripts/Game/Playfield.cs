using System;
using UnityEngine;

namespace GM.Game
{
    public class Playfield : MonoBehaviour
    {
        [SerializeField] private PlayfieldRenderer _renderer;
        [SerializeField] private Randomizer _blockData;

        private TetraBlock _tetraBlock;

        private Block?[,] _grid;

        

        public void OnReceiveInputs(IInput input)
        {
            if (input.ButtonDown(Actions.Move, out float direction))
            {
                _tetraBlock.Move(direction > 0 ? Direction.Right : Direction.Left, _grid);
            }
        }

        private void Awake()
        {
            if (!_renderer)
            {
                throw new ArgumentNullException(nameof(_renderer));
            }
        }
    }
}
