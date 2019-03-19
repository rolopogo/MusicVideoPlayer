using IllusionPlugin;
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
    public sealed class Plugin : IPlugin
    {
        public string Name => "Music Video Player";
        public string Version => "1.2.0";

        public static Plugin Instance;

        public static string PluginName
        {
            get
            {
                return Instance.Name;
            }
        }

        public void OnApplicationStart()
        {
            Instance = this;
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            Base64Sprites.ConvertToSprites();

            //Application.logMessageReceived += LogCallback;
        }

        //Called when there is an exception
        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log) return;
            Console.WriteLine(condition);
            Console.WriteLine(stackTrace);
        }

        void IPlugin.OnApplicationQuit()
        {
            BSEvents.menuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
            YouTubeDownloader.Instance.OnApplicationQuit();
        }

        #region Unused
        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
        #endregion

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
                ModPrefs.SetBool(Plugin.PluginName, "ShowVideo", ScreenManager.showVideo);
            };

            var placementSetting = subMenu.AddList("Screen Position", VideoPlacementSetting.Modes());
            placementSetting.GetValue += delegate
            {
                return (float)ScreenManager.Instance.placement;
            };
            placementSetting.SetValue += delegate (float value)
            {
                ScreenManager.Instance.SetPlacement((VideoPlacement)value);
                ModPrefs.SetInt(Plugin.PluginName, "ScreenPositionMode", (int)ScreenManager.Instance.placement);
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
                ModPrefs.SetInt(Plugin.PluginName, "VideoDownloadQuality", (int)YouTubeDownloader.Instance.quality);
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
                ModPrefs.SetBool(Plugin.PluginName, "AutoDownload", ScreenManager.showVideo);
            };
        }
    }
}