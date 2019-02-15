using System;
using System.Collections.Generic;
using System.Linq;
using VRUI;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using HMUI;
using TMPro;
using UnityEngine;
using CustomUI.BeatSaber;
using CustomUI.Utilities;
using MusicVideoPlayer.Util;

namespace MusicVideoPlayer.UI.ViewControllers
{
    class VideoDetailViewController : VRUIViewController
    {
        public event Action listButtonPressed;
        public event Action downloadDeleteButtonPressed;
        public event Action previewButtonPressed;
        public event Action backButtonPressed;

        public event Action addOffsetPressed;
        public event Action subOffsetPressed;
        public event Action loopButtonPressed;

        private Button _listButton;
        private Button _downloadDeleteButton;
        private Button _previewButton;
        private Button _loopButton;
        private Button _backButton;

        private VideoData selectedVideo;

        private Image _thumbnail;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _duration;
        private TextMeshProUGUI _description;
        private TextMeshProUGUI _uploader;

        private Button _addOffset;
        private Button _subOffset;
        private TextMeshProUGUI _offsetText;
        private Image _progressRingGlow;
        private HoverHint _hoverHint;
        private Image _progressCircle;

        private Button _progressButton;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (type == ActivationType.AddedToHierarchy)
            {
                UpdateContent();
            }
        }

        public void Init()
        {
            // Buttons
            _backButton = BeatSaberUI.CreateBackButton(rectTransform as RectTransform, delegate () { backButtonPressed?.Invoke(); });

            _listButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, 30), new Vector2(30, 8), () =>
            {
                listButtonPressed?.Invoke();
            }, "Search");

            _downloadDeleteButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, 20), new Vector2(30, 8), () =>
            {
                downloadDeleteButtonPressed?.Invoke();
            }, "Delete");
            _downloadDeleteButton.GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);

            _previewButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, -30), new Vector2(30, 8), () =>
            {
                previewButtonPressed?.Invoke();
            }, "Preview");

            _loopButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, -20), new Vector2(30, 8), () =>
            {
                loopButtonPressed?.Invoke();
            }, "Loop");

            _addOffset = BeatSaberUI.CreateUIButton(rectTransform, "QuitButton", new Vector2(71, -10), new Vector2(8, 8), null, "+");
            foreach (StackLayoutGroup stack in _addOffset.GetComponentsInChildren<StackLayoutGroup>())
            {
                stack.childForceExpandHeight = false;
                stack.childForceExpandWidth = false;
                stack.padding = new RectOffset(0, 0, 0, 0);
            }

            _addOffset.GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);

            _addOffset.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 0, 0, 0);
            (_addOffset.transform.Find("Wrapper") as RectTransform).sizeDelta = new Vector2(8, 8);

            _subOffset = Instantiate(_addOffset, rectTransform);
            (_subOffset.transform as RectTransform).anchoredPosition = new Vector2(49, -10);
            _subOffset.SetButtonText("-");
            _subOffset.onClick.AddListener(() => { subOffsetPressed?.Invoke(); });
            _addOffset.onClick.AddListener(() => { addOffsetPressed?.Invoke(); });

            BeatSaberUI.AddHintText(_addOffset.transform as RectTransform, "Video lags behind music\nStart the video earlier");
            BeatSaberUI.AddHintText(_subOffset.transform as RectTransform, "Video is ahead of music\nStart the video later");

            _offsetText = BeatSaberUI.CreateText(rectTransform, "?", new Vector2(60, -10));
            _offsetText.rectTransform.sizeDelta = new Vector2(14, 8);
            _offsetText.alignment = TextAlignmentOptions.Center;
            _offsetText.color = Color.white;

            var _offsetTitle = BeatSaberUI.CreateText(rectTransform, "Video Offset (ms)", new Vector2(60, -3));
            _offsetTitle.rectTransform.sizeDelta = new Vector2(30, 8);
            _offsetTitle.alignment = TextAlignmentOptions.Center;

            // Video data
            _thumbnail = Instantiate(Resources.FindObjectsOfTypeAll<Image>().First(x => x.name == "CoverImage"), rectTransform);
            _thumbnail.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _thumbnail.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            var height = 30f;
            var width = 16f / 9f * height;
            _thumbnail.rectTransform.anchoredPosition = new Vector2(0 - width / 2, 20 - height / 2);
            _thumbnail.rectTransform.sizeDelta = new Vector2(width, height);
            _thumbnail.preserveAspect = false;
            _thumbnail.sprite = null;
            _thumbnail.transform.SetAsLastSibling();
            
            _title = BeatSaberUI.CreateText(rectTransform, "TITLE", new Vector2(0, -1));
            _title.alignment = TextAlignmentOptions.Top;
            _title.maxVisibleLines = 1;
            _title.fontSize = 6;
            _title.rectTransform.sizeDelta = new Vector2(100, 10);

            _uploader = BeatSaberUI.CreateText(rectTransform, "UPLOADER", new Vector2(0, -7));
            _uploader.alignment = TextAlignmentOptions.Left;
            _uploader.color = Color.white * 0.9f;
            _uploader.rectTransform.sizeDelta = new Vector2(80, 5);

            _duration = BeatSaberUI.CreateText(rectTransform, "[00:00]", new Vector2(0, -7));
            _duration.alignment = TextAlignmentOptions.Right;
            _duration.color = Color.white * 0.9f;
            _duration.rectTransform.sizeDelta = new Vector2(80, 5);

            _description = BeatSaberUI.CreateText(rectTransform,
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                new Vector2(0, -14));
            _description.alignment = TextAlignmentOptions.TopLeft;
            _description.rectTransform.sizeDelta = new Vector2(80, 10);
            _description.enableWordWrapping = true;
            _description.maxVisibleLines = 5;

            // Download Progress ring
            var _detailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First(x => x.name == "StandardLevelDetailViewController");
            var buttonsRect = _detailViewController.GetComponentsInChildren<RectTransform>().First(x => x.name == "Buttons");
            var _playbutton = buttonsRect.GetComponentsInChildren<Button>().First(x => x.name == "PlayButton");

            _progressButton = Instantiate(_playbutton, rectTransform);
            _progressButton.name = "DownloadProgress";
            _progressButton.SetButtonText("100%");
            (_progressButton.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (_progressButton.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (_progressButton.transform as RectTransform).anchoredPosition = new Vector2(0, 20);
            (_progressButton.transform as RectTransform).sizeDelta = new Vector2(18, 18);
            _progressButton.interactable = false;

            _progressRingGlow = _progressButton.GetComponentsInChildren<Image>().First(x => x.name == "Glow");
            _progressRingGlow.gameObject.SetActive(false);

            var hlg = _progressButton.GetComponentsInChildren<HorizontalLayoutGroup>().First(x => x.name == "Content");
            hlg.padding = new RectOffset(2, 2, 2, 2);

            _progressCircle = _progressButton.GetComponentsInChildren<Image>().First(x => x.name == "Stroke");
            _progressCircle.type = Image.Type.Filled;
            _progressCircle.fillMethod = Image.FillMethod.Radial360;
            _progressCircle.fillAmount = 1f;

            _progressButton.transform.SetParent(_thumbnail.transform);
            _hoverHint = BeatSaberUI.AddHintText(_thumbnail.transform as RectTransform, "Banana banana banana");
        }

        public void SetPreviewState(bool playing)
        {
            if (playing)
            {
                _previewButton.SetButtonText("Stop");
            }
            else
            {
                _previewButton.SetButtonText("Preview");
            }
        }

        public void SetOffsetText(string offset)
        {
            _offsetText.text = offset;
        }

        public void SetContent(VideoData video)
        {
            selectedVideo = video;
        }

        public void UpdateContent()
        {

            if (selectedVideo == null)
            {
                _title.SetText("<No Video Selected>");
                _description.SetText("Click Search to choose a video to download.");
                _uploader.SetText("");
                _duration.SetText("");

                _thumbnail.sprite = null;
                _thumbnail.color = Color.black;

                _progressCircle.color = Color.white;
                _progressCircle.fillAmount = 1;

                _progressButton.gameObject.SetActive(true);
                _progressButton.SetButtonText("N/A");
                _hoverHint.text = "No video selected\nDownload a video";

                _addOffset.interactable = false;
                _subOffset.interactable = false;
                _previewButton.interactable = false;
                _loopButton.interactable = false;
                _downloadDeleteButton.interactable = false;
                _downloadDeleteButton.SetButtonText("Download");
                return;
            }

            _title.SetText(selectedVideo.title);
            _description.SetText(selectedVideo.description);
            _uploader.SetText(selectedVideo.author);
            _duration.SetText($"[{selectedVideo.duration}]");
            SetOffsetText(selectedVideo.offset.ToString());
            _loopButton.SetButtonText(selectedVideo.loop ? "Loop" : "Once");
            StartCoroutine(LoadScripts.LoadSprite(selectedVideo.thumbnailURL, _thumbnail, 16f / 9f));

            if (selectedVideo.downloadState == DownloadState.NotDownloaded)
            {
                _progressButton.gameObject.SetActive(false);
                _progressButton.SetButtonText("Not Downloaded");
                _progressCircle.color = Color.red;
                _progressCircle.fillAmount = 1;

                _thumbnail.color = Color.white.ColorWithAlpha(0.2f);
                _hoverHint.text = "Video selected but not downloaded";

                _addOffset.interactable = false;
                _subOffset.interactable = false;
                _previewButton.interactable = false;
                _loopButton.interactable = false;
                _downloadDeleteButton.interactable = true;
                _downloadDeleteButton.SetButtonText("Download");
            }

            else if (selectedVideo.downloadState == DownloadState.Cancelled)
            {
                _progressButton.gameObject.SetActive(false);
                _progressButton.SetButtonText("Cancelled");
                _progressCircle.color = Color.red;
                _progressCircle.fillAmount = 1;

                _thumbnail.color = Color.white.ColorWithAlpha(0.2f);
                _hoverHint.text = "Download cancelled or encountered an error";

                _addOffset.interactable = false;
                _subOffset.interactable = false;
                _previewButton.interactable = false;
                _loopButton.interactable = false;
                _downloadDeleteButton.interactable = true;
                _downloadDeleteButton.SetButtonText("Download");
            }

            else if (selectedVideo.downloadState == DownloadState.Downloading)
            {
                _progressButton.gameObject.SetActive(true);

                _hoverHint.text = String.Format("Downloading: {0:#.0}% complete", selectedVideo.downloadProgress * 100);
                _progressButton.SetButtonText(String.Format("{0:#.0}%", selectedVideo.downloadProgress * 100));
                _progressCircle.color = Color.white;
                _progressCircle.fillAmount = selectedVideo.downloadProgress;
                _thumbnail.color = Color.Lerp(Color.white.ColorWithAlpha(0.2f), Color.white, selectedVideo.downloadProgress);

                _hoverHint.text = "Download in progress";

                _addOffset.interactable = false;
                _subOffset.interactable = false;
                _previewButton.interactable = false;
                _loopButton.interactable = false;
                _downloadDeleteButton.interactable = true;
                _downloadDeleteButton.SetButtonText("Cancel");
            }

            else if (selectedVideo.downloadState == DownloadState.Downloaded)
            {
                _progressButton.gameObject.SetActive(false);

                _thumbnail.color = Color.white;
                _hoverHint.text = "Video Ready, Search again to overwrite";

                _addOffset.interactable = true;
                _subOffset.interactable = true;
                _previewButton.interactable = true;
                _loopButton.interactable = true;
                _downloadDeleteButton.interactable = true;
                _downloadDeleteButton.SetButtonText("Delete");
            }

            else if (selectedVideo.downloadState == DownloadState.Queued)
            {
                _progressButton.gameObject.SetActive(true);
                _progressButton.SetButtonText("Pending");
                _progressCircle.color = Color.cyan;
                _progressCircle.fillAmount = 1;

                _thumbnail.color = Color.white.ColorWithAlpha(0.2f);
                _hoverHint.text = "Download queued and will begin shortly";

                _addOffset.interactable = false;
                _subOffset.interactable = false;
                _previewButton.interactable = false;
                _loopButton.interactable = false;
                _downloadDeleteButton.interactable = true;
                _downloadDeleteButton.SetButtonText("Cancel");
            }
        }
    }
}