using UnityEngine;

namespace GM.Game
{
    public class GameData : MonoBehaviour
    {
        private static GameData INSTANCE;
        public static bool HasInstance => INSTANCE;

        public static GameData GetInstance()
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

        //Game Controllers
        public Playfield Playfield;

        //Game Settings
        public DriverState InitialState;
    }
}
