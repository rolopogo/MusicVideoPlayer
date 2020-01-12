using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using MusicVideoPlayer.YT;
using SongCore;

namespace MusicVideoPlayer.Util
{
    public class VideoLoader : MonoBehaviour
    {
        public event Action VideosLoadedEvent;
        public bool AreVideosLoaded { get; private set; }
        public bool AreVideosLoading { get; private set; }

        private Dictionary<IPreviewBeatmapLevel, VideoData> videos;

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

            SongCore.Loader.SongsLoadedEvent += RetrieveAllVideoData;

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

        public VideoData GetVideo(IPreviewBeatmapLevel level)
        {
            VideoData vid;
            if (videos.TryGetValue(level, out vid)) return vid;
            return null;
        }

        public static string GetLevelPath(IPreviewBeatmapLevel level)
        {
            if (level is CustomPreviewBeatmapLevel)
            {
                // Custom song
                return (level as CustomPreviewBeatmapLevel).customLevelPath;
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

        private void RetrieveAllVideoData(Loader loader, Dictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            videos = new Dictionary<IPreviewBeatmapLevel, VideoData>();
            RetrieveCustomLevelVideoData(loader, levels);
            RetrieveOSTVideoData();
        }

        private void RetrieveOSTVideoData()
        {
            BeatmapLevelSO[] levels = Resources.FindObjectsOfTypeAll<BeatmapLevelSO>().Where(x=> x.GetType() != typeof(CustomBeatmapLevel)).ToArray();
            
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
                            Plugin.logger.Error("Failed to load song folder: " + result);
                            Plugin.logger.Error(e.ToString());
                        }
                    }

                }
                catch (Exception e)
                {
                    Plugin.logger.Error("RetrieveOSTVideoData failed:");
                    Plugin.logger.Error(e.ToString());
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

        private void RetrieveCustomLevelVideoData(Loader loader, Dictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            Action job = delegate
            {
                try
                {
                    float i = 0;
                    foreach (var level in levels)
                    {
                        i++;
                        var songPath = level.Value.customLevelPath;
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
                                
                                VideoData video = LoadVideo(result, level.Value);
                                if (video != null)
                                {
                                    AddVideo(video);
                                }
                                
                            });
                        }
                        catch (Exception e)
                        {
                            Plugin.logger.Error("Failed to load song folder: " + result);
                            Plugin.logger.Error(e.ToString());
                        }
                    }

                }
                catch (Exception e)
                {
                    Plugin.logger.Error("RetrieveCustomLevelVideoData failed:");
                    Plugin.logger.Error(e.ToString());
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
            RemoveVideo(video);
            File.Delete(Path.Combine(GetLevelPath(video.level), "video.json"));
            File.Delete(GetVideoPath(video));
        }

        private VideoData LoadVideo(string jsonPath, IPreviewBeatmapLevel level)
        {
            var infoText = File.ReadAllText(jsonPath);
            VideoData vid;
            try
            {
                vid = JsonUtility.FromJson<VideoData>(infoText);
            }
            catch (Exception)
            {
                Plugin.logger.Warn("Error parsing video json: " + jsonPath);
                return null;
            }

            vid.level = level;

            var path = GetVideoPath(vid);
            Plugin.logger.Debug("Level: " + level.songName);
            Plugin.logger.Debug("Path: " + path);
            Plugin.logger.Debug("JSON: " + infoText + "\n\n");

            if (File.Exists(GetVideoPath(vid)))
            {
                vid.downloadState = DownloadState.Downloaded;
            }

            return vid;
        }
    }
}
