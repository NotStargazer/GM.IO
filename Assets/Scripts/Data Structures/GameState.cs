using System.Collections.Generic;
using GM.Game;
using GM.UI.Playfield;
using UnityEngine;

namespace GM.Data
{
    public struct DestroyedLine
    {
        public int LineNumber;
        public Block[] Blocks;
    }

    public struct LevelState
    {
        public int Level;
        public float Gravity;
    }

    //Used as reference point for UI.
    public class GameState
    {
        public List<DestroyedLine> LinesCleared;
        public LevelState LevelState;
        public Vector2? GameOverCenter;
        public AlertType? Alert;
        public ProgressionAssets? ProgressionAssets;

        public GameState()
        {
            LinesCleared = new List<DestroyedLine>();
        }


        public void Reset()
        {
            LinesCleared.Clear();
            Alert = null;
            ProgressionAssets = null;
        }
    }
}
