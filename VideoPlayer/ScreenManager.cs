using CustomUI.Utilities;
using MusicVideoPlayer.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using VRUIControls;
using BSEvents = MusicVideoPlayer.Util.BSEvents;

namespace MusicVideoPlayer
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance;

        public static bool showVideo = false;
        public VideoPlacement placement;

        private VideoData currentVideo;
        private GameObject screen;
        private Renderer vsRenderer;
        private Shader glowShader;
        private MoverPointer _moverPointer;
        private CustomBloomPrePass _customBloomPrePass;
        private Color _onColor = Color.white.ColorWithAlpha(0) * 0.85f;

        public VideoPlayer videoPlayer;
        private float offsetSec = 0f;

        public static void OnLoad()
        {
            if (Instance == null)
                new GameObject("VideoManager").AddComponent<ScreenManager>();
        }

        void Start()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            showVideo = Plugin.config.GetBool("Settings", "showVideo", true, true);
            placement = (VideoPlacement)Plugin.config.GetInt("Settings", "ScreenPositionMode", (int)VideoPlacement.Bottom, true);

            BSEvents.songPaused += PauseVideo;
            BSEvents.songUnpaused += ResumeVideo;
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoaded;
            BSEvents.menuSceneLoaded += OnMenuSceneLoaded;
            BSEvents.gameSceneLoaded += OnGameSceneLoaded;

            DontDestroyOnLoad(gameObject);
            
            CreateScreen();
            HideScreen();
        }


        void Update()
        {
            if (screen == null) return;
            vsRenderer.material.SetTexture("_MainTex", videoPlayer.texture);
        }
        
        void CreateScreen()
        {
            screen = new GameObject("Screen");
            screen.AddComponent<BoxCollider>().size = new Vector3(16f / 9f + 0.1f, 1.1f, 0.1f);
            screen.transform.parent = transform;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (body.GetComponent<Collider>() != null) Destroy(body.GetComponent<Collider>());
            body.transform.parent = screen.transform;
            body.transform.localPosition = new Vector3(0, 0, 0.1f);
            body.transform.localScale = new Vector3(16f / 9f + 0.1f, 1.1f, 0.1f);
            Renderer bodyRenderer = body.GetComponent<Renderer>();
            bodyRenderer.material = new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "DarkEnvironment1")); // finding objects is wonky because platforms hides them

            GameObject videoScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            if (videoScreen.GetComponent<Collider>() != null) Destroy(videoScreen.GetComponent<Collider>());
            videoScreen.transform.parent = screen.transform;
            videoScreen.transform.localPosition = Vector3.zero;
            videoScreen.transform.localScale = new Vector3(16f / 9f, 1, 1);
            vsRenderer = videoScreen.GetComponent<Renderer>();
            vsRenderer.material = new Material(GetShader());
            vsRenderer.material.color = Color.clear;
            
            screen.transform.position = VideoPlacementSetting.Position(placement);
            screen.transform.eulerAngles = VideoPlacementSetting.Rotation(placement);
            screen.transform.localScale = VideoPlacementSetting.Scale(placement) * Vector3.one;

            _customBloomPrePass = videoScreen.AddComponent<CustomBloomPrePass>();
            
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.targetMaterialProperty = "_MainTex";
            videoPlayer.playOnAwake = false;
            videoPlayer.targetMaterialRenderer = vsRenderer;
            vsRenderer.material.SetTexture("_MainTex", videoPlayer.texture);
            videoPlayer.errorReceived += VideoPlayerErrorReceived;

            OnMenuSceneLoaded();
        }
        
        private void OnMenuSceneLoaded()
        {
            var pointer = Resources.FindObjectsOfTypeAll<VRPointer>().First();
            if (pointer == null) return;

            if (_moverPointer)
            {
                _moverPointer.wasMoved -= ScreenWasMoved;
                Destroy(_moverPointer);
            }
            _moverPointer = pointer.gameObject.AddComponent<MoverPointer>();
            _moverPointer.Init(screen.transform);
            _moverPointer.wasMoved += ScreenWasMoved;

            if(currentVideo != null) PrepareVideo(currentVideo);
            PauseVideo();
            HideScreen();
        }

        private void OnGameSceneLoaded()
        {
            var pointer = Resources.FindObjectsOfTypeAll<VRPointer>().Last();
            if (pointer == null) return;
            if (_moverPointer)
            {
                _moverPointer.wasMoved -= ScreenWasMoved;
                Destroy(_moverPointer);
            }
            _moverPointer = pointer.gameObject.AddComponent<MoverPointer>();
            _moverPointer.Init(screen.transform);
            _moverPointer.wasMoved += ScreenWasMoved;

            Plugin.logger.Info("Checking Playback Speed");
            var levelData = BS_Utils.Plugin.LevelData;
            float practiceOffset = 0;
            if (levelData.GameplayCoreSceneSetupData.practiceSettings != null)
            {
                Plugin.logger.Info("Practice mode");
                if (levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul != videoPlayer.playbackSpeed)
                {
                    Plugin.logger.Info("Changing Song Speed1");
                    videoPlayer.playbackSpeed = levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
                }

                practiceOffset = levelData.GameplayCoreSceneSetupData.practiceSettings.startSongTime;
            }
            else if (levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul != videoPlayer.playbackSpeed)
            {
                Plugin.logger.Info("Song Speed Mul Changed");
                videoPlayer.playbackSpeed = levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
                Plugin.logger.Info("changed Song Speed2");
            }

            if (videoPlayer.time != offsetSec + practiceOffset)
            {
                // game was restarted
                if (currentVideo.offset + practiceOffset >= 0)
                {
                    videoPlayer.time = offsetSec + practiceOffset;
                }
                else
                {
                    videoPlayer.time = 0;
                }
            }

            if(currentVideo != null)
            {
                if(currentVideo.downloadState == DownloadState.Downloaded)
                {
                    PlayVideo();
                }
            }
        }

        private void ScreenWasMoved(Vector3 pos, Quaternion rot, float scale)
        {
            screen.transform.position = pos;
            screen.transform.rotation = rot;
            screen.transform.localScale = scale *  Vector3.one;

            Plugin.config.SetString("Placement", "CustomPosition", pos.ToString());
            Plugin.config.SetString("Placement", "CustomRotation", rot.eulerAngles.ToString());
            Plugin.config.SetFloat("Placement", "CustomScale", scale);

            placement = VideoPlacement.Custom;
            Plugin.config.SetInt("Settings", "ScreenPositionMode", (int)placement);
        }

        private void VideoPlayerErrorReceived(VideoPlayer source, string message)
        {
            if (message == "Can't play movie []") return;
            Plugin.logger.Warn("Video player error: " + message);
        }

        public void PrepareVideo(VideoData video)
        {
            currentVideo = video;
            if (video == null)
            {
                videoPlayer.url = null;
                vsRenderer.material.color = Color.clear;
                return;
            }
            if (video.downloadState != DownloadState.Downloaded) return;
            videoPlayer.isLooping = video.loop;

            string videoPath = VideoLoader.Instance.GetVideoPath(video);
            videoPlayer.Pause();
            if(videoPlayer.url != videoPath) videoPlayer.url = videoPath;
            offsetSec = video.offset / 1000f; // ms -> s
            if (video.offset >= 0)
            {
                videoPlayer.time = offsetSec;
            } else
            {
                videoPlayer.time = 0;
            }
            if(!videoPlayer.isPrepared) videoPlayer.Prepare();
            vsRenderer.material.color = Color.clear;
            videoPlayer.Pause();
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                videoPlayer.EnableAudioTrack(i, false); //Mute Audio
            }
        }

        public void PlayVideo()
        {
            if (!showVideo) return;
            if (currentVideo == null) return;
            if (currentVideo.downloadState != DownloadState.Downloaded) return;

            ShowScreen();
            vsRenderer.material.color = _onColor;

            if (offsetSec < 0)
            {
                StopAllCoroutines();
                StartCoroutine(StartVideoDelayed(offsetSec));
            }
            else
            {
                videoPlayer.Play();
            }
        }

        private IEnumerator StartVideoDelayed(float offset)
        {
            if(offset >= 0)
            {
                videoPlayer.Play();
                yield break;
            }

            videoPlayer.frame = 0;

            // Wait
            float timeElapsed = 0;
            while (timeElapsed < -offset)
            {
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            // Time has elapsed, start video
            // frames are short enough that we shouldn't notice imprecise start time
            videoPlayer.Play();
        }
        
        public void PauseVideo()
        {
            StopAllCoroutines();
            if (videoPlayer == null) return;
            if (videoPlayer.isPlaying) videoPlayer.Pause();
        }

        public void ResumeVideo()
        {
            if (videoPlayer == null) return;
            if(!videoPlayer.isPlaying) videoPlayer.Play();
        }

        public void ShowScreen()
        {
            screen.SetActive(true);
        }

        public void HideScreen()
        {
            screen.SetActive(false);
        }
        
        public void SetPlacement(VideoPlacement placement)
        {
            this.placement = placement;
            if (Instance.screen == null) return;
            screen.transform.position = VideoPlacementSetting.Position(placement);
            screen.transform.eulerAngles = VideoPlacementSetting.Rotation(placement);
            screen.transform.localScale = VideoPlacementSetting.Scale(placement) * Vector3.one;
        }

        public Shader GetShader()
        {
            if (glowShader != null) return glowShader;
            // load shader

            var myLoadedAssetBundle = AssetBundle.LoadFromMemory(UIUtilities.GetResource(Assembly.GetExecutingAssembly(), "MusicVideoPlayer.Resources.mvp.bundle"));
        
            Shader shader = myLoadedAssetBundle.LoadAsset<Shader>("ScreenGlow");
            myLoadedAssetBundle.Unload(false);

            glowShader = shader;
            return shader;
        }
    }
}
