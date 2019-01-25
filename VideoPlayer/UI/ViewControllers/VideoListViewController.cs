using System;
using System.Collections.Generic;
using System.Linq;
using VRUI;
using UnityEngine.UI;
using HMUI;
using TMPro;
using UnityEngine;
using CustomUI.BeatSaber;
using CustomUI.Utilities;
using MusicVideoPlayer.Util;
using MusicVideoPlayer.YT;

namespace MusicVideoPlayer.UI.ViewControllers
{
    enum TopButtonsState { Select, SortBy, Search, Playlists };

    class VideoListViewController : VRUIViewController, TableView.IDataSource
    {
        public event Action<int> didSelectRow;

        public event Action searchButtonPressed;
        public event Action<YTResult> downloadButtonPressed;
        public event Action backButtonPressed;
        
        public List<YTResult> resultsList = new List<YTResult>();

        private Button _pageUpButton;
        private Button _pageDownButton;

        private Button _searchButton;
        private Button _downloadButton;
        private Button _backButton;

        private GameObject _loadingIndicator;

        private TableView _videosTableView;
        private LevelListTableCell _videoListTableCellInstance;

        private int _lastSelectedRow;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation && type == ActivationType.AddedToHierarchy)
            {   
                _backButton = BeatSaberUI.CreateBackButton(rectTransform as RectTransform, delegate () { backButtonPressed?.Invoke(); });
                
                RectTransform container = new GameObject("VideoListContainer", typeof(RectTransform)).transform as RectTransform;
                container.SetParent(rectTransform, false);
                container.sizeDelta = new Vector2(105f, 0f);
                
                _pageUpButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "PageUpButton")), rectTransform, false);
                (_pageUpButton.transform as RectTransform).anchorMin = new Vector2(0.5f, 1f);
                (_pageUpButton.transform as RectTransform).anchorMax = new Vector2(0.5f, 1f);
                (_pageUpButton.transform as RectTransform).anchoredPosition = new Vector2(0f, -10f);
                (_pageUpButton.transform as RectTransform).sizeDelta = new Vector2(40f, 10f);
                _pageUpButton.interactable = true;
                _pageUpButton.onClick.AddListener(delegate ()
                {
                    _videosTableView.PageScrollUp();
                });

                _pageDownButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "PageDownButton")), rectTransform, false);
                (_pageDownButton.transform as RectTransform).anchorMin = new Vector2(0.5f, 0f);
                (_pageDownButton.transform as RectTransform).anchorMax = new Vector2(0.5f, 0f);
                (_pageDownButton.transform as RectTransform).anchoredPosition = new Vector2(0f, 10f);
                (_pageDownButton.transform as RectTransform).sizeDelta = new Vector2(40f, 10f);
                _pageDownButton.interactable = true;
                _pageDownButton.onClick.AddListener(delegate ()
                {
                    _videosTableView.PageScrollDown();
                });
                
                _searchButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, -20), new Vector2(30, 8), () =>
                {
                    searchButtonPressed?.Invoke();
                }, "Refine");

                _downloadButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, -30), new Vector2(30, 8), () =>
                {
                    downloadButtonPressed?.Invoke(resultsList[_lastSelectedRow]);
                }, "Download");
                _downloadButton.GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
                                
                _loadingIndicator = BeatSaberUI.CreateLoadingSpinner(rectTransform);
                (_loadingIndicator.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
                (_loadingIndicator.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
                (_loadingIndicator.transform as RectTransform).anchoredPosition = new Vector2(0f, 0f);
                _loadingIndicator.SetActive(true);
                
                _videoListTableCellInstance = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));
                _videosTableView = new GameObject().AddComponent<TableView>();
                _videosTableView.transform.SetParent(container, false);

                _videosTableView.SetPrivateField("_isInitialized", false);
                _videosTableView.SetPrivateField("_preallocatedCells", new TableView.CellsGroup[0]);
                _videosTableView.Init();

                RectMask2D viewportMask = Instantiate(Resources.FindObjectsOfTypeAll<RectMask2D>().First(), _videosTableView.transform, false);
                viewportMask.transform.DetachChildren();
                _videosTableView.GetComponentsInChildren<RectTransform>().First(x => x.name == "Content").transform.SetParent(viewportMask.rectTransform, false);

                (_videosTableView.transform as RectTransform).anchorMin = new Vector2(0f, 0.5f);
                (_videosTableView.transform as RectTransform).anchorMax = new Vector2(1f, 0.5f);
                (_videosTableView.transform as RectTransform).sizeDelta = new Vector2(0f, 60f);
                (_videosTableView.transform as RectTransform).anchoredPosition = new Vector3(-10f, 0f);
                
                _videosTableView.dataSource = this;
                _videosTableView.didSelectRowEvent += _songsTableView_DidSelectRowEvent;
            }
            else
            {
                _videosTableView.ReloadData();
            }
            _downloadButton.interactable = false;
        }

        internal void Refresh()
        {
            _videosTableView.ReloadData();
            if(_lastSelectedRow > -1)
                _videosTableView.SelectRow(_lastSelectedRow);
        }

        protected override void DidDeactivate(DeactivationType type)
        {
            _lastSelectedRow = -1;
        }
        
        public void SetContent(List<YTResult> videos)
        {
            if(videos == null && resultsList != null)
                resultsList.Clear();
            else
                resultsList = new List<YTResult>(videos);

            if (_videosTableView != null)
            {
                _videosTableView.ReloadData();
                _videosTableView.ScrollToRow(0, false);
                _lastSelectedRow = -1;
            }
        }

        public void SetLoadingState(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(isLoading);
            }
        }
        
        private void _songsTableView_DidSelectRowEvent(TableView sender, int row)
        {
            _lastSelectedRow = row;
            didSelectRow?.Invoke(row);
            _downloadButton.interactable = true;
        }

        public float RowHeight()
        {
            return 10f;
        }

        public int NumberOfRows()
        {

            return resultsList.Count;
        }

        public TableCell CellForRow(int row)
        {
            LevelListTableCell _tableCell = Instantiate(_videoListTableCellInstance);

            // fix aspect ratio
            (_tableCell.transform.Find("CoverImage") as RectTransform).sizeDelta = new Vector2(160f/9f, 10);
            (_tableCell.transform.Find("CoverImage") as RectTransform).anchoredPosition += new Vector2(160f / 9f / 2f, 0f);
            (_tableCell.transform.Find("CoverImage") as RectTransform).GetComponent<UnityEngine.UI.Image>().preserveAspect = false;

            (_tableCell.transform.Find("SongName") as RectTransform).anchoredPosition += new Vector2(160f / 9f, 0f);
            (_tableCell.transform.Find("Author") as RectTransform).anchoredPosition += new Vector2(160f / 9f, 0f);

            _tableCell.reuseIdentifier = "VideosTableCell";
            _tableCell.songName = string.Format("{0}\n<size=80%>{1}</size>", resultsList[row].title, resultsList[row].author);
            _tableCell.author = "[" + resultsList[row].duration + "]" + resultsList[row].description;
            StartCoroutine(LoadScripts.LoadSprite(resultsList[row].thumbnailURL, _tableCell));

            return _tableCell;
        }
    }
}
