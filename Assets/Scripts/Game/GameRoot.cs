using UnityEngine;

namespace GM.Game
{
    public class GameRoot : MonoBehaviour
    {
        private static GameRoot INSTANCE;
        public static bool HasInstance => INSTANCE;

        public static GameRoot GetInstance()
        {
            return INSTANCE;
        }

        private void Awake()
        {
            if (INSTANCE)
            {
                Debug.Log("Duplicate GameRoot loaded, deleting.");
                Destroy(gameObject);
            }

            INSTANCE = this;
        }

        //Game Settings
        public DriverState InitialState;
    }
}
