using IPA;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using CustomUI.Settings;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.UI;
using System.Linq;
using UnityEngine.UI;
using MusicVideoPlayer.YT;

namespace MusicVideoPlayer
{
    public sealed class Plugin : IBeatSaberPlugin
    {
        public static IPA.Logging.Logger logger;
        public static BS_Utils.Utilities.Config config;
        
        public void Init(object thisWillBeNull, IPA.Logging.Logger logger)
        {
            Plugin.logger = logger;
        }

        public void OnApplicationStart()
        {
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            config = new BS_Utils.Utilities.Config("MVP");
            Base64Sprites.ConvertToSprites();
        }

        private void OnMenuSceneLoadedFresh()
        {
            YouTubeDownloader.OnLoad();
            VideoUI.Instance.OnLoad();
            ScreenManager.OnLoad();
            VideoLoader.OnLoad();
            CreateSettingsUI();
        }
        
        private static void CreateSettingsUI()
        {
            var subMenu = SettingsUI.CreateSubMenu("VideoPlayer");

            var showVideoSetting = subMenu.AddBool("Show Video");
            showVideoSetting.GetValue += delegate
            {
                return ScreenManager.showVideo;
            };
            showVideoSetting.SetValue += delegate (bool value)
            {
                ScreenManager.showVideo = value;
                config.SetBool("Settings", "ShowVideo", ScreenManager.showVideo);
            };

            var placementSetting = subMenu.AddList("Screen Position", VideoPlacementSetting.Modes());
            placementSetting.GetValue += delegate
            {
                return (float)ScreenManager.Instance.placement;
            };
            placementSetting.SetValue += delegate (float value)
            {
                ScreenManager.Instance.SetPlacement((VideoPlacement)value);
                config.SetInt("Settings", "ScreenPositionMode", (int)ScreenManager.Instance.placement);
            };
            placementSetting.FormatValue += delegate (float value) { return VideoPlacementSetting.Name((VideoPlacement)value); };


            var qualitySetting = subMenu.AddList("Video Download Quality", VideoQualitySetting.Modes());
            qualitySetting.GetValue += delegate
            {
                return (float)YouTubeDownloader.Instance.quality;
            };
            qualitySetting.SetValue += delegate (float value)
            {
                YouTubeDownloader.Instance.quality = (VideoQuality)value;
                config.SetInt("Settings", "VideoDownloadQuality", (int)YouTubeDownloader.Instance.quality);
            };
            qualitySetting.FormatValue += delegate (float value) { return VideoQualitySetting.Name((VideoQuality)value); };


            var autoDownloadSetting = subMenu.AddBool("Auto Download");
            autoDownloadSetting.GetValue += delegate
            {
                return VideoLoader.Instance.autoDownload;
            };
            autoDownloadSetting.SetValue += delegate (bool value)
            {
                VideoLoader.Instance.autoDownload = value;
                config.SetBool("Settings", "AutoDownload", ScreenManager.showVideo);
            };
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