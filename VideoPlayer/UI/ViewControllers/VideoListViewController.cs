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
using HtmlAgilityPack;

namespace MusicVideoPlayer.UI.ViewControllers
{

    class VideoListViewController : CustomListViewController
    {
        public event Action searchButtonPressed;
        public event Action<YTResult> downloadButtonPressed;
        
        public List<YTResult> resultsList = new List<YTResult>();
        
        private Button _searchButton;
        private Button _downloadButton;
        
        private GameObject _loadingIndicator;
        
        private int _lastSelectedRow;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);

            if (firstActivation && type == ActivationType.AddedToHierarchy)
            {   
                _backButton = BeatSaberUI.CreateBackButton(rectTransform as RectTransform, delegate () { backButtonPressed?.Invoke(); });
                
                RectTransform container = new GameObject("VideoListContainer", typeof(RectTransform)).transform as RectTransform;
                container.SetParent(rectTransform, false);
                container.sizeDelta = new Vector2(105f, 0f);
                
                _searchButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, -20), new Vector2(30, 8), () =>
                {
                    searchButtonPressed?.Invoke();
                }, "Refine");

                _downloadButton = BeatSaberUI.CreateUIButton(rectTransform, "CreditsButton", new Vector2(60, -30), new Vector2(30, 8), () =>
                {
                    downloadButtonPressed?.Invoke(resultsList[_lastSelectedRow]);
                }, "Download");
//                _downloadButton.GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
                                
                _loadingIndicator = BeatSaberUI.CreateLoadingSpinner(rectTransform);
                (_loadingIndicator.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
                (_loadingIndicator.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
                (_loadingIndicator.transform as RectTransform).anchoredPosition = new Vector2(0f, 0f);
                _loadingIndicator.SetActive(true);

                _customListTableView.didSelectCellWithIdxEvent -= DidSelectRowEvent;
                _customListTableView.didSelectCellWithIdxEvent += _songsTableView_DidSelectRowEvent;

                (_customListTableView.transform.parent as RectTransform).sizeDelta = new Vector2(105,0);
                (_customListTableView.transform as RectTransform).anchorMin = new Vector2(0f, 0.5f);
                (_customListTableView.transform as RectTransform).anchorMax = new Vector2(1f, 0.5f);
                (_customListTableView.transform as RectTransform).sizeDelta = new Vector2(0f, 60f);
                (_customListTableView.transform as RectTransform).anchoredPosition = new Vector3(-10f, 0f);
            }
            else
            {
                _customListTableView.ReloadData();
            }
            _downloadButton.interactable = false;
        }

        internal void Refresh()
        {
            _customListTableView.ReloadData();
            if(_lastSelectedRow > -1)
                _customListTableView.SelectCellWithIdx(_lastSelectedRow);
        }

        protected override void DidDeactivate(DeactivationType type)
        {
            _lastSelectedRow = -1;
        }
        
        public void SetContent(List<YTResult> videos)
        {
            if (videos == null && resultsList != null)
            {
                resultsList.Clear();
            }
            else
            {
                resultsList = new List<YTResult>(videos);
            }

            if (_customListTableView != null)
            {
                _customListTableView.ReloadData();
                _customListTableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Center, false);
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
            DidSelectRowEvent?.Invoke(_customListTableView,row);
            _downloadButton.interactable = true;
        }

        public override int NumberOfCells()
        {
            return resultsList.Count;
        }

        public override TableCell CellForIdx(TableView tableView, int row)
        {
            LevelListTableCell _tableCell = GetTableCell(false);
            RawImage coverImage = _tableCell.GetPrivateField<RawImage>("_coverRawImage");
            TextMeshProUGUI songName = _tableCell.GetPrivateField<TextMeshProUGUI>("_songNameText");
            TextMeshProUGUI description = _tableCell.GetPrivateField<TextMeshProUGUI>("_authorText");

            coverImage.rectTransform.sizeDelta = new Vector2(160f/ 9f, coverImage.rectTransform.sizeDelta.y);
            songName.rectTransform.anchoredPosition += new Vector2(160f / 9f, 0f);
            description.rectTransform.anchoredPosition += new Vector2(160f / 9f, 0f);

            songName.text = string.Format("{0}\n<size=80%>{1}</size>", resultsList[row].title, resultsList[row].author);
            songName.color = Color.white;
            description.text = "[" + resultsList[row].duration + "]" + resultsList[row].description;
            description.color = Color.white;

            StartCoroutine(LoadScripts.LoadSprite(resultsList[row].thumbnailURL, coverImage.GetComponent<UnityEngine.UI.RawImage>(), 16f/9f));
            _tableCell.reuseIdentifier = "VideosTableCell";

            return _tableCell;
        }
    }
}
