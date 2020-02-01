using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Parser;
using BS_Utils.Utilities;
using HMUI;
using MusicVideoPlayer.UI;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.YT;
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
        IPreviewBeatmapLevel selectedLevel;

        VideoData selectedVideo;

        [UIObject("current-video-player")]
        private GameObject currentVideoPlayer;

        [UIComponent("video-details")]
        private RectTransform videoDetailsView;

        [UIComponent("video-search-results")]
        private RectTransform videoSearchResultsView;

        [UIComponent("video-list")]
        private CustomListTableData customListTableData;

        [UIComponent("current-video-title")]
        private TextMeshProUGUI currentVideoTitle;

        [UIComponent("current-video-description")]
        private TextMeshProUGUI currentVideoDescription;

        [UIComponent("current-video-offset")]
        private TextMeshProUGUI currentVideoOffset;

        [UIComponent("preview-button")]
        private TextMeshProUGUI previewButtonText;

        [UIComponent("search-results-loading")]
        private TextMeshProUGUI searchResultsLoading;

        [UIComponent("looping-button")]
        private TextMeshProUGUI LoopingButtonText;

        [UIComponent("offset-magnitude")]
        private ClickableText offsetMagnitude;

        [UIComponent("offset-decrease-button")]
        private Button OffsetDecreaseButton;

        [UIComponent("offset-increase-button")]
        private Button OffsetIncreaseButton;

        [UIComponent("delete-button")]
        private Button DeleteButton;

        [UIComponent("download-button")]
        private Button DownloadButton;

        [UIComponent("refine-button")]
        private Button RefineButton;

        [UIComponent("save-button")]
        private Button SaveButton;

        [UIComponent("preview-button")]
        private Button PreviewButton;

        [UIComponent("looping-button")]
        private Button LoopingButton;

        [UIComponent("search-keyboard")]
        private ModalKeyboard searchKeyboard;

        [UIParams]
        private BSMLParserParams parserParams;

        private SongPreviewPlayer songPreviewPlayer;

        private bool isPreviewing = false;

        private bool isOffsetInSeconds = false;

        private IEnumerator updateSearchResultsCoroutine = null;

        private IEnumerator videoPlayerCheckCoroutine = null;

        private int selectedCell;

        public void OnLoad()
        {
            Setup();
        }

        internal void Setup()
        {
            YouTubeDownloader.Instance.downloadProgress += VideoDownloaderDownloadProgress;
            BSEvents.levelSelected += HandleDidSelectLevel;
            BSEvents.gameSceneLoaded += GameSceneLoaded;
            BSEvents.menuSceneActive += MenuSceneActive;
            songPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();

            videoDetailsView.gameObject.SetActive(true);
            videoSearchResultsView.gameObject.SetActive(false);

            videoPlayerCheckCoroutine = CheckEnabled();
        }

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
                currentVideoTitle.text = $"[{videoData.duration}] {videoData.title} by {videoData.author}";
                currentVideoDescription.text = videoData.description;
                currentVideoOffset.text = videoData.offset.ToString();
                EnableButtons(true);
                UpdateLooping();
            }
            else
            {
                currentVideoTitle.text = "NO VIDEO SET";
                currentVideoDescription.text = "";
                currentVideoOffset.text = "N/A";
                EnableButtons(false);
            }

            ScreenManager.Instance.PrepareVideo(videoData);
        }

        private void EnableButtons(bool enable)
        {
            DeleteButton.interactable = enable;
            OffsetDecreaseButton.interactable = enable;
            OffsetIncreaseButton.interactable = enable;
            SaveButton.interactable = enable;
            LoopingButton.interactable = enable;
            PreviewButton.interactable = enable;
        }

        private void SetPreviewState()
        {
            if(isPreviewing)
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
            ScreenManager.Instance.PauseVideo();
            songPreviewPlayer.FadeOut();
            SetPreviewState();
        }

        private void ChangeView(bool searchView)
        {
            StopPreview();
            ResetSearchView();
            videoDetailsView.gameObject.SetActive(!searchView);
            videoSearchResultsView.gameObject.SetActive(searchView);

            if(!searchView)
            {
                parserParams.EmitEvent("hide-keyboard");
                StopCoroutine(videoPlayerCheckCoroutine);
                StartCoroutine(videoPlayerCheckCoroutine);

                LoadVideoSettings(selectedVideo);
            }
        }

        private void ResetSearchView()
        {
            if (updateSearchResultsCoroutine != null)
            {
                StopCoroutine(updateSearchResultsCoroutine);
            }

            StopCoroutine(SearchLoading());

            customListTableData.data.Clear();
            customListTableData.tableView.ReloadData();
            selectedCell = -1;
        }

        private void UpdateLooping()
        {
            if (selectedVideo != null)
            {
                if (selectedVideo.loop)
                {
                    LoopingButtonText.text = "Loop";
                }
                else
                {
                    LoopingButtonText.text = "Once";
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
                currentVideoOffset.text = magnitude.ToString();
            }
        }

        private void Save()
        {
            if(selectedVideo != null)
            {
                VideoLoader.SaveVideoToDisk(selectedVideo);
            }
        }

        private IEnumerator UpdateSearchResults(List<YTResult> results)
        {
            List<CustomListTableData.CustomCellInfo> videos = new List<CustomListTableData.CustomCellInfo>();

            foreach (var result in results)
            {
                var item = new CustomListTableData.CustomCellInfo(result.title, result.description);

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

            RefineButton.interactable = true;
            searchResultsLoading.gameObject.SetActive(false);
        }

        private IEnumerator SearchLoading()
        {
            int count = 0;
            string loadingText = "Loading Results";
            searchResultsLoading.gameObject.SetActive(true);

            while(searchResultsLoading.gameObject.activeInHierarchy)
            {
                string periods = string.Empty;
                count++;

                for (int i = 0; i < count; i++)
                {
                    periods += ".";
                }

                if(count == 3)
                {
                    count = 0;
                }

                searchResultsLoading.SetText(loadingText + periods);

                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator CheckEnabled()
        {
            while(true)
            {
                if (currentVideoPlayer != null && currentVideoPlayer.activeInHierarchy)
                {
                    ScreenManager.Instance.SetScale(new Vector3(0.57f, 0.57f, 1f));
                    ScreenManager.Instance.SetPosition(currentVideoPlayer.transform.position);
                    ScreenManager.Instance.SetRotation(currentVideoPlayer.transform.eulerAngles);
                }
                else
                {
                    ScreenManager.Instance.SetPlacement(MVPSettings.instance.PlacementMode);
                }

                yield return null;
            }
        }

        #region Actions
        [UIAction("on-looping-action")]
        private void OnLoopingAction()
        {
            selectedVideo.loop = !selectedVideo.loop;
            UpdateLooping();
        }

        [UIAction("on-offset-magnitude-action")]
        private void OnOffsetMagnitudeAction()
        {
            isOffsetInSeconds = !isOffsetInSeconds;

            if(isOffsetInSeconds)
            {
                offsetMagnitude.text = "S";
            }
            else
            {
                offsetMagnitude.text = "MS";
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
            Plugin.logger.Debug("Is Preview: " + isPreviewing);
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

        [UIAction("on-save-action")]
        private void OnSaveAction()
        {
            Save();
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
                DownloadButton.interactable = true;
                Plugin.logger.Debug($"Selected Cell: {YouTubeSearcher.searchResults[idx].ToString()}");
            }
            else
            {
                DownloadButton.interactable = false;
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
            DownloadButton.interactable = false;
            RefineButton.interactable = false;
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
                OnBackAction();
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

        private void MenuSceneActive()
        {
            ScreenManager.Instance.ShowScreen();
            ChangeView(false);
        }

        private void GameSceneLoaded()
        {
            StopAllCoroutines();

            ScreenManager.Instance.SetPlacement(MVPSettings.instance.PlacementMode);

            if (!ScreenManager.Instance.IsVideoPlayable())
            {
                ScreenManager.Instance.HideScreen();
            }
            else
            {
                ScreenManager.Instance.ShowScreen();
            }
        }
        #endregion
    }
}
