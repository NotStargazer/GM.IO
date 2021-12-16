using System.Collections.Generic;

namespace GM.Data
{
    public struct LevelState
    {
        public int Level;
        public float Gravity;
    }

    //Used as reference point for UI.
    public class GameState
    {
        public List<int> LinesCleared;
        public LevelState LevelState;

        public GameState()
        {
            LinesCleared = new List<int>();
        }

        public void Reset()
        {
            LinesCleared.Clear();
        }
    }
}
