using HMUI;
using Image = UnityEngine.UI.Image;
using RawImage = UnityEngine.UI.RawImage;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MusicVideoPlayer.Util
{
    class LoadScripts
    {
        static public Dictionary<string, Sprite> _cachedSprites = new Dictionary<string, Sprite>();

        static public IEnumerator LoadSprite(string spritePath, Image image, float aspectRatio)
        {
            Texture2D tex;

            if (_cachedSprites.ContainsKey(spritePath))
            {
                image.sprite = _cachedSprites[spritePath];
                yield break;
            }

            using (WWW www = new WWW(spritePath))
            {
                yield return www;
                tex = www.texture;

                float newHeight = tex.width / aspectRatio;
                float bottom = (tex.height - newHeight) / 2;

                var newSprite = Sprite.Create(tex, new Rect(0, bottom, tex.width, newHeight), Vector2.one * 0.5f, 100, 1);
                _cachedSprites.Add(spritePath, newSprite);
                image.sprite = newSprite;
            }
        }

        static public IEnumerator LoadSprite(string spritePath, RawImage image, float aspectRatio)
        {
            Texture2D tex;

            if (_cachedSprites.ContainsKey(spritePath))
            {
                image.texture = textureFromSprite(_cachedSprites[spritePath]);
                yield break;
            }

            using (WWW www = new WWW(spritePath))
            {
                yield return www;
                tex = www.texture;

                float newHeight = tex.width / aspectRatio;
                float bottom = (tex.height - newHeight) / 2;

                var newSprite = Sprite.Create(tex, new Rect(0, bottom, tex.width, newHeight), Vector2.one * 0.5f, 100, 1);
                _cachedSprites.Add(spritePath, newSprite);
                image.texture = textureFromSprite(newSprite);
            }
        }

        public static Texture2D textureFromSprite(Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }

        static public IEnumerator LoadSprite(string spritePath, TableCell obj)
        {
            Texture2D tex;

            if (_cachedSprites.ContainsKey(spritePath))
            {
                obj.GetComponentsInChildren<Image>(true).First(x => x.name == "CoverImage").sprite = _cachedSprites[spritePath];
                yield break;
            }

            using (WWW www = new WWW(spritePath))
            {
                yield return www;
                tex = www.texture;
                float border = (tex.height - (tex.height * 9f / 16f));
                var newSprite = Sprite.Create(tex, new Rect(0, border / 2f, tex.width, tex.height - border), Vector2.one * 0.5f, 100, 1);
                _cachedSprites.Add(spritePath, newSprite);
                obj.GetComponentsInChildren<Image>(true).First(x => x.name == "CoverImage").sprite = newSprite;
            }
        }
    }
}
