using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using MusicVideoPlayer.UI.ViewControllers;
using System.Collections.Generic;
using System.Linq;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.YT;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage;

namespace MusicVideoPlayer.UI
{
    class VideoFlowCoordinator : FlowCoordinator
    {
        public event Action<VideoData> finished;

        private MainFlowCoordinator _mainFlowCoordinator;
        private FlowCoordinator _freePlayFlowCoordinator;
        private SongPreviewPlayer songPreviewPlayer;

        //private SearchKeyboardViewController _searchViewController;
        private VideoListViewController _videoListViewController;
        private VideoDetailViewController _videoDetailViewController;
        private SimpleDialogPromptViewController _simpleDialog;

        private IBeatmapLevel selectedLevel;
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
            
            if (_videoDetailViewController == null)
            {

                _videoDetailViewController = BeatSaberUI.CreateViewController<VideoDetailViewController>();
                _videoDetailViewController.Init();
                _videoDetailViewController.backButtonPressed += DetailViewBackPressed;
                _videoDetailViewController.addOffsetPressed += DetailViewAddOffsetPressed;
                _videoDetailViewController.subOffsetPressed += DetailViewSubOffsetPressed;
                _videoDetailViewController.previewButtonPressed += DetailViewPreviewPressed;
                _videoDetailViewController.loopButtonPressed += DetailViewLoopPressed;
                _videoDetailViewController.listButtonPressed += DetailViewSearchPressed;
                _videoDetailViewController.downloadDeleteButtonPressed += DetailViewDownloadDeletePressed;
            }
            if (_videoListViewController == null)
            {
                _videoListViewController = BeatSaberUI.CreateViewController<VideoListViewController>();
                //_videoListViewController.backButtonPressed += ListViewBackPressed;
                _videoListViewController.downloadButtonPressed += ListViewDownloadPressed;
                _videoListViewController.searchButtonPressed += ListViewSearchPressed;
            }
            if (_simpleDialog == null)
            {
                _simpleDialog = Resources.FindObjectsOfTypeAll<SimpleDialogPromptViewController>().First();
                _simpleDialog = Instantiate(_simpleDialog.gameObject, _simpleDialog.transform.parent).GetComponent<SimpleDialogPromptViewController>();
            }
            if (activationType == FlowCoordinator.ActivationType.AddedToHierarchy)
            {
                _videoDetailViewController.SetContent(selectedLevelVideo);
                previewPlaying = false;
                _videoDetailViewController.SetPreviewState(previewPlaying);
                if (selectedLevelVideo != null)
                {
                    ScreenManager.Instance.ShowScreen();
                }
                ProvideInitialViewControllers(_videoDetailViewController, null, null);
            }
        }

        private void DetailViewDownloadDeletePressed()
        {
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
            _freePlayFlowCoordinator.InvokeMethod("PresentFlowCoordinator", new object[] { this, null, false, false });
        }

        private void DoSearch(string query)
        {
            _videoListViewController.SetLoadingState(true);
            _videoListViewController.SetContent(new List<YTResult>());

            YouTubeSearcher.Search(query, selectedLevel, delegate () {
                _videoListViewController.SetContent(YouTubeSearcher.searchResults);
                _videoListViewController.SetLoadingState(false);
            });
        }

        #region DetailView
        private void DetailViewBackPressed()
        {
            VideoLoader.SaveVideoToDisk(selectedLevelVideo);
            finished?.Invoke(selectedLevelVideo);
            _freePlayFlowCoordinator.InvokeMethod("DismissFlowCoordinator", new object[] { this, null, false });
            ScreenManager.Instance.PrepareVideo(selectedLevelVideo);
            ScreenManager.Instance.HideScreen();
        }

        private void DetailViewSearchPressed()
        {
            PresentViewController(_videoListViewController);

            DoSearch(selectedLevel.songName + " " + selectedLevel.songAuthorName);
            StopPreview();
        }

        private void DetailViewLoopPressed()
        {
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
                songPreviewPlayer.CrossfadeTo(selectedLevel.beatmapLevelData.audioClip, 0, selectedLevel.beatmapLevelData.audioClip.length, 1f);

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
            //if (_searchViewController == null)
            //{
            //    //_searchViewController = BeatSaberUI.CreateViewController<SearchKeyboardViewController>();
            //    _searchViewController.backButtonPressed += SearchViewControllerBackButtonPressed;
            //    _searchViewController.searchButtonPressed += SearchViewControllerSearchButtonPressed;
            //}
            //_searchViewController.SetQuickButtons(selectedLevel);
            //PresentViewController(_searchViewController);
            ScreenManager.Instance.HideScreen();
        }
        #endregion ListView
        
        #region SearchView
        private void SearchViewControllerSearchButtonPressed(string request)
        {
            //DismissViewController(_searchViewController);
            DoSearch(request);
        }

        private void SearchViewControllerBackButtonPressed()
        {
            //DismissViewController(_searchViewController);
        }
        #endregion SearchView
        
        public void VideoDownloaderDownloadProgress(VideoData video)
        {
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

        public void HandleDidSelectLevel(LevelCollectionViewController sender, IPreviewBeatmapLevel level)
        {
            selectedLevel = Resources.FindObjectsOfTypeAll<BeatmapLevelSO>().First(x=>x.levelID == level.levelID);
            
            selectedLevelVideo = VideoLoader.Instance.GetVideo(selectedLevel);
            ScreenManager.Instance.PrepareVideo(selectedLevelVideo);
        }
        
        protected override void DidDeactivate(DeactivationType type)
        {

        }
    }
}