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
            Plugin.logger.Info("SetContent");
            if (videos == null && resultsList != null)
            {
                Plugin.logger.Info("Clearing");
                resultsList.Clear();
                Plugin.logger.Info("Cleared");
            }
            else
            {
                Plugin.logger.Info("new results");
                resultsList = new List<YTResult>(videos);
                Plugin.logger.Info("results made");
            }

            if (_customListTableView != null)
            {
                Plugin.logger.Info("not null");
                Plugin.logger.Info(_customListTableView.numberOfCells.ToString());
                Plugin.logger.Info(_customListTableView.ToString());
                _customListTableView.ReloadData();
                Plugin.logger.Info("reloaded");
                _customListTableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Center, false);
                Plugin.logger.Info("reloaded");
                _lastSelectedRow = -1;
                Plugin.logger.Info("-1");
            }
            Plugin.logger.Info("passed Set Content");
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
            Plugin.logger.Info($"Row:{row}");
            LevelListTableCell _tableCell = GetTableCell(false);
            Plugin.logger.Info("gotTableCell");

            var coverImage = _tableCell.transform.Find("CoverImage");

            // fix aspect ratios
            (coverImage as RectTransform).sizeDelta = new Vector2(160f / 9f, 10);
            Plugin.logger.Info("coverimage");
            (coverImage as RectTransform).anchoredPosition += new Vector2(160f / 9f / 2f, 0f);
            Plugin.logger.Info("anchored");
//            (_tableCell.transform.Find("CoverImage") as RectTransform).GetComponent<UnityEngine.UI.Image>().preserveAspect = true;
//            (_tableCell.transform.Find("CoverImage") as RectTransform).preserveAspect = true;
//            Plugin.logger.Info("preserve");

            (_tableCell.transform.Find("SongName") as RectTransform).anchoredPosition += new Vector2(160f / 9f, 0f);
            Plugin.logger.Info("song name anchored");
            (_tableCell.transform.Find("Author") as RectTransform).anchoredPosition += new Vector2(160f / 9f, 0f);
            Plugin.logger.Info("fixed aspect ratios");

            // Fill in data
            _tableCell.GetPrivateField<TextMeshProUGUI>("_songNameText").text = string.Format("{0}\n<size=80%>{1}</size>", resultsList[row].title, resultsList[row].author);
            _tableCell.GetPrivateField<TextMeshProUGUI>("_songNameText").color = Color.white;
            _tableCell.GetPrivateField<TextMeshProUGUI>("_authorText").text = "[" + resultsList[row].duration + "]" + resultsList[row].description;
            _tableCell.GetPrivateField<TextMeshProUGUI>("_authorText").color = Color.white;
            Plugin.logger.Info("Loading Sprites");


            StartCoroutine(LoadScripts.LoadSprite(resultsList[row].thumbnailURL, coverImage.GetComponent<UnityEngine.UI.RawImage>(), 16f/9f));
            _tableCell.reuseIdentifier = "VideosTableCell";
            Plugin.logger.Info("filled data");

            return _tableCell;
        }
    }
}
