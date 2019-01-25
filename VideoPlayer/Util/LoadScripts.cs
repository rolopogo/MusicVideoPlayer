using HMUI;
using Image = UnityEngine.UI.Image;
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

        //static public IEnumerator LoadAudio(string audioPath, object obj, string fieldName)
        //{
        //    using (var www = new WWW(audioPath))
        //    {
        //        yield return www;
        //        ReflectionUtil.SetPrivateField(obj, fieldName, www.GetAudioClip(true, true, AudioType.UNKNOWN));
        //    }
        //}

    }
}
