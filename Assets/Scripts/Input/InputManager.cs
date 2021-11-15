using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GM
{
    public enum Actions
    {
        Move = 0,
        Rotation = 1,
        DropLock = 2,
        SonicDrop = 3
    }

    public interface IInput
    {
        public bool ButtonDown(Actions action);
        public bool ButtonDown<T>(Actions action, out T outValue) where T : struct;
        public bool ButtonHold(Actions action);
        public bool ButtonHold<T>(Actions action, out T outValue) where T : struct;
        public bool ButtonUp(Actions action);
        public bool ButtonUp<T>(Actions action, out T outValue) where T : struct;
    }

    public class InputManager : MonoBehaviour, IInput
    {
        [SerializeField] private PlayerInput _gmInput;

        struct InputRead
        {
            public bool IsPressed { get; private set; }
            public bool IsHeld { get; private set; }
            public bool IsReleased { get; private set; }

            private bool _previousFrame;
            private readonly InputAction _input;

            public InputRead(InputAction action)
            {
                IsPressed = false;
                IsHeld = false;
                IsReleased = false;

                _previousFrame = false;
                _input = action;
            }

            public void UpdateInput()
            {
                var triggered = _input.phase == InputActionPhase.Performed;
                IsPressed = !_previousFrame && triggered;
                IsHeld = triggered;
                IsReleased = _previousFrame && !triggered;

                _previousFrame = triggered;
            }

            public T ReadValue<T>() where T : struct
            {
                return _input.ReadValue<T>();
            }
        }

        private Dictionary<Actions, InputRead> _actions;

        public void Awake()
        {
            _actions = new Dictionary<Actions, InputRead>();

            foreach (var value in Enum.GetValues(typeof(Actions)))
            {
                var action = _gmInput.actions.actionMaps[0].actions[(int)value];
                _actions.Add((Actions)value, new InputRead(action));
            }
        }

        private void Update()
        {
            foreach (Actions value in Enum.GetValues(typeof(Actions)))
            {
                var action = _actions[value];
                action.UpdateInput();
                _actions[value] = action;
            }
        }

        public bool ButtonDown(Actions action)
        {
            var buttonAction = _actions[action];

            return buttonAction.IsPressed;
        }

        public bool ButtonDown<T>(Actions action, out T outValue) where T : struct
        {
            var buttonAction = _actions[action];

            outValue = buttonAction.ReadValue<T>();

            return buttonAction.IsPressed;
        }

        public bool ButtonHold(Actions action)
        {
            var buttonAction = _actions[action];

            return buttonAction.IsHeld;
        }

        public bool ButtonHold<T>(Actions action, out T outValue) where T : struct
        {
            var buttonAction = _actions[action];

            outValue = buttonAction.ReadValue<T>();
            return buttonAction.IsHeld;
        }

        public bool ButtonUp(Actions action)
        {
            var buttonAction = _actions[action];

            return buttonAction.IsReleased;
        }

        public bool ButtonUp<T>(Actions action, out T outValue) where T : struct
        {
            var buttonAction = _actions[action];

            outValue = buttonAction.ReadValue<T>();
            return buttonAction.IsReleased;
        }
    }

}