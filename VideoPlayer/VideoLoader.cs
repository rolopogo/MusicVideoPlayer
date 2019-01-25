using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SongLoaderPlugin;
using UnityEngine;
using System.IO;
using SimpleJSON;
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

            SongLoader.SongsLoadedEvent += RetrieveAllVideoData;

            DontDestroyOnLoad(gameObject);
        }

        public string FetchURLForPlayingSong()
        {
            var standardLevelSceneSetup = Resources.FindObjectsOfTypeAll<StandardLevelSceneSetup>().FirstOrDefault();
            if (standardLevelSceneSetup == null) return null;

            IBeatmapLevel level = standardLevelSceneSetup.GetPrivateField<StandardLevelSceneSetupDataSO>("_standardLevelSceneSetupData")?.difficultyBeatmap?.level;
            if (level == null) return null;

            return GetVideoPath(level);
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
            if (level.levelID.Length < 32)
            {
                // OST
                return Path.Combine(Environment.CurrentDirectory, "CustomSongs", "_OST", level.songName);
            }
            else
            {
                // Custom song
                return SongLoader.CustomLevels.Find(x => x.customSongInfo.GetIdentifier() == level.levelID)?.customSongInfo?.path;
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
            //File.WriteAllText(Path.Combine(GetLevelPath(video.level), "video.json"), JsonUtility.ToJson(video));

            using (StreamWriter streamWriter = File.CreateText(Path.Combine(GetLevelPath(video.level), "video.json")))
            {
                streamWriter.Write(JsonUtility.ToJson(video));
            }
        }

        private void RetrieveAllVideoData(SongLoader songLoader, List<CustomLevel> levels)
        {
            videos = new Dictionary<IBeatmapLevel, VideoData>();
            RetrieveCustomLevelVideoData(songLoader, levels);
            RetrieveOSTVideoData();
        }

        private void RetrieveOSTVideoData()
        {
            LevelSO[] levels = Resources.FindObjectsOfTypeAll<LevelSO>().Where(x=> x.GetType() != typeof(CustomLevel)).ToArray();
            
            Action job = delegate
            {
                try
                {

                    float i = 0;
                    foreach (var level in levels)
                    {
                        i++;
                        var songPath = Path.Combine(Environment.CurrentDirectory, "CustomSongs", "_OST", level.songName);
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
                                VideoData video = LoadVideo(result, level.difficultyBeatmaps[0].level);
                                if (video != null)
                                {
                                    AddVideo(video);
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to load song folder: " + result);
                            Console.WriteLine(e.ToString());
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("RetrieveOSTVideoData failed:");
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
                    foreach (var level in levels)
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
                                VideoData video = LoadVideo(result, level.difficultyBeatmaps[0].level);
                                if (video != null)
                                {
                                    AddVideo(video);
                                }
                                
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to load song folder: " + result);
                            Console.WriteLine(e.ToString());
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("RetrieveCustomLevelVideoData failed:");
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
                Console.WriteLine("Error parsing video json: " + jsonPath);
                return null;
            }

            vid.level = level;

            if (!File.Exists(GetVideoPath(vid)))
            {
                Console.WriteLine("Couldn't find Video: " + vid.videoPath + " queueing for download...");
                YouTubeDownloader.Instance.EnqueueVideo(vid);
            }
            else
            {
                vid.downloadState = DownloadState.Downloaded;
            }

            return vid;
        }
    }
}
