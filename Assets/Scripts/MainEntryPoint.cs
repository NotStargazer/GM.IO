using System;
using GM.UI;
using UnityEngine;

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

        [SerializeField] private InputManager _inputManager;
        [SerializeField] private UIRoot _uiRootPrefab;
        [SerializeField] private GlobalResources _globalResources;

        private UIRoot _uiRootInstance;

        private void Awake()
        {
            if (INSTANCE)
            {
                Debug.Log("Duplicate MainEntryPoint loaded, deleting.");
                Destroy(gameObject);
            }

            INSTANCE = this;

            DontDestroyOnLoad(gameObject);

            if (!_inputManager)
            {
                throw new ArgumentNullException(nameof(_inputManager));
            }

            if (!_uiRootPrefab)
            {
                throw new ArgumentNullException(nameof(_uiRootPrefab));
            }

            if (!_globalResources)
            {
                throw new ArgumentNullException(nameof(_globalResources));
            }

            _uiRootInstance = Instantiate(_uiRootPrefab);
            var uiGameObject = _uiRootInstance.gameObject;
            uiGameObject.name = "[Game UI Root]";
            DontDestroyOnLoad(uiGameObject);

            var globalResources = Instantiate(_globalResources);
            globalResources.name = "[Global Resources]";
            DontDestroyOnLoad(globalResources);

            _uiRootInstance.OnStartForGame();
        }

        //Primary Game Loop
        private void Update()
        {
            _uiRootInstance.OnReceiveInputs(_inputManager);
        }
    }
}
