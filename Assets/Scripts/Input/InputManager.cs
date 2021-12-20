using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GM
{
    public enum Actions
    {
        Move = 0,
        Rotation = 1,
        DropLock = 2,
        SonicDrop = 3,
        Hold = 4,
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
            private object _oldValue;

            public InputRead(InputAction action)
            {
                IsPressed = false;
                IsHeld = false;
                IsReleased = false;

                _previousFrame = false;
                _input = action;
                _oldValue = null;
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
                IsPressed = _oldValue != _input.ReadValueAsObject() && _input.phase == InputActionPhase.Performed;
                _oldValue = _input.ReadValueAsObject();
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
            return _actions[action].IsPressed;
        }

        public bool ButtonDown<T>(Actions action, out T outValue) where T : struct
        {
            outValue = _actions[action].ReadValue<T>();

            return _actions[action].IsPressed;
        }

        public bool ButtonHold(Actions action)
        {
            return _actions[action].IsHeld;
        }

        public bool ButtonHold<T>(Actions action, out T outValue) where T : struct
        {
            outValue = _actions[action].ReadValue<T>();
            return _actions[action].IsHeld;
        }

        public bool ButtonUp(Actions action)
        {
            return _actions[action].IsReleased;
        }

        public bool ButtonUp<T>(Actions action, out T outValue) where T : struct
        {
            outValue = _actions[action].ReadValue<T>();
            return _actions[action].IsReleased;
        }
    }

}