using System;
using GM.Game;
using GM.UI;
using GM.UI.Playfield;
using UnityEngine;

namespace GM.Data
{
    [RequireComponent(typeof(GameLogic))]
    public class GameData : MonoBehaviour
    {
        private static GameData INSTANCE;
        public static bool HasInstance => INSTANCE;

        public static GameData GetInstance()
        {
            return INSTANCE;
        }

        private void OnValidate()
        {
            GameLogic = GetComponent<GameLogic>();
        }

        //Game Controllers
        public GameLogic GameLogic;

        //Game Settings
        public DriverState InitialState;

        //Progression Data
        public ProgressionData ProgressionData;

        //TetraBlock Data

        [Header("Playfield Data")]
        public Vector2Int GridSize;
        public int ExcessHeight;
        public Mesh BlockMesh;
        public Material BlockMaterial;
        public PlayfieldUI PlayfieldUI;

        private void Awake()
        {
            if (INSTANCE)
            {
                Debug.Log("Multiple GameData objects detected, you don't want this, deleting.");
                Destroy(gameObject);
            }

            INSTANCE = this;

            if (!BlockMesh)
            {
                throw new ArgumentNullException(nameof(BlockMesh));
            }

            if (!BlockMaterial)
            {
                throw new ArgumentNullException(nameof(BlockMaterial));
            }

            if (!PlayfieldUI)
            {
                throw new ArgumentNullException(nameof(PlayfieldUI));
            }

            if (!ProgressionData)
            {
                throw new ArgumentNullException(nameof(ProgressionData));
            }
        }
    }
}
