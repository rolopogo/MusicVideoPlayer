using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using BS_Utils.Utilities;
using HMUI;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.ViewControllers;
using MusicVideoPlayer.YT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MusicVideoPlayer.UI
{
    public class VideoMenu : PersistentSingleton<VideoMenu>
    {
        private GameplaySetupViewController gameplaySetupViewController;
        IPreviewBeatmapLevel selectedLevel;

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
        }

        public void LoadVideoSettings(VideoData selectedVideo)
        {
            Plugin.logger.Debug("Load Video Settings");
            if (selectedVideo == null)
            {
                selectedVideo = VideoLoader.Instance.GetVideo(selectedLevel);
            }
            
            if(selectedVideo != null)
            {
                ScreenManager.Instance.PrepareVideo(selectedVideo);
                ScreenManager.Instance.HideScreen();

                if (selectedVideo.downloadState == DownloadState.Queued)
                {
                }
                else if (selectedVideo.downloadState == DownloadState.Downloading)
                {
                }
                else if (selectedVideo.downloadState == DownloadState.Downloaded)
                {
                    ScreenManager.Instance.ShowScreen();
                }
                else
                {
                }
            }
            else
            {
                Plugin.logger.Debug("Failed to load: " + selectedLevel.songName);
            }
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

        private VideoDetailsViewController controller = new VideoDetailsViewController();
        #region Gameplay Setup View Controller
        private void OnActivate(bool firstActivation, ViewController.ActivationType activationType)
        {
            //GameplaySetup.instance.AddTab("Video", "MusicVideoPlayer.Views.videoMenu.bsml", this);
        }

        #endregion

        #region BS Events
        public void HandleDidSelectLevel(LevelCollectionViewController sender, IPreviewBeatmapLevel level)
        {
            selectedLevel = level;
            LoadVideoSettings(null);
        }
        #endregion
    }
}
