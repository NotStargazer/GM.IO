using System.Collections.Generic;

namespace GM.Data
{
    //Used as reference point for UI.
    public class GameState
    {
        public List<int> LinesCleared;

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
