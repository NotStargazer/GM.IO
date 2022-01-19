using System;
using UnityEngine;

namespace GM
{
    public class GlobalResources : MonoBehaviour
    {
        private static GlobalResources INSTANCE;
        public static bool HasInstance => INSTANCE;
        public static GlobalResources GetInstance()
        {
            if (!INSTANCE)
            {
                throw new Exception("GlobalResources is not instantiated yet");
            }

            return INSTANCE;
        }

        public SoundController SoundController;

        private void Awake()
        {
            if (INSTANCE)
            {
                Debug.Log("Multiple instances of GlobalResources, deleting.");
                Destroy(this);
                return;
            }

            INSTANCE = this;

            if (!SoundController)
            {
                throw new ArgumentNullException(nameof(SoundController));
            }
        }
    }
}
