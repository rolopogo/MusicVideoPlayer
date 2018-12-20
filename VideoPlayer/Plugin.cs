using IllusionPlugin;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using CustomUI.Settings;
using MusicVideoPlayer.Misc;
using MusicVideoPlayer.UI;
using System.Linq;
using UnityEngine.UI;

namespace MusicVideoPlayer
{
    public sealed class Plugin : IPlugin
    {
        public string Name => "Video Player";
        public string Version => "0.0.0";

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
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            Base64Sprites.ConvertToSprites();
            VideoManager.OnLoad();

            Application.logMessageReceived += LogCallback;
        }

        //Called when there is an exception
        void LogCallback(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log) return;
            Console.WriteLine(condition);
            Console.WriteLine(stackTrace);
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            YoutubeDownloader.Instance.OnApplicationQuit();
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

        public void OnActiveSceneChanged(Scene from, Scene newScene)
        {
            if (newScene.name.Contains("Menu")) {
                if (from.name == "EmptyTransition")
                {
                    CreateSettingsUI();
                    VideoUI.Instance.OnLoad();
                }
                VideoManager.Instance.StopVideo();
            }
            if (newScene.name == "GameCore")
            {
                // find video url to play
                string url = VideoFetcher.FetchURLForPlayingSong();
                if (url == null) return;

                VideoManager.Instance.PlayVideo(url);
            }
        }
        
        private static void CreateSettingsUI()
        {
            var subMenu = SettingsUI.CreateSubMenu("VideoPlayer");

            var showVideoSetting = subMenu.AddBool("Show Video");
            showVideoSetting.GetValue += delegate
            {
                return VideoManager.showVideo;
            };
            showVideoSetting.SetValue += delegate (bool value)
            {
                VideoManager.showVideo = value;
                ModPrefs.SetBool(Plugin.PluginName, "ShowVideo", VideoManager.showVideo);
            };

            var placementSetting = subMenu.AddList("Screen Position", VideoPlacementSetting.Modes());
            placementSetting.GetValue += delegate
            {
                return (float)VideoManager.placement;
            };
            placementSetting.SetValue += delegate (float value)
            {
                VideoManager.SetPlacement((VideoPlacement)value);
                ModPrefs.SetInt(Plugin.PluginName, "ScreenPositionMode", (int)VideoManager.placement);
            };
            placementSetting.FormatValue += delegate (float value) { return VideoPlacementSetting.Name((VideoPlacement)value); };


            var qualitySetting = subMenu.AddList("Video Download Quality", VideoQualitySetting.Modes());
            qualitySetting.GetValue += delegate
            {
                return (float)YoutubeDownloader.Instance.quality;
            };
            qualitySetting.SetValue += delegate (float value)
            {
                YoutubeDownloader.Instance.quality = (VideoQuality)value;
                ModPrefs.SetInt(Plugin.PluginName, "VideoDownloadQuality", (int)YoutubeDownloader.Instance.quality);
            };
            qualitySetting.FormatValue += delegate (float value) { return VideoQualitySetting.Name((VideoQuality)value); };
        }
    }
}