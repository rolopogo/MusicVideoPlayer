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
            Plugin.logger.Info("tex");

            if (_cachedSprites.ContainsKey(spritePath))
            {
                Plugin.logger.Info("spritepath");
                image.sprite = _cachedSprites[spritePath];
                Plugin.logger.Info("set sprite 4");
                yield break;
            }

            using (WWW www = new WWW(spritePath))
            {
                yield return www;
                tex = www.texture;
                Plugin.logger.Info("tex3");

                float newHeight = tex.width / aspectRatio;
                Plugin.logger.Info("height");
                float bottom = (tex.height - newHeight) / 2;
                Plugin.logger.Info("botto");

                var newSprite = Sprite.Create(tex, new Rect(0, bottom, tex.width, newHeight), Vector2.one * 0.5f, 100, 1);
                Plugin.logger.Info("newsprite");
                _cachedSprites.Add(spritePath, newSprite);
                Plugin.logger.Info("added");
                image.sprite = newSprite;
            }
        }

        static public IEnumerator LoadSprite(string spritePath, TableCell obj)
        {
            Texture2D tex;
            Plugin.logger.Info(_cachedSprites.Keys.ToString());
            if (_cachedSprites.ContainsKey(spritePath))
            {
                Plugin.logger.Info("sprite path found");
                obj.GetComponentsInChildren<Image>(true).First(x => x.name == "CoverImage").sprite = _cachedSprites[spritePath];
                Plugin.logger.Info("set sprite");
                yield break;
            }

            using (WWW www = new WWW(spritePath))
            {
                Plugin.logger.Info("WWWSprite");
                yield return www;
                tex = www.texture;
                Plugin.logger.Info("tex");
                float border = (tex.height - (tex.height * 9f / 16f));
                Plugin.logger.Info("border");
                var newSprite = Sprite.Create(tex, new Rect(0, border / 2f, tex.width, tex.height - border), Vector2.one * 0.5f, 100, 1);
                Plugin.logger.Info("newSprite");
                _cachedSprites.Add(spritePath, newSprite);
                Plugin.logger.Info("cache sprite");
                obj.GetComponentsInChildren<Image>(true).First(x => x.name == "CoverImage").sprite = newSprite;
                Plugin.logger.Info("set sprite 2");
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
