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

        [UIParams]
        private BSMLParserParams parserParams;

        private SongPreviewPlayer songPreviewPlayer;

        private bool isPreviewing = false;

        private IEnumerator coroutineHandle = null;

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

            videoDetailsView.gameObject.SetActive(false);
            videoSearchResultsView.gameObject.SetActive(true);
        }

        public void LoadVideoSettings(VideoData videoData)
        {
            StopPreview();

            if (videoData == null)
            {
                videoData = VideoLoader.Instance.GetVideo(selectedLevel);
            }


            if (coroutineHandle == null)
            {
                coroutineHandle = CheckEnabled();
                StartCoroutine(coroutineHandle);
            }

            selectedVideo = videoData;

            if (videoData != null)
            {
                currentVideoTitle.text = $"[{videoData.duration}] {videoData.title} by {videoData.author}";
                currentVideoDescription.text = videoData.description;
                currentVideoOffset.text = videoData.offset.ToString();
            }
            else
            {
                currentVideoTitle.text = "NO VIDEO SET";
                currentVideoDescription.text = "N/A";
                currentVideoOffset.text = "N/A";
            }

            ScreenManager.Instance.PrepareVideo(videoData);
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

        IEnumerator UpdateSearchResults(List<YTResult> results)
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
        }

        private IEnumerator CheckEnabled()
        {
            while(true)
            {
                //if (currentVideoPlayer != null && currentVideoPlayer.activeInHierarchy)
                //{
                //    ScreenManager.Instance.SetScale(new Vector3(0.57f, 0.57f, 1f));
                //    ScreenManager.Instance.SetPosition(currentVideoPlayer.transform.position);
                //    ScreenManager.Instance.SetRotation(currentVideoPlayer.transform.eulerAngles);
                //}
                //else
                //{
                //    ScreenManager.Instance.SetPlacement(MVPSettings.instance.PlacementMode);
                //}

                yield return null;
            }
        }

        #region Actions
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
            videoDetailsView.gameObject.SetActive(false);
            videoSearchResultsView.gameObject.SetActive(true);

            parserParams.EmitEvent("show-keyboard");
        }

        [UIAction("on-refine-action")]
        private void OnRefineAction()
        {
            OnSearchAction();
        }

        [UIAction("on-query")]
        private void OnQueryAction(string query)
        {
            YouTubeSearcher.Search(query, () =>
            {
                StartCoroutine(UpdateSearchResults(YouTubeSearcher.searchResults));
            });
        }
        #endregion

        #region Youtube Downloader
        private void VideoDownloaderDownloadProgress(VideoData video)
        {
            if (selectedLevel == video.level)
            {
                LoadVideoSettings(video);
            }
        }
        #endregion

        #region BS Events
        public void HandleDidSelectLevel(LevelCollectionViewController sender, IPreviewBeatmapLevel level)
        {
            selectedLevel = level;
            LoadVideoSettings(null);
            Plugin.logger.Debug("Selected Level: " + level.songName);
        }

        private void MenuSceneActive()
        {
            ScreenManager.Instance.ShowScreen();

            if (coroutineHandle == null)
            {
                coroutineHandle = CheckEnabled();
                StartCoroutine(coroutineHandle);
            }
        }

        private void GameSceneLoaded()
        {
            StopCoroutine(coroutineHandle);
            coroutineHandle = null;

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
