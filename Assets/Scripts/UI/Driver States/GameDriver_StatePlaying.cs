using System;
using GM.Data;
using GM.Game;
using UnityEngine;

namespace GM.UI
{
    public class GameDriver_StatePlaying : IDriverState
    {
        private GameLogic _gameLogic;

        public void OnStateEnter(DriverState from)
        {
            _gameLogic = GameData.GetInstance().GameLogic;
            _gameLogic.Initialize();
        }

        public void OnStateExit(DriverState to)
        {
        }

        public DriverState? OnReceiveInputsWithUI(IUIRoot ui, IInput input)
        {
            _gameLogic.LogicUpdate(input);

            return null;
        }
    }
}
