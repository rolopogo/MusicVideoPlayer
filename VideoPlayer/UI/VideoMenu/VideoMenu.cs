using BeatSaberMarkupLanguage.Attributes;
using BS_Utils.Utilities;
using HMUI;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.YT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MusicVideoPlayer
{
    public class VideoMenu : PersistentSingleton<VideoMenu>
    {
        private GameplaySetupViewController gameplaySetupViewController;
        IPreviewBeatmapLevel selectedLevel;
        VideoData selectedVideo;

        public void OnLoad()
        {
            Setup();
        }

        internal void Setup()
        {
            gameplaySetupViewController = Resources.FindObjectsOfTypeAll<GameplaySetupViewController>().First();
            gameplaySetupViewController.didActivateEvent -= OnActivate;
            gameplaySetupViewController.didActivateEvent += OnActivate;
            YouTubeDownloader.Instance.downloadProgress += VideoDownloaderDownloadProgress;
            BSEvents.levelSelected += HandleDidSelectLevel;
            BSEvents.gameSceneLoaded += GameSceneLoaded;
            BSEvents.menuSceneActive += MenuSceneActive;
        }

        public void LoadVideoSettings(VideoData videoData)
        {
            Plugin.logger.Debug("Load Video Settings");
            if (videoData == null)
            {
                videoData = VideoLoader.Instance.GetVideo(selectedLevel);
            }

            selectedVideo = videoData;
            ScreenManager.Instance.PrepareVideo(videoData);
        }

        #region Youtube Downloader
        private void VideoDownloaderDownloadProgress(VideoData video)
        {
            if (selectedLevel == video.level)
            {
                LoadVideoSettings(video);
            }
        }
        #endregion

        #region Gameplay Setup View Controller

        #endregion

        #region BS Events
        public void HandleDidSelectLevel(LevelCollectionViewController sender, IPreviewBeatmapLevel level)
        {
            selectedLevel = level;
            LoadVideoSettings(null);
        }

        private void MenuSceneActive()
        {
            ScreenManager.Instance.ShowScreen();
        }

        private void GameSceneLoaded()
        {
            if (selectedVideo == null || selectedVideo.downloadState != DownloadState.Downloaded)
            {
                ScreenManager.Instance.HideScreen();
            }
        }
        #endregion
    }
}
