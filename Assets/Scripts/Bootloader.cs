using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GM
{
    public class Bootloader : MonoBehaviour
    {
        public static string BOOTLOADER_EDITOR_SCENE_PREF = "initialScene";

        [SerializeField] private GameObject _entryPoint;

        private AsyncOperation _sceneLoadOp;

        private IEnumerator Start()
        {
            if (!_entryPoint)
            {
                throw new ArgumentNullException(nameof(_entryPoint));
            }

            DontDestroyOnLoad(gameObject);
            gameObject.name = "[Bootloader]";

            //Main Entry Point
            var mainEntryPoint = Instantiate(_entryPoint.gameObject);
            mainEntryPoint.name = "[MainEntryPoint]";
            DontDestroyOnLoad(mainEntryPoint);

            //Game UIRoot

            //Load Scene
            var path = EditorPrefs.GetString(BOOTLOADER_EDITOR_SCENE_PREF, null);

            path = path == "" ? EditorBuildSettings.scenes[1].path : path;

            _sceneLoadOp = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);

            yield return new WaitUntil(InitialSceneLoadDone);

            Destroy(gameObject);
        }

        private bool InitialSceneLoadDone()
        {
            return _sceneLoadOp.progress >= 1;
        }
    }
}
