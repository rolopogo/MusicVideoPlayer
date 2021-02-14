﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using SongCore;
using MusicVideoPlayer.YT;

namespace MusicVideoPlayer.Util
{
    public class VideoLoader : MonoBehaviour
    {
        public event Action VideosLoadedEvent;
        public bool AreVideosLoaded { get; private set; }
        public bool AreVideosLoading { get; private set; }

        public bool autoDownload = false;

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

            autoDownload = Plugin.config.GetBool("Settings", "autoDownload", false, true);
            Loader.SongsLoadedEvent += RetrieveAllVideoData;

            DontDestroyOnLoad(gameObject);
        }
        
        public string GetVideoPath(IPreviewBeatmapLevel level)
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

        public bool SongHasVideo(IPreviewBeatmapLevel level)
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

        private void RetrieveAllVideoData(Loader songLoader, Dictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            videos = new Dictionary<IPreviewBeatmapLevel, VideoData>();
            List<CustomPreviewBeatmapLevel> LevelList = new List<CustomPreviewBeatmapLevel>(levels.Values);
            RetrieveCustomLevelVideoData(LevelList);
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

        private void RetrieveCustomLevelVideoData(List<CustomPreviewBeatmapLevel> levels)
        {
            Action job = delegate
            {
                try
                {
                    float i = 0;
                    foreach (CustomPreviewBeatmapLevel level in levels)
                    {
                        i++;
                        var songPath = level.customLevelPath;
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
                                VideoData video = LoadVideo(result, level);
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

            if (!File.Exists(GetVideoPath(vid)))
            {
                if (autoDownload) {
                    Plugin.logger.Info("Couldn't find Video: " + vid.videoPath + " queueing for download...");
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
