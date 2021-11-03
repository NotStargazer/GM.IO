using System;
using GM.Game;
using UnityEngine;

namespace GM.UI
{
    public class GameDriver_StatePlaying : IDriverState
    {
        private Playfield _playfield;

        public void OnStateEnter(DriverState from)
        {
            _playfield = GameData.GetInstance().Playfield;
        }

        public void OnStateExit(DriverState to)
        {
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            _playfield.OnReceiveInputs(input);

            return null;
        }
    }
}
