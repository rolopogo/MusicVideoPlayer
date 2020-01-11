using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.YT;
using BS_Utils.Utilities;
using MusicVideoPlayer.UI.UIElements;
using BeatSaberMarkupLanguage;
using HMUI;
using Image = UnityEngine.UI.Image;

namespace MusicVideoPlayer.UI
{
    class VideoUI : MonoBehaviour
    {
        private static VideoUI _instance = null;
        public static VideoUI Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = new GameObject("VideoPlayerMenuTweaks").AddComponent<VideoUI>();
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

        private VideoFlowCoordinator _videoFlowCoordinator;

        private Button _videoButton;
        private Image _videoButtonGlow;
        private HoverHint _videoButtonHint;

        private Image _progressCircle;

        public void OnLoad()
        {
            SetupTweaks();
        }

        private void SetupTweaks()
        {
            YouTubeDownloader.Instance.downloadProgress += VideoDownloaderDownloadProgress;

            _videoFlowCoordinator = gameObject.AddComponent<VideoFlowCoordinator>();

            _videoFlowCoordinator.finished += VideoFlowCoordinatorFinished;
            _videoFlowCoordinator.Init();
            
            BSEvents.levelSelected += HandleDidSelectLevel;

            var _levelDetailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();

            var _buttons = _levelDetailViewController.transform.Find("LevelDetail/PlayContainer/PlayButtons");
            var _playbutton = _buttons.GetComponentsInChildren<Button>().First(x => x.name == "PlayButton");
            var _practiceButton = _buttons.GetComponentsInChildren<Button>().First(x => x.name == "PracticeButton");

            var _coverImage = _levelDetailViewController.transform.Find("LevelDetail/Level/CoverImage");

            _videoButton = Instantiate(_practiceButton, _coverImage);
            _videoButton.name = "VideoButton";
            BeatSaberUI.SetButtonIcon(_videoButton, Base64Sprites.PlayIcon);
            (_videoButton.transform as RectTransform).anchoredPosition = new Vector2(0,0); 
            (_videoButton.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (_videoButton.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (_videoButton.transform as RectTransform).sizeDelta = new Vector2(8, 8);
            _videoButton.onClick.AddListener(delegate () { _videoFlowCoordinator.Present(); });

            _videoButtonHint = CustomUIElements.AddHintText(_videoButton.transform as RectTransform, "Download a video");

            var glow = _playbutton.GetComponentsInChildren<RectTransform>().First(x => x.name == "GlowContainer");
            var videoWrapper = _videoButton.GetComponentsInChildren<RectTransform>().First(x => x.name == "Wrapper");
            _videoButtonGlow = Instantiate(glow.gameObject, videoWrapper).gameObject.GetComponentInChildren<Image>();

            var hlg = _videoButton.GetComponentsInChildren<HorizontalLayoutGroup>().First(x => x.name == "Content");
            hlg.padding = new RectOffset(3, 2, 2, 2);

            _progressCircle = videoWrapper.GetComponentsInChildren<Image>().First(x => x.name == "Stroke");
            _progressCircle.type = Image.Type.Filled;
            _progressCircle.fillMethod = Image.FillMethod.Radial360;
            _progressCircle.fillAmount = 1f;
        }

        private void VideoFlowCoordinatorFinished(VideoData video)
        {
            UpdateVideoButton(video);
        }

        private void VideoDownloaderVideoDownloaded(VideoData video)
        {
            if (selectedLevel == video.level)
            {
                UpdateVideoButton(video);
            }
        }

        private void VideoDownloaderDownloadProgress(VideoData video)
        {
            if (selectedLevel == video.level)
            {
                UpdateVideoButton(video);
            }
        }

        public void HandleDidSelectLevel(LevelCollectionViewController sender, IPreviewBeatmapLevel level)
        {
            selectedLevel = Resources.FindObjectsOfTypeAll<BeatmapLevelSO>().First(x => x.levelID == level.levelID);
            UpdateVideoButton(VideoLoader.Instance.GetVideo(selectedLevel));
        }
        
        private void UpdateVideoButton(VideoData selectedVideo)
        {
            selectedVideo = VideoLoader.Instance.GetVideo(selectedLevel);

            if (selectedVideo != null)
            {
                if (selectedVideo.downloadState == DownloadState.Queued)
                {
                    // video queued
                    _videoButtonHint.text = "Queued for download";
                    _videoButtonGlow.gameObject.SetActive(true);
                    _videoButtonGlow.color = Color.cyan;
                    _progressCircle.color = Color.cyan;
                    _progressCircle.fillAmount = 1;
                }
                else if (selectedVideo.downloadState == DownloadState.Downloading)
                {
                    // video downloading
                    _videoButtonHint.text = String.Format("Downloading: {0:#.0}%", selectedVideo.downloadProgress * 100);
                    _videoButtonGlow.gameObject.SetActive(false);
                    _progressCircle.color = Color.Lerp(Color.red, Color.green, selectedVideo.downloadProgress);
                    _progressCircle.fillAmount = selectedVideo.downloadProgress;
                }
                else if (selectedVideo.downloadState == DownloadState.Downloaded)
                {
                    // video ready
                    _videoButtonGlow.gameObject.SetActive(true);
                    _videoButtonGlow.color = Color.green;
                    _videoButtonHint.text = "<color=#c0c0c0><size=80%>Replace existing video";
                    _progressCircle.fillAmount = 1f;
                    _progressCircle.color = new Color(0.75f, 0.75f, 0.75f);
                }
                else
                {
                    // notdownloaded or cancelled
                    _videoButtonGlow.gameObject.SetActive(true);
                    _videoButtonHint.text = "<color=#808080><size=80%>Video selected but not downloaded";
                    _videoButtonGlow.color = Color.yellow;
                    _progressCircle.fillAmount = 1f;
                }
            }
            else
            {
                // no video
                _videoButtonGlow.gameObject.SetActive(true);
                _videoButtonGlow.color = Color.red;
                _videoButtonHint.text = "Add a Video";
                _progressCircle.fillAmount = 1f;
                _progressCircle.color = Color.white;
            }
        }
    }
}
