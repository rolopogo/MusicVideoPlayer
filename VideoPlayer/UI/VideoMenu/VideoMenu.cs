using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Parser;
using BS_Utils.Utilities;
using HMUI;
using MusicVideoPlayer.UI;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.YT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace MusicVideoPlayer
{
    public class VideoMenu : PersistentSingleton<VideoMenu>
    {
        #region Fields
        [UIObject("root-object")]
        private GameObject root;

        [UIObject("current-video-player")]
        private GameObject currentVideoPlayer;

        #region Rect Transform
        [UIComponent("video-details")]
        private RectTransform videoDetailsViewRect;

        [UIComponent("video-search-results")]
        private RectTransform videoSearchResultsViewRect;
        #endregion

        #region Text Mesh Pro
        [UIComponent("current-video-title")]
        private TextMeshProUGUI currentVideoTitleText;

        [UIComponent("current-video-description")]
        private TextMeshProUGUI currentVideoDescriptionText;

        [UIComponent("current-video-offset")]
        private TextMeshProUGUI currentVideoOffsetText;

        [UIComponent("preview-button")]
        private TextMeshProUGUI previewButtonText;

        [UIComponent("search-results-loading")]
        private TextMeshProUGUI searchResultsLoadingText;

        [UIComponent("looping-button")]
        private TextMeshProUGUI loopingButtonText;

        [UIComponent("download-state-text")]
        private TextMeshProUGUI downloadStateText;

        [UIComponent("offset-magnitude-button")]
        private TextMeshProUGUI offsetMagnitudeButtonText;
        #endregion

        #region Buttons
        [UIComponent("video-list")]
        private CustomListTableData customListTableData;

        [UIComponent("offset-decrease-button")]
        private Button offsetDecreaseButton;

        [UIComponent("offset-increase-button")]
        private Button offsetIncreaseButton;

        [UIComponent("delete-button")]
        private Button deleteButton;

        [UIComponent("download-button")]
        private Button downloadButton;

        [UIComponent("refine-button")]
        private Button refineButton;

        [UIComponent("preview-button")]
        private Button previewButton;

        [UIComponent("looping-button")]
        private Button loopingButton;
        #endregion

        [UIComponent("search-keyboard")]
        private ModalKeyboard searchKeyboard;

        [UIParams]
        private BSMLParserParams parserParams;

        private IPreviewBeatmapLevel selectedLevel;

        private VideoData selectedVideo;

        private SongPreviewPlayer songPreviewPlayer;

        private VideoMenuStatus statusViewer;

        private bool isPreviewing = false;

        private bool isOffsetInSeconds = false;

        private bool isActive = false;

        private bool isPlayingLevel = false;

        private IEnumerator updateSearchResultsCoroutine = null;

        private int selectedCell;
        #endregion

        public void OnLoad()
        {
            Setup();
        }

        internal void Setup()
        {
            YouTubeDownloader.Instance.downloadProgress += VideoDownloaderDownloadProgress;
            BSEvents.levelSelected += HandleDidSelectLevel;
            BSEvents.gameSceneLoaded += GameSceneLoaded;
            songPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();

            videoDetailsViewRect.gameObject.SetActive(true);
            videoSearchResultsViewRect.gameObject.SetActive(false);

            statusViewer = root.AddComponent<VideoMenuStatus>();
            statusViewer.DidEnable += StatusViewerDidEnable;
            statusViewer.DidDisable += StatusViewerDidDisable;
        }

        private void StatusViewerDidEnable(object sender, EventArgs e)
        {
            Plugin.logger.Debug("Activated");
            Activate();
        }

        private void StatusViewerDidDisable(object sender, EventArgs e)
        {
            Plugin.logger.Debug("Deactivated");
            Deactivate();
        }

        #region Public Methods
        public void LoadVideoSettings(VideoData videoData)
        {
            StopPreview();

            if (videoData == null)
            {
                videoData = VideoLoader.Instance.GetVideo(selectedLevel);
            }

            selectedVideo = videoData;

            if (videoData != null)
            {
                currentVideoTitleText.text = $"[{videoData.duration}] {videoData.title} by {videoData.author}";
                currentVideoDescriptionText.text = videoData.description;
                currentVideoOffsetText.text = videoData.offset.ToString();
                EnableButtons(true);
                UpdateLooping();
            }
            else
            {
                currentVideoTitleText.text = "NO VIDEO SET";
                currentVideoDescriptionText.text = "";
                currentVideoOffsetText.text = "N/A";
                EnableButtons(false);
            }

            LoadVideoDownloadState();

            Plugin.logger.Debug("Has Loaded: " + videoData);
            ScreenManager.Instance.PrepareVideo(videoData);
        }

        public void Activate()
        {
            isActive = true;
            isPlayingLevel = false;
            ScreenManager.Instance.ShowScreen();
            ChangeView(false);
        }

        public void Deactivate()
        {
            StopPreview();

            isActive = false;
            selectedVideo = null;

            currentVideoTitleText.text = "NO VIDEO SET";
            currentVideoDescriptionText.text = "";
            currentVideoOffsetText.text = "N/A";
            EnableButtons(false);

            ScreenManager.Instance.SetPlacement(MVPSettings.instance.PlacementMode);

            if(!isPlayingLevel)
            {
                selectedLevel = null;
            }
        }
        #endregion

        #region Private Methods
        private void EnableButtons(bool enable)
        {
            offsetDecreaseButton.interactable = enable;
            offsetIncreaseButton.interactable = enable;
            loopingButton.interactable = enable;

            if (selectedVideo == null || selectedVideo.downloadState != DownloadState.Downloaded)
            {
                enable = false;
            }

            previewButton.interactable = enable;
            deleteButton.interactable = enable;
        }

        private void SetPreviewState()
        {
            if (isPreviewing)
            {
                previewButtonText.text = "Stop";
            }
            else
            {
                previewButtonText.text = "Preview";
            }
        }

        private void StopPreview()
        {
            isPreviewing = false;
            ScreenManager.Instance.PrepareVideo(selectedVideo);
            songPreviewPlayer.FadeOut();
            SetPreviewState();
        }

        private void ChangeView(bool searchView)
        {
            StopPreview();
            ResetSearchView();
            videoDetailsViewRect.gameObject.SetActive(!searchView);
            videoSearchResultsViewRect.gameObject.SetActive(searchView);

            if (!searchView)
            {
                parserParams.EmitEvent("hide-keyboard");

                if(isActive)
                {
                    ScreenManager.Instance.SetScale(new Vector3(0.57f, 0.57f, 1f));

                    var position = videoDetailsViewRect.transform.position;
                    position.y += 0.25f;
                    position.z -= 0.25f;
                    position.x -= 0.10f;

                    ScreenManager.Instance.SetPosition(position);
                    ScreenManager.Instance.SetRotation(videoDetailsViewRect.transform.eulerAngles);
                }

                LoadVideoSettings(selectedVideo);
            }
            else
            {
                ScreenManager.Instance.SetPlacement(MVPSettings.instance.PlacementMode);
            }
        }

        private void ResetSearchView()
        {
            if (updateSearchResultsCoroutine != null)
            {
                StopCoroutine(updateSearchResultsCoroutine);
            }

            StopCoroutine(SearchLoading());

            if (customListTableData.data != null || customListTableData.data.Count > 0)
            {
                customListTableData.data.Clear();
                customListTableData.tableView.ReloadData();
            }

            selectedCell = -1;
        }

        private void UpdateLooping()
        {
            if (selectedVideo != null)
            {
                if (selectedVideo.loop)
                {
                    loopingButtonText.text = "Loop";
                }
                else
                {
                    loopingButtonText.text = "Once";
                }
            }
        }

        private void UpdateOffset(bool isDecreasing)
        {
            if (selectedVideo != null)
            {
                int magnitude = isOffsetInSeconds ? 1000 : 100;
                magnitude = isDecreasing ? magnitude * -1 : magnitude;

                selectedVideo.offset += magnitude;
                currentVideoOffsetText.text = selectedVideo.offset.ToString();
                Save();
            }
        }

        private void Save()
        {
            if (selectedVideo != null)
            {
                StopPreview();
                VideoLoader.SaveVideoToDisk(selectedVideo);
            }
        }

        private void LoadVideoDownloadState()
        {
            string state = "No Video";

            if (selectedVideo != null)
            {
                switch (selectedVideo.downloadState)
                {
                    case DownloadState.NotDownloaded:
                        state = "No Video";
                        break;
                    case DownloadState.Queued:
                        state = "Queued";
                        break;
                    case DownloadState.Downloading:
                        state = $"Downloading {selectedVideo.downloadProgress * 100}%";
                        break;
                    case DownloadState.Downloaded:
                        state = "Downloaded";
                        break;
                    case DownloadState.Cancelled:
                        state = "Cancelled";
                        break;
                }
            }

            downloadStateText.text = "Download Progress: " + state;
        }

        private IEnumerator UpdateSearchResults(List<YTResult> results)
        {
            List<CustomListTableData.CustomCellInfo> videos = new List<CustomListTableData.CustomCellInfo>();

            foreach (var result in results)
            {
                string description = $"[{result.duration}] {result.description}";
                var item = new CustomListTableData.CustomCellInfo(result.title, description);

                UnityWebRequest request = UnityWebRequestTexture.GetTexture(result.thumbnailURL);
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                    Debug.Log(request.error);
                else
                    item.icon = ((DownloadHandlerTexture)request.downloadHandler).texture;

                videos.Add(item);
            }

            customListTableData.data = videos;
            customListTableData.tableView.ReloadData();

            refineButton.interactable = true;
            searchResultsLoadingText.gameObject.SetActive(false);
        }

        private IEnumerator SearchLoading()
        {
            int count = 0;
            string loadingText = "Loading Results";
            searchResultsLoadingText.gameObject.SetActive(true);

            while (searchResultsLoadingText.gameObject.activeInHierarchy)
            {
                string periods = string.Empty;
                count++;

                for (int i = 0; i < count; i++)
                {
                    periods += ".";
                }

                if (count == 3)
                {
                    count = 0;
                }

                searchResultsLoadingText.SetText(loadingText + periods);

                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        #region Actions
        [UIAction("on-looping-action")]
        private void OnLoopingAction()
        {
            selectedVideo.loop = !selectedVideo.loop;
            UpdateLooping();
            Save();
        }

        [UIAction("on-offset-magnitude-action")]
        private void OnOffsetMagnitudeAction()
        {
            isOffsetInSeconds = !isOffsetInSeconds;

            if(isOffsetInSeconds)
            {
                offsetMagnitudeButtonText.text = "+1000";
            }
            else
            {
                offsetMagnitudeButtonText.text = "+100";
            }
        }

        [UIAction("on-offset-decrease-action")]
        private void OnOffsetDecreaseAction()
        {
            UpdateOffset(true);
        }

        [UIAction("on-offset-increase-action")]
        private void OnOffsetIncreaseAction()
        {
            UpdateOffset(false);
        }

        [UIAction("on-delete-action")]
        private void OnDeleteAction()
        {
            if(selectedVideo != null)
            {
                VideoLoader.Instance.DeleteVideo(selectedVideo);
                LoadVideoSettings(null);
            }
        }

        [UIAction("on-preview-action")]
        private void OnPreviewAction()
        {
            if (isPreviewing)
            {
                StopPreview();
            }
            else
            {
                isPreviewing = true;
                ScreenManager.Instance.PlayVideo();
                songPreviewPlayer.CrossfadeTo(selectedLevel.GetPreviewAudioClipAsync(new CancellationToken()).Result, 0, selectedLevel.previewDuration, 1f);
            }

            SetPreviewState();
        }

        [UIAction("on-search-action")]
        private void OnSearchAction()
        {
            ChangeView(true);
            searchKeyboard.SetText(selectedLevel.songName + " - " + selectedLevel.songSubName);
            parserParams.EmitEvent("show-keyboard");
        }

        [UIAction("on-back-action")]
        private void OnBackAction()
        {
            ChangeView(false);
        }

        [UIAction("on-select-cell")]
        private void OnSelectCell(TableView view, int idx)
        {
            if(customListTableData.data.Count > idx)
            {
                selectedCell = idx;
                downloadButton.interactable = true;
                Plugin.logger.Debug($"Selected Cell: {YouTubeSearcher.searchResults[idx].ToString()}");
            }
            else
            {
                downloadButton.interactable = false;
                selectedCell = -1;
            }
        }

        [UIAction("on-download-action")]
        private void OnDownloadAction()
        {
            if(selectedCell >= 0)
            {
                VideoData data = new VideoData(YouTubeSearcher.searchResults[selectedCell], selectedLevel);
                YouTubeDownloader.Instance.EnqueueVideo(data);
                VideoLoader.Instance.AddVideo(data);
            }
        }

        [UIAction("on-refine-action")]
        private void OnRefineAction()
        {
            OnSearchAction();
        }

        [UIAction("on-query")]
        private void OnQueryAction(string query)
        {
            ResetSearchView();
            downloadButton.interactable = false;
            refineButton.interactable = false;
            StartCoroutine(SearchLoading());

            YouTubeSearcher.Search(query, () =>
            {
                updateSearchResultsCoroutine = UpdateSearchResults(YouTubeSearcher.searchResults);
                StartCoroutine(updateSearchResultsCoroutine);
            });
        }

        #endregion

        #region Youtube Downloader
        private void VideoDownloaderDownloadProgress(VideoData video)
        {
            if (selectedLevel == video.level)
            {
                ChangeView(false);
                LoadVideoSettings(video);
            }
        }
        
        #endregion

        #region BS Events
        public void HandleDidSelectLevel(LevelCollectionViewController sender, IPreviewBeatmapLevel level)
        {
            selectedLevel = level;
            selectedVideo = null;
            ChangeView(false);
            Plugin.logger.Debug("Selected Level: " + level.songName);
        }

        private void GameSceneLoaded()
        {
            isPlayingLevel = true;
            StopAllCoroutines();
            ScreenManager.Instance.TryPlayVideo();
        }
        #endregion

        #region Classes
        public class VideoMenuStatus : MonoBehaviour
        {
            public event EventHandler DidEnable;
            public event EventHandler DidDisable;

            void OnEnable()
            {
                var handler = DidEnable;

                if(handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            void OnDisable()
            {
                var handler = DidDisable;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }
        #endregion
    }
}
