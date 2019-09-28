using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using CustomUI.BeatSaber;
using MusicVideoPlayer.UI.ViewControllers;
using VRUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MusicVideoPlayer.Util;
//using SongLoaderPlugin;
using MusicVideoPlayer.YT;
using SongCore.Utilities;
using Object = System.Object;

namespace MusicVideoPlayer.UI
{
    class VideoFlowCoordinator : FlowCoordinator
    {
        public event Action<VideoData> finished;

        private MainFlowCoordinator _mainFlowCoordinator;
        private FlowCoordinator _freePlayFlowCoordinator;
        private SongPreviewPlayer songPreviewPlayer;

        private SearchKeyboardViewController _searchViewController;
        private VideoListViewController _videoListViewController;
        private VideoDetailViewController _videoDetailViewController;
        private SimpleDialogPromptViewController _simpleDialog;

        private IPreviewBeatmapLevel selectedLevel;
        private VideoData selectedLevelVideo;
        private bool previewPlaying;

        public void Init()
        {
            _mainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault();
            _mainFlowCoordinator.GetPrivateField<MainMenuViewController>("_mainMenuViewController").didFinishEvent += SongListTweaks_didFinishEvent;
            BSEvents.levelSelected += HandleDidSelectLevel;
            YouTubeDownloader.Instance.downloadProgress += VideoDownloaderDownloadProgress;
             songPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            title = "Video - " + selectedLevel.songName;
            Plugin.logger.Info("Did Activate 01");
            if (_videoDetailViewController == null)
            {
                _videoDetailViewController = BeatSaberUI.CreateViewController<MusicVideoPlayer.UI.ViewControllers.VideoDetailViewController>();
                _videoDetailViewController.Init();
                Plugin.logger.Info(_videoDetailViewController.ToString());
//                _videoDetailViewController.Init();
                Plugin.logger.Info("Did Activate 3");
                _videoDetailViewController.backButtonPressed += DetailViewBackPressed;
                Plugin.logger.Info("Did Activate 4");
                _videoDetailViewController.addOffsetPressed += DetailViewAddOffsetPressed;
                Plugin.logger.Info("Did Activate 5");
                _videoDetailViewController.subOffsetPressed += DetailViewSubOffsetPressed;
                Plugin.logger.Info("Did Activate 6");
                _videoDetailViewController.previewButtonPressed += DetailViewPreviewPressed;
                Plugin.logger.Info("Did Activate 7");
                _videoDetailViewController.loopButtonPressed += DetailViewLoopPressed;
                Plugin.logger.Info("Did Activate 8");
                _videoDetailViewController.listButtonPressed += DetailViewSearchPressed;
                Plugin.logger.Info("Did Activate 9");
                _videoDetailViewController.downloadDeleteButtonPressed += DetailViewDownloadDeletePressed;
            }
            Plugin.logger.Info("Made _videoDetailViewController");
            if (_videoListViewController == null)
            {
                _videoListViewController = BeatSaberUI.CreateViewController<VideoListViewController>();
                _videoListViewController.backButtonPressed += ListViewBackPressed;
                _videoListViewController.downloadButtonPressed += ListViewDownloadPressed;
                _videoListViewController.searchButtonPressed += ListViewSearchPressed;
                Plugin.logger.Info("vLVC Made");
            }
            Plugin.logger.Info("Made _videoListViewController y");
            if (_simpleDialog == null)
            {
                _simpleDialog = Resources.FindObjectsOfTypeAll<SimpleDialogPromptViewController>().First();
                _simpleDialog = Instantiate(_simpleDialog.gameObject, _simpleDialog.transform.parent).GetComponent<SimpleDialogPromptViewController>();
            }
            Plugin.logger.Info("Made _simpleDialog");
            if (activationType == FlowCoordinator.ActivationType.AddedToHierarchy)
            {
                _videoDetailViewController.SetContent(selectedLevelVideo);
                Plugin.logger.Info("Content Set");
                previewPlaying = false;
                _videoDetailViewController.SetPreviewState(previewPlaying);
                Plugin.logger.Info("Preview State Set");
                if (selectedLevelVideo != null)
                {
                    Plugin.logger.Info("Showing Screen");
                    ScreenManager.Instance.ShowScreen();
                    Plugin.logger.Info("Showed Screen");
                }
                Plugin.logger.Info("activationType == FlowCoordinator.ActivationType.AddedToHierarchy");
                ProvideInitialViewControllers(_videoDetailViewController, null, null);
            }
            Plugin.logger.Info("Made activationType == FlowCoordinator.ActivationType.AddedToHierarchy");
        }

        private void DetailViewDownloadDeletePressed()
        {
            Plugin.logger.Info("Downloaded-Delete");
            switch (selectedLevelVideo.downloadState) { 
                case DownloadState.Downloaded:
                    VideoLoader.Instance.DeleteVideo(selectedLevelVideo);
                    selectedLevelVideo = null;
                    _videoDetailViewController.SetContent(null);
                    _videoDetailViewController.UpdateContent();
                    return;
                case DownloadState.Downloading:
                case DownloadState.Queued:
                    YouTubeDownloader.Instance.DequeueVideo(selectedLevelVideo);
                    return;
                case DownloadState.NotDownloaded:
                case DownloadState.Cancelled:
                    QueueDownload(selectedLevelVideo);
                    return;
            }
        }

        public void Present()
        {
            _freePlayFlowCoordinator.InvokePrivateMethod("PresentFlowCoordinator", new object[] { this, null, false, false });
        }

        private void DoSearch(string query)
        {
            Plugin.logger.Info("Do Search");
            _videoListViewController.SetLoadingState(true);
            _videoListViewController.SetContent(new List<YTResult>());
            Plugin.logger.Info("Doing Search");

            YouTubeSearcher.Search(query, selectedLevel, delegate () {
                Plugin.logger.Info("YT Search done");
                _videoListViewController.SetContent(YouTubeSearcher.searchResults);
                Plugin.logger.Info("YT Set Content");
                _videoListViewController.SetLoadingState(false);
                Plugin.logger.Info("Set Loading State");
            });
        }

        #region DetailView
        private void DetailViewBackPressed()
        {
            VideoLoader.SaveVideoToDisk(selectedLevelVideo);
            finished?.Invoke(selectedLevelVideo);
            _freePlayFlowCoordinator.InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, false });
            ScreenManager.Instance.PrepareVideo(selectedLevelVideo);
            ScreenManager.Instance.HideScreen();
        }

        private void DetailViewSearchPressed()
        {
            Plugin.logger.Info("DVSP");
            PresentViewController(_videoListViewController);
            Plugin.logger.Info("PVC Complete");

            DoSearch(selectedLevel.songName + " " + selectedLevel.songAuthorName);
            Plugin.logger.Info("Did Search");
            StopPreview();
            Plugin.logger.Info("Stopped Preview");
        }

        private void DetailViewLoopPressed()
        {
            Plugin.logger.Info("DVLP");
            selectedLevelVideo.loop = !selectedLevelVideo.loop;
            _videoDetailViewController.UpdateContent();
        }
        private void DetailViewPreviewPressed()
        {
            if (previewPlaying)
            {
                StopPreview();
            }
            else
            {
                // start preview
                ScreenManager.Instance.PlayVideo();
                songPreviewPlayer.CrossfadeTo(selectedLevel.GetPreviewAudioClipAsync(new CancellationToken()).Result, 0, selectedLevel.previewDuration, 1f);

                previewPlaying = true;
                
            }
            _videoDetailViewController.SetPreviewState(previewPlaying);
        }

        private void DetailViewAddOffsetPressed()
        {
            selectedLevelVideo.offset += 100;
            _videoDetailViewController.SetOffsetText(selectedLevelVideo.offset.ToString());
            StopPreview();
        }

        private void DetailViewSubOffsetPressed()
        {
            selectedLevelVideo.offset -= 100;
            _videoDetailViewController.SetOffsetText(selectedLevelVideo.offset.ToString());
            StopPreview();
        }

        private void StopPreview()
        {
            previewPlaying = false;
            ScreenManager.Instance.PrepareVideo(selectedLevelVideo);
            ScreenManager.Instance.PauseVideo();
            songPreviewPlayer.FadeOut();
            _videoDetailViewController.SetPreviewState(previewPlaying);
        }

        #endregion DetailView

        #region ListView
        private void ListViewDownloadPressed(YTResult result)
        {
            if (selectedLevelVideo != null)
            {
                // present
                _simpleDialog.Init("Overwrite video?", $"Do you really want to delete \"{ selectedLevelVideo.title }\"\n and replace it with \"{result.title }\"", "Overwrite", "Cancel", delegate (int button) { if (button == 0) { DismissViewController(_simpleDialog); QueueDownload(result); } });
                PresentViewController(_simpleDialog, null, false);
            }
            else
            {
                QueueDownload(result);
            }
        }

        private void QueueDownload(YTResult result)
        {
            // Delete existing
            Plugin.logger.Info("Queue Downloaded");
            if (selectedLevelVideo != null)
            {
                VideoLoader.Instance.RemoveVideo(selectedLevelVideo);

                switch (selectedLevelVideo.downloadState)
                {
                    case DownloadState.Downloaded:
                        VideoLoader.Instance.DeleteVideo(selectedLevelVideo);
                        break;
                    case DownloadState.Downloading:
                    case DownloadState.Queued:
                        selectedLevelVideo.downloadState = DownloadState.Cancelled; // stop download and dequeue
                        break;
                    default: // not downloaded, other
                        break;
                }
                selectedLevelVideo = null;
            }

            VideoData video = new VideoData(result);

            video.level = selectedLevel;
            selectedLevelVideo = video;
            VideoLoader.Instance.AddVideo(video);
            VideoLoader.SaveVideoToDisk(video);

            _videoDetailViewController.SetContent(video);
            _videoDetailViewController.UpdateContent();

            QueueDownload(video);

            DismissViewController(_videoListViewController);
        }

        private void QueueDownload(VideoData video)
        {
            YouTubeDownloader.Instance.EnqueueVideo(video);
            
            VideoDownloaderDownloadProgress(video);
        }

        private void ListViewBackPressed()
        {
            DismissViewController(_videoListViewController);
            SongPreviewPlayer preview = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
            preview.FadeOut();
        }

        private void ListViewSearchPressed()
        {
            Plugin.logger.Info("List View Search Pressed");
            if (_searchViewController == null)
            {
                Plugin.logger.Info("sVC null");
                _searchViewController = BeatSaberUI.CreateViewController<SearchKeyboardViewController>();
                Plugin.logger.Info("keyboard made");
                _searchViewController.backButtonPressed += SearchViewControllerBackButtonPressed;
                _searchViewController.searchButtonPressed += SearchViewControllerSearchButtonPressed;
            }
            _searchViewController.SetQuickButtons(selectedLevel);
            PresentViewController(_searchViewController);
            ScreenManager.Instance.HideScreen();
        }
        #endregion ListView
        
        #region SearchView
        private void SearchViewControllerSearchButtonPressed(string request)
        {
            DismissViewController(_searchViewController);
            DoSearch(request);
        }

        private void SearchViewControllerBackButtonPressed()
        {
            DismissViewController(_searchViewController);
        }
        #endregion SearchView
        
        public void VideoDownloaderDownloadProgress(VideoData video)
        {
            Plugin.logger.Info("VDDP");
            if (selectedLevelVideo == video)
            {
                _videoDetailViewController.UpdateContent();

                if (video.downloadState == DownloadState.Downloaded)
                {
                    ScreenManager.Instance.PrepareVideo(video);
                    ScreenManager.Instance.ShowScreen();
                }
            }
        }
        
        private void SongListTweaks_didFinishEvent(MainMenuViewController sender, MainMenuViewController.MenuButton result)
        {
            if (result == MainMenuViewController.MenuButton.SoloFreePlay)
            {
                _freePlayFlowCoordinator = FindObjectOfType<SoloFreePlayFlowCoordinator>();
            }
            else if (result == MainMenuViewController.MenuButton.Party)
            {
                _freePlayFlowCoordinator = FindObjectOfType<PartyFreePlayFlowCoordinator>();
            }
            else
            {
                _freePlayFlowCoordinator = null;
            }
        }

        public void HandleDidSelectLevel(LevelPackLevelsViewController sender, IPreviewBeatmapLevel level)
        {
            Plugin.logger.Info(level.levelID);
            selectedLevel = level;
            selectedLevelVideo = VideoLoader.Instance.GetVideo(level);
            ScreenManager.Instance.PrepareVideo(selectedLevelVideo);
        }
        
        protected override void DidDeactivate(DeactivationType type)
        {

        }
    }
}