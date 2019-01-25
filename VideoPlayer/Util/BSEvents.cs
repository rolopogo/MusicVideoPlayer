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

        public static event Action gamePaused;
        public static event Action gameUnpaused;

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
                    GameSceneWasMadeActive();

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
                        MenuSceneWasLoadedFresh();
                    }
                    else
                    {
                        MenuSceneWasLoaded();
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }

        private void GameSceneSceneWasLoaded()
        {
            Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().First().GetPrivateField<IPauseTrigger>("_pauseTrigger").pauseTriggeredEvent += GameWasPaused;

            Resources.FindObjectsOfTypeAll<PauseAnimationController>().First().resumeFromPauseAnimationDidFinishEvent += GameWasUnpaused;

            gameSceneLoaded?.Invoke();
        }

        private void GameSceneWasMadeActive()
        {   
            gameSceneActive?.Invoke();
        }

        private void MenuSceneWasLoaded()
        {
            menuSceneLoaded?.Invoke();
        }

        private void MenuSceneWasLoadedFresh()
        {
            menuSceneLoadedFresh?.Invoke();
        }

        private void GameWasPaused()
        {
            gamePaused?.Invoke();
        }

        private void GameWasUnpaused()
        {
            gameUnpaused?.Invoke();
        }
    }
}