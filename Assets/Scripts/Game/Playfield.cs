using System;
using UnityEngine;

namespace GM.Game
{
    public class Playfield : MonoBehaviour
    {
        [SerializeField] private PlayfieldRenderer _renderer;
        
        private TetraBlock _tetraBlock;

        private void Awake()
        {
            if (!_renderer)
            {
                throw new ArgumentNullException(nameof(_renderer));
            }
        }
    }
}
