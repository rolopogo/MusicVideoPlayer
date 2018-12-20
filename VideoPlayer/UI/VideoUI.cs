using CustomUI.BeatSaber;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRUI;
using SongLoaderPlugin;
using SongLoaderPlugin.OverrideClasses;
using System.IO;
//using HMUI;
using UnityEngine.UI;
using MusicVideoPlayer.UI.ViewControllers;
using MusicVideoPlayer.Misc;

namespace MusicVideoPlayer.UI
{
    class VideoUI : MonoBehaviour
    {

        public bool initialized = false;
        
        private static VideoUI _instance = null;
        public static VideoUI Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = new GameObject("SongListTweaks").AddComponent<VideoUI>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        IBeatmapLevel selectedLevel;
        Video downloadingVideo;

        private MainFlowCoordinator _mainFlowCoordinator;
        private FlowCoordinator _freePlayFlowCoordinator;
        private LevelListViewController _levelListViewController;
        private BeatmapDifficultyViewController _difficultyViewController;
        private StandardLevelDetailViewController _detailViewController;
        private SearchKeyboardViewController _searchViewController;
        private VideoListViewController _videosViewController;
        
        private Button _videoButton;
        private GameObject _videoButtonGlow;
        private HoverHint _videoButtonHint;

        private Image progressCircle;

        public void OnLoad()
        {
            initialized = false;
            SetupTweaks();
        }

        private void SetupTweaks()
        {
            YoutubeDownloader.Instance.downloadProgress += VideoDownloaderDownloadProgress;
            YoutubeDownloader.Instance.videoDownloaded += VideoDownloaderVideoDownloaded;

            _mainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault();
            _mainFlowCoordinator.GetPrivateField<MainMenuViewController>("_mainMenuViewController").didFinishEvent += SongListTweaks_didFinishEvent;

            _difficultyViewController = Resources.FindObjectsOfTypeAll<BeatmapDifficultyViewController>().FirstOrDefault();
            _difficultyViewController.didSelectDifficultyEvent += _difficultyViewController_didSelectDifficultyEvent;

            _levelListViewController = Resources.FindObjectsOfTypeAll<LevelListViewController>().FirstOrDefault();
            
            _detailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First(x => x.name == "StandardLevelDetailViewController");
            
            RectTransform buttonsRect = _detailViewController.GetComponentsInChildren<RectTransform>().First(x => x.name == "Buttons");
            var _playbutton = buttonsRect.GetComponentsInChildren<Button>().First(x => x.name == "PlayButton");
            var _practiceButton = buttonsRect.GetComponentsInChildren<Button>().First(x => x.name == "PracticeButton");
            
            _videoButton = Instantiate(_practiceButton, buttonsRect.parent);
            _videoButton.name = "VideoButton";
            _videoButton.SetButtonIcon(Base64Sprites.PlayIcon);
            (_videoButton.transform as RectTransform).anchoredPosition = new Vector2(46, -6);
            (_videoButton.transform as RectTransform).sizeDelta = new Vector2(8, 8);
            _videoButton.onClick.AddListener(VideoPressed);
            _videoButtonHint = BeatSaberUI.AddHintText(_videoButton.transform as RectTransform, "Download a video");

            var glow = _playbutton.GetComponentsInChildren<RectTransform>().First(x => x.name == "GlowContainer");
            var videoWrapper = _videoButton.GetComponentsInChildren<RectTransform>().First(x => x.name == "Wrapper");
            _videoButtonGlow = Instantiate(glow.gameObject, videoWrapper).gameObject;
            _videoButtonGlow.name = "GlowContainer";

            var hlg = _videoButton.GetComponentsInChildren<HorizontalLayoutGroup>().First(x => x.name == "Content");
            hlg.padding = new RectOffset(3, 2, 2, 2);

            progressCircle = videoWrapper.GetComponentsInChildren<Image>().First(x => x.name == "Stroke");
            progressCircle.type = Image.Type.Filled;
            progressCircle.fillMethod = Image.FillMethod.Radial360;
            progressCircle.fillAmount = 1f;
            
            initialized = true;
        }

        private void VideoDownloaderVideoDownloaded(Video video)
        {
            if(video == downloadingVideo)
            {
                downloadingVideo = null;
                UpdateVideoButton(selectedLevel);
            }
        }

        private void VideoDownloaderDownloadProgress(Video video)
        {
            if (downloadingVideo == video)
            {
                _videoButtonHint.text = "Downloading: " + video.downLoadProgress * 100 + "%";
                _videoButtonGlow.SetActive(false);
                progressCircle.color = Color.Lerp(Color.red, Color.green, video.downLoadProgress);
                progressCircle.fillAmount = video.downLoadProgress;
            }
        }
        
        private void _difficultyViewController_didSelectDifficultyEvent(BeatmapDifficultyViewController sender, IDifficultyBeatmap beatmap)
        {
            selectedLevel = beatmap.level;
            // see if this is being downloaded or in queue
            downloadingVideo = YoutubeDownloader.Instance.GetDownloadingVideo(selectedLevel);
            
            if(downloadingVideo?.level == selectedLevel)
            {
                VideoDownloaderDownloadProgress(downloadingVideo);
            }
            else
            {
                UpdateVideoButton(selectedLevel);
            }
        }

        private void UpdateVideoButton(IBeatmapLevel level)
        {
            if (level.levelID.Length >= 32)
            {
                _videoButton.interactable = true;
                if (VideoFetcher.SongHasVideo(level))
                {
                    _videoButtonGlow.SetActive(false);
                    _videoButtonHint.text = "<color=#c0c0c0><size=80%>Replace existing video";
                    progressCircle.fillAmount = 1f;
                    progressCircle.color = new Color(0.75f, 0.75f, 0.75f);
                }
                else
                {
                    // video file exists
                    _videoButtonGlow.SetActive(true);
                    _videoButtonHint.text = "Add a Video";
                    progressCircle.fillAmount = 1f;
                    progressCircle.color = Color.white;
                }
            }
            else
            {
                _videoButton.interactable = false;
                _videoButtonHint.text = "<color=#808080><size=80%>Videos not available for OST songs\nsorry :(";
                progressCircle.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private void VideoPressed()
        {
            if (_videosViewController == null)
            {
                _videosViewController = BeatSaberUI.CreateViewController<VideoListViewController>();
                _videosViewController.backButtonPressed += BackPressed;
                _videosViewController.downloadButtonPressed += DownloadPressed;
                _videosViewController.searchButtonPressed += SearchPressed;
            }
            _freePlayFlowCoordinator.InvokePrivateMethod("PresentViewController", new object[] { _videosViewController, null, false });

            // Auto Search
            string query = string.Format("{0} {1}", selectedLevel.songName, selectedLevel.songAuthorName);
            DoSearch(query);
        }

        private void DoSearch(string query)
        {
            _videosViewController.SetLoadingState(true);
            _videosViewController.SetContent(new List<Video>());

            YouTubeSearcher.Search(query, selectedLevel, delegate () {
                _videosViewController.SetContent(YouTubeSearcher.searchResults);
                _videosViewController.SetLoadingState(false);
            });
        }

        private void DownloadPressed(Video video)
        {
            // download
            video.level = selectedLevel;
            Console.WriteLine("Downloading: \n" + video.ToString());
            YoutubeDownloader.Instance.EnqueueVideo(video);
            downloadingVideo = video;
            VideoDownloaderDownloadProgress(video);
            
            _freePlayFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _videosViewController, null, false });
        }

        private void BackPressed()
        {
            _freePlayFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _videosViewController, null, false });
        }

        private void SearchPressed()
        {
            if (_searchViewController == null)
            {
                _searchViewController = BeatSaberUI.CreateViewController<SearchKeyboardViewController>();
                _searchViewController.backButtonPressed += SearchViewControllerBackButtonPressed;
                _searchViewController.searchButtonPressed += SearchViewControllerSearchButtonPressed;
            }
            _searchViewController.SetQuickButtons(selectedLevel);
            _freePlayFlowCoordinator.InvokePrivateMethod("PresentViewController", new object[] { _searchViewController, null, false });

        }

        private void SearchViewControllerSearchButtonPressed(string request)
        {
            _freePlayFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _searchViewController, null, false });
            DoSearch(request);
        }

        private void SearchViewControllerBackButtonPressed()
        {
            _freePlayFlowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _searchViewController, null, false });
        }

        private string GetSelectedSongPath()
        {
            return SongLoader.CustomLevels.Find(x => x.customSongInfo.GetIdentifier() == selectedLevel.levelID)?.customSongInfo?.path;
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
    }
}
