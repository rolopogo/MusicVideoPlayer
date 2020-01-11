using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MusicVideoPlayer.UI.UIElements
{
    public class CustomUIElements : MonoBehaviour
    {
        private static Button _backButtonInstance;

        /// <summary>
        /// Creates a copy of a back button.
        /// </summary>
        /// <param name="parent">The transform to parent the new button to.</param>
        /// <param name="onClick">Callback for when the button is pressed.</param>
        /// <returns>The newly created back button.</returns>
        public static Button CreateBackButton(RectTransform parent, UnityAction onClick = null)
        {
            if (_backButtonInstance == null)
            {
                try
                {
                    _backButtonInstance = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "BackArrowButton"));
                }
                catch
                {
                    return null;
                }
            }

            Button btn = Instantiate(_backButtonInstance, parent, false);
            btn.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btn.name = "CustomUIButton";

            return btn;
        }

        /// <summary>
        /// Creates a copy of a template button and returns it.
        /// </summary>
        /// <param name="parent">The transform to parent the button to.</param>
        /// <param name="buttonTemplate">The name of the button to make a copy of. Example: "QuitButton", "PlayButton", etc.</param>
        /// <param name="anchoredPosition">The position the button should be anchored to.</param>
        /// <param name="sizeDelta">The size of the buttons RectTransform.</param>
        /// <param name="onClick">Callback for when the button is pressed.</param>
        /// <param name="buttonText">The text that should be shown on the button.</param>
        /// <param name="icon">The icon that should be shown on the button.</param>
        /// <returns>The newly created button.</returns>
        public static Button CreateUIButton(RectTransform parent, string buttonTemplate, Vector2 anchoredPosition, Vector2 sizeDelta, UnityAction onClick = null, string buttonText = "BUTTON", Sprite icon = null)
        {
            Button btn = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == buttonTemplate)), parent, false);
            btn.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btn.name = "CustomUIButton";

            (btn.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchoredPosition = anchoredPosition;
            (btn.transform as RectTransform).sizeDelta = sizeDelta;

            btn.SetButtonText(buttonText);
            if (icon != null)
                btn.SetButtonIcon(icon);

            return btn;
        }

        /// <summary>
        /// Adds hint text to any component that handles pointer events.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new HoverHint component to.</param>
        /// <param name="text">The text to be displayed on the HoverHint panel.</param>
        /// <returns>The newly created HoverHint component.</returns>
        public static HoverHint AddHintText(RectTransform parent, string text)
        {
            var hoverHint = parent.gameObject.AddComponent<HoverHint>();
            hoverHint.text = text;
            //hoverHint.name = "CustomHintText";
            HoverHintController hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            return hoverHint;
        }
    }
}
