using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SongLoaderPlugin;
using UnityEngine;
using System.IO;
using IllusionPlugin;
using SongLoaderPlugin.OverrideClasses;
using System.Diagnostics;
using MusicVideoPlayer.YT;

namespace MusicVideoPlayer.Util
{
    public class VideoLoader : MonoBehaviour
    {
        public event Action VideosLoadedEvent;
        public bool AreVideosLoaded { get; private set; }
        public bool AreVideosLoading { get; private set; }

        public bool autoDownload = false;

        private Dictionary<IBeatmapLevel, VideoData> videos;

        private HMTask _loadingTask;
        private bool _loadingCancelled;

        public static VideoLoader Instance;

        public static void OnLoad()
        {
            if (Instance != null) return;
            new GameObject("VideoFetcher").AddComponent<VideoLoader>();
            
        }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            autoDownload = ModPrefs.GetBool(Plugin.PluginName, "autoDownload", false, true);
            SongLoader.SongsLoadedEvent += RetrieveAllVideoData;

            DontDestroyOnLoad(gameObject);
        }
        
        public string GetVideoPath(IBeatmapLevel level)
        {
            VideoData vid;
            if (videos.TryGetValue(level, out vid)) return GetVideoPath(vid);
            return null;
        }

        public string GetVideoPath(VideoData video)
        {
            return Path.Combine(GetLevelPath(video.level), video.videoPath);
        }

        public VideoData GetVideo(IBeatmapLevel level)
        {
            VideoData vid;
            if (videos.TryGetValue(level, out vid)) return vid;
            return null;
        }

        public static string GetLevelPath(IBeatmapLevel level)
        {
            if (level is CustomLevel)
            {
                // Custom song
                return (level as CustomLevel).customSongInfo.path;
            }
            else
            {
                // OST
                var videoFileName = level.songName;
                // strip invlid characters
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    videoFileName = videoFileName.Replace(c, '-');
                }
                videoFileName = videoFileName.Replace('\\', '-');
                videoFileName = videoFileName.Replace('/', '-');

                return Path.Combine(Environment.CurrentDirectory, "CustomSongs", "_OST", videoFileName);
            }
        }

        public bool SongHasVideo(IBeatmapLevel level)
        {
            return videos.ContainsKey(level);
        }

        public void AddVideo(VideoData video)
        {
            videos.Add(video.level, video);
        }

        public void RemoveVideo(VideoData video)
        {
            videos.Remove(video.level);
        }

        public static void SaveVideoToDisk(VideoData video)
        {
            if (video == null) return;
            if (!Directory.Exists(GetLevelPath(video.level))) Directory.CreateDirectory(GetLevelPath(video.level));
            File.WriteAllText(Path.Combine(GetLevelPath(video.level), "video.json"), JsonUtility.ToJson(video));

            //using (StreamWriter streamWriter = File.CreateText(Path.Combine(GetLevelPath(video.level), "video.json")))
            //{
            //    streamWriter.Write(JsonUtility.ToJson(video));
            //}
        }

        private void RetrieveAllVideoData(SongLoader songLoader, List<CustomLevel> levels)
        {
            videos = new Dictionary<IBeatmapLevel, VideoData>();
            RetrieveCustomLevelVideoData(songLoader, levels);
            RetrieveOSTVideoData();
        }

        private void RetrieveOSTVideoData()
        {
            BeatmapLevelSO[] levels = Resources.FindObjectsOfTypeAll<BeatmapLevelSO>().Where(x=> x.GetType() != typeof(CustomLevel)).ToArray();
            
            Action job = delegate
            {
                try
                {

                    float i = 0;
                    foreach (var level in levels)
                    {
                        i++;
                        var videoFileName = level.songName;
                        // strip invlid characters
                        foreach (var c in Path.GetInvalidFileNameChars())
                        {
                            videoFileName = videoFileName.Replace(c, '-');
                        }
                        videoFileName = videoFileName.Replace('\\', '-');
                        videoFileName = videoFileName.Replace('/', '-');
                        
                        var songPath = Path.Combine(Environment.CurrentDirectory, "CustomSongs", "_OST", videoFileName);
                        
                        if (!Directory.Exists(songPath)) continue;
                        var results = Directory.GetFiles(songPath, "video.json", SearchOption.AllDirectories);
                        if (results.Length == 0)
                        {
                            continue;
                        }

                        var result = results[0];

                        try
                        {
                            var i1 = i;
                            HMMainThreadDispatcher.instance.Enqueue(delegate
                            {
                                if (_loadingCancelled) return;
                                VideoData video = LoadVideo(result, level.difficultyBeatmapSets[0].difficultyBeatmaps[0].level);
                                if (video != null)
                                {
                                    AddVideo(video);
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[MVP] Failed to load song folder: " + result);
                            Console.WriteLine(e.ToString());
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("[MVP] RetrieveOSTVideoData failed:");
                    Console.WriteLine(e.ToString());
                }
            };

            Action finish = delegate
            {

                AreVideosLoaded = true;
                AreVideosLoading = false;

                _loadingTask = null;

                VideosLoadedEvent?.Invoke();
            };

            _loadingTask = new HMTask(job, finish);
            _loadingTask.Run();
        }

        private void RetrieveCustomLevelVideoData(SongLoader songLoader, List<CustomLevel> levels)
        {
            Action job = delegate
            {
                try
                {
                    float i = 0;
                    foreach (CustomLevel level in levels)
                    {
                        i++;
                        var songPath = level.customSongInfo.path;
                        var results = Directory.GetFiles(songPath, "video.json", SearchOption.AllDirectories);
                        if (results.Length == 0)
                        {
                            continue;
                        }

                        var result = results[0];

                        try
                        {
                            var i1 = i;
                            HMMainThreadDispatcher.instance.Enqueue(delegate
                            {
                                if (_loadingCancelled) return;
                                VideoData video = LoadVideo(result, level.difficultyBeatmapSets[0].difficultyBeatmaps[0].level);
                                if (video != null)
                                {
                                    AddVideo(video);
                                }
                                
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[MVP] Failed to load song folder: " + result);
                            Console.WriteLine(e.ToString());
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("[MVP] RetrieveCustomLevelVideoData failed:");
                    Console.WriteLine(e.ToString());
                }
            };

            Action finish = delegate
            {
                AreVideosLoaded = true;
                AreVideosLoading = false;

                _loadingTask = null;

                VideosLoadedEvent?.Invoke();
            };

            _loadingTask = new HMTask(job, finish);
            _loadingTask.Run();
        }

        public void DeleteVideo(VideoData video)
        {
            File.Delete(GetVideoPath(video));
        }

        private VideoData LoadVideo(string jsonPath, IBeatmapLevel level)
        {
            var infoText = File.ReadAllText(jsonPath);
            VideoData vid;
            try
            {
                vid = JsonUtility.FromJson<VideoData>(infoText);
            }
            catch (Exception)
            {
                Console.WriteLine("[MVP] Error parsing video json: " + jsonPath);
                return null;
            }

            vid.level = level;

            if (!File.Exists(GetVideoPath(vid)))
            {
                if (autoDownload) {
                    Console.WriteLine("[MVP] Couldn't find Video: " + vid.title + " queueing for download...");
                    YouTubeDownloader.Instance.EnqueueVideo(vid);
                }
            }
            else
            {
                vid.downloadState = DownloadState.Downloaded;
            }

            return vid;
        }
    }
}
