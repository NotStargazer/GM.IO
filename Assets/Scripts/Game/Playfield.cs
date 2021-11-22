using System;
using GM.UI;
using UnityEngine;

namespace GM.Game
{
    public class Playfield : MonoBehaviour
    {
        [SerializeField] private PlayfieldRenderer _renderer;
        [SerializeField] private TetraBlockFactory _blockData;

        public void OnReceiveInputs(IInput input)
        {

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
