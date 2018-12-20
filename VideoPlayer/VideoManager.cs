using IllusionPlugin;
using MusicVideoPlayer.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace MusicVideoPlayer
{
    public class VideoManager : MonoBehaviour
    {
        public static VideoManager Instance;

        GameObject screen;
        VideoPlayer vp;

        public static bool showVideo = false;
        public static VideoPlacement placement;

        public static void OnLoad()
        {
            new GameObject("VideoManager").AddComponent<VideoManager>();
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            showVideo = ModPrefs.GetBool(Plugin.PluginName, "showVideo", true, true);
            placement = (VideoPlacement)ModPrefs.GetInt(Plugin.PluginName, "ScreenPositionMode", (int)VideoPlacement.Bottom, true);
            
            DontDestroyOnLoad(gameObject);
        }

        void CreateScreen()
        {
            GameObject videoScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject overlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            screen = new GameObject("Screen");
            screen.transform.parent = transform;
            videoScreen.transform.parent = screen.transform;
            overlay.transform.parent = screen.transform;
            body.transform.parent = screen.transform;

            screen.transform.position = VideoPlacementSetting.Position(placement);
            screen.transform.eulerAngles = VideoPlacementSetting.Rotation(placement);
            screen.transform.localScale = VideoPlacementSetting.Scale(placement) * Vector3.one;
            
            videoScreen.transform.localScale = new Vector3(16f / 9f, 1, 1);
            overlay.transform.localPosition = -videoScreen.transform.forward * 0.01f;
            overlay.transform.localScale = new Vector3(16f / 9f, 1, 1);
            body.transform.localPosition = videoScreen.transform.forward * 0.06f;
            body.transform.localScale = new Vector3(16f / 9f + 0.1f, 1.1f, 0.1f);


            Renderer vsRenderer = videoScreen.GetComponent<Renderer>();
            vsRenderer.material = new Material(Shader.Find("Custom/SimpleTexture"));
            vsRenderer.material.color = new Color(0.9f, 0.9f, 0.9f);

            Renderer overlayRenderer = overlay.GetComponent<Renderer>();
            overlayRenderer.material = new Material(Shader.Find("Custom/SmokeParticle"));

            Renderer bodyRenderer = body.GetComponent<Renderer>();
            bodyRenderer.material = new Material(GameObject.Find("Column").GetComponent<Renderer>().material);

            vp = gameObject.AddComponent<VideoPlayer>();
            vp.isLooping = true;
            vp.renderMode = VideoRenderMode.MaterialOverride;
            vp.targetMaterialRenderer = vsRenderer;
            vp.targetMaterialProperty = "_MainTex";

        }

        public void PlayVideo(string url)
        {
            if (!showVideo) return;
            if (vp == null) CreateScreen();
            vp.url = url;
            vp.Play();
        }

        public void StopVideo()
        {
            if (vp == null) return;
            vp.Stop();
        }

        void Update()
        {
            if (screen == null) return;
            screen.SetActive(vp.isPlaying);
        }

        public static void SetPlacement(VideoPlacement placement)
        {
            VideoManager.placement = placement;
            if (VideoManager.Instance.screen == null) return;
            VideoManager.Instance.screen.transform.position = VideoPlacementSetting.Position(placement);
            VideoManager.Instance.screen.transform.eulerAngles = VideoPlacementSetting.Rotation(placement);
            VideoManager.Instance.screen.transform.localScale = VideoPlacementSetting.Scale(placement) * Vector3.one;
        }
    }
}
