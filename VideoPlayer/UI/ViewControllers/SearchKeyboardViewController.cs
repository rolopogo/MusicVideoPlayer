//using MusicVideoPlayer.UI.UIElements;
//using System;
//using System.Linq;
//using UnityEngine;
//using VRUI;
//using UnityEngine.UI;
//using TMPro;
//using CustomUI.BeatSaber;

//namespace MusicVideoPlayer.UI.ViewControllers
//{
//    class SearchKeyboardViewController : VRUIViewController
//    {

//        GameObject _searchKeyboardGO;

//        CustomUIKeyboard _searchKeyboard;

//        TextMeshProUGUI _inputText;
//        public string _inputString = "";
        
//        private Button _titleButton;
//        private Button _subtitleButton;
//        private Button _artistButton;
        

//        public event Action<string> searchButtonPressed;
//        public event Action backButtonPressed;

//        protected override void DidActivate(bool firstActivation, ActivationType type)
//        {
//            if (type == ActivationType.AddedToHierarchy && firstActivation)
//            {
//                _searchKeyboardGO = Instantiate(Resources.FindObjectsOfTypeAll<UIKeyboard>().First(x => x.name != "CustomUIKeyboard"), rectTransform, false).gameObject;

//                Destroy(_searchKeyboardGO.GetComponent<UIKeyboard>());
//                _searchKeyboard = _searchKeyboardGO.AddComponent<CustomUIKeyboard>();

//                _searchKeyboard.textKeyWasPressedEvent += delegate (char input) { _inputString += input; UpdateInputText(); };
//                _searchKeyboard.deleteButtonWasPressedEvent += delegate () { _inputString = _inputString.Substring(0, _inputString.Length - 1); UpdateInputText(); };
//                _searchKeyboard.cancelButtonWasPressedEvent += () => { backButtonPressed?.Invoke(); };
//                _searchKeyboard.okButtonWasPressedEvent += () => { searchButtonPressed?.Invoke(_inputString); };

//                _inputText = BeatSaberUI.CreateText(rectTransform, "Search...", new Vector2(0f, 22f));
//                _inputText.alignment = TextAlignmentOptions.Center;
//                _inputText.fontSize = 6f;
//            }
//            else
//            {
//                _inputString = "";
//                UpdateInputText();
//            }

//        }

//        void UpdateInputText()
//        {
//            if (_inputText != null)
//            {
//                _inputText.text = _inputString.ToUpper();
//                if (string.IsNullOrEmpty(_inputString))
//                {
//                    _searchKeyboard.OkButtonInteractivity = false;
//                }
//                else
//                {
//                    _searchKeyboard.OkButtonInteractivity = true;
//                }
//            }
//        }

//        void ClearInput()
//        {
//            _inputString = "";
//        }

//        public void SetQuickButtons(IBeatmapLevel level)
//        {
//            if(_titleButton == null)
//            {
//                _titleButton = BeatSaberUI.CreateUIButton(rectTransform, "QuitButton", new Vector2(-35,35), new Vector2(30f, 6f));
//            }
//            _titleButton.SetButtonText(level.songName);
//            _titleButton.SetButtonTextSize(3);
//            _titleButton.onClick.RemoveAllListeners();
//            _titleButton.onClick.AddListener(delegate() { _inputString += level.songName + " "; UpdateInputText(); });

//            if (_subtitleButton == null)
//            {
//                _subtitleButton = BeatSaberUI.CreateUIButton(rectTransform, "QuitButton", new Vector2(0, 35), new Vector2(30f, 6f));
//            }
//            _subtitleButton.SetButtonText(level.songSubName);
//            _subtitleButton.SetButtonTextSize(3);
//            _subtitleButton.onClick.RemoveAllListeners();
//            _subtitleButton.onClick.AddListener(delegate () { _inputString += level.songSubName + " "; UpdateInputText(); });

//            if (_artistButton == null)
//            {
//                _artistButton = BeatSaberUI.CreateUIButton(rectTransform, "QuitButton", new Vector2(35, 35), new Vector2(30f, 6f));
//            }
//            _artistButton.SetButtonText(level.songAuthorName);
//            _artistButton.SetButtonTextSize(3);
//            _artistButton.onClick.RemoveAllListeners();
//            _artistButton.onClick.AddListener(delegate () { _inputString += level.songAuthorName + " "; UpdateInputText(); });
//        }
//    }
//}
