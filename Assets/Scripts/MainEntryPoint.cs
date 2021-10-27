using System;
using GM.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GM
{
    public class MainEntryPoint : MonoBehaviour
    {
        private static MainEntryPoint INSTANCE;

        public static bool HasInstance => INSTANCE;

        public static MainEntryPoint GetInstance()
        {
            if (!INSTANCE)
            {
                throw new Exception("MainEntryPoint has not been instantiated.");
            }

            return INSTANCE;
        }

        [SerializeField] private InputManager _input;
        [SerializeField] private UIRoot _uiRoot;
        
        private GameDriver _gameDriver;

        private void Awake()
        {
            if (INSTANCE)
            {
                Debug.Log("Duplicate MainEntryPoint loaded, deleting.");
                Destroy(gameObject);
            }

            INSTANCE = this;

            DontDestroyOnLoad(gameObject);


            if (!_input)
            {
                throw new ArgumentNullException(nameof(_input));
            }

            if (!_uiRoot)
            {
                throw new ArgumentNullException(nameof(_uiRoot));
            }

            var ui = Instantiate(_uiRoot);
            ui.gameObject.name = "[Game UI Root]";
            DontDestroyOnLoad(ui.gameObject);

            _gameDriver = new GameDriver(ui);
        }

        //Primary Game Loop
        private void Update()
        {
            _gameDriver.OnReceiveInputs(_input);
        }
    }
}
