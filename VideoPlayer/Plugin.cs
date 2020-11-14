﻿using IPA;
using UnityEngine.SceneManagement;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.UI;
using MusicVideoPlayer.YT;
using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;
using System.Collections;
using UnityEngine;
using System.Linq;
using BeatSaberMarkupLanguage.GameplaySetup;

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
            GameplaySetup.instance.AddTab("Video", "MusicVideoPlayer.Views.video-menu.bsml", VideoMenu.instance);
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            Base64Sprites.ConvertToSprites();
        }

        private void OnMenuSceneLoadedFresh()
        {
            YouTubeDownloader.OnLoad();
            ScreenManager.OnLoad();
            VideoLoader.OnLoad();
            VideoMenu.instance.OnLoad();
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