using IPA;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.UI;
using System.Linq;
using UnityEngine.UI;
using MusicVideoPlayer.YT;
using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;

namespace MusicVideoPlayer
{
    public sealed class Plugin : IBeatSaberPlugin
    {
        public static IPA.Logging.Logger logger;
        
        public void Init(object thisWillBeNull, IPA.Logging.Logger logger)
        {
            Plugin.logger = logger;
        }

        public void OnApplicationStart()
        {
            BSMLSettings.instance.AddSettingsMenu("MVP", "MusicVideoPlayer.Views.settings.bsml", MVPSettings.instance);
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            Base64Sprites.ConvertToSprites();
        }

        private void OnMenuSceneLoadedFresh()
        {
            YouTubeDownloader.OnLoad();
            VideoUI.Instance.OnLoad();
            ScreenManager.OnLoad();
            VideoLoader.OnLoad();
        }

        public void OnApplicationQuit()
        {
            BSEvents.menuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
            YouTubeDownloader.Instance.OnApplicationQuit();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }

        public void OnSceneUnloaded(Scene scene) { }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) { }

        public void OnUpdate() { }

        public void OnFixedUpdate() { }
    }
}