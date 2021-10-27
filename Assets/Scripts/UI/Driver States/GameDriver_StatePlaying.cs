using System;
using GM.Game;
using UnityEngine;

namespace GM.UI
{
    public class GameDriver_StatePlaying : IDriverState
    {

        public void OnStateEnter(DriverState from)
        {

        }

        public void OnStateExit(DriverState to)
        {
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            GameRoot gameRoot = GameRoot.GetInstance();

            if (input.ButtonDown(Actions.Move, out float direction))
            {

            }

            return null;
        }
    }
}
