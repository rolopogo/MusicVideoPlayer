using CustomUI.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MusicVideoPlayer.Util
{
    class BSEvents : MonoBehaviour
    {
        static BSEvents Instance;
        
        //Scene Events
        public static event Action menuSceneLoaded;
        public static event Action menuSceneLoadedFresh;
        public static event Action gameSceneActive;
        public static event Action gameSceneLoaded;

        public static event Action songPaused;
        public static event Action songUnpaused;
        public static event Action songRestarted; // TODO

        public static event Action songSelected; // TODO
        public static event Action difficultySelected; // TODO
        public static event Action gameModeSelected; // TODO

        const string Menu = "Menu";
        const string Game = "GameCore";
        const string EmptyTransition = "EmptyTransition";
        
        public static void OnLoad()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("BSSceneManager");
            go.AddComponent<BSEvents>();
        }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;

            DontDestroyOnLoad(gameObject);
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            try
            {
                if (arg1.name == Game)
                {
                    gameSceneActive?.Invoke();

                    var sceneManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();

                    if (sceneManager != null)
                    {
                        sceneManager.transitionDidFinishEvent -= GameSceneSceneWasLoaded; // make sure we don't ever subscribe twice
                        sceneManager.transitionDidFinishEvent += GameSceneSceneWasLoaded;
                    }
                }
                else if (arg1.name == Menu)
                {
                    if (arg0.name == EmptyTransition)
                    {
                        menuSceneLoadedFresh?.Invoke();
                    }
                    else
                    {
                        menuSceneLoaded?.Invoke();
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine("[BSEvents] " + e); 
            }
        }

        private void GameSceneSceneWasLoaded()
        {
            Resources.FindObjectsOfTypeAll<GamePauseManager>().First().GetPrivateField<Signal>("_gameDidResumeSignal").Subscribe(delegate () { songUnpaused?.Invoke(); });
            Resources.FindObjectsOfTypeAll<GamePauseManager>().First().GetPrivateField<Signal>("_gameDidPauseSignal").Subscribe(delegate () { songPaused?.Invoke(); });

            gameSceneLoaded?.Invoke();
        }
        
    }
}