using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MusicVideoPlayer.Util;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MusicVideoPlayer.YT
{
    public class YouTubeDownloader : MonoBehaviour
    {
        class VideoDownload
        {
            public VideoData video;
            public int downloadAttempts;
            public float timeSinceLastUpdate;

            public VideoDownload(VideoData video)
            {
                this.video = video;
                downloadAttempts = 0;
                timeSinceLastUpdate = 0;
            }

            public void Update()
            {
                timeSinceLastUpdate = 0;
            }
        }

        const float TimeoutDuration = 10;
        const int MaxRetries = 3;

        public event Action<VideoData> downloadProgress;
        
        public VideoQuality quality = VideoQuality.Medium;

        Queue<VideoDownload> videoQueue;
        bool downloading;
        bool updated;

        Process ydl;

        public static YouTubeDownloader Instance = null;
        public static void OnLoad()
        {
            if (!Instance)
            {
                Instance = new GameObject("YoutubeDownloader").AddComponent<YouTubeDownloader>();
                DontDestroyOnLoad(Instance);
                Instance.videoQueue = new Queue<VideoDownload>();
                Instance.quality = (VideoQuality)Plugin.config.GetInt("Settings", "VideoDownloadQuality", (int)VideoQuality.Medium, true);
                Instance.downloading = false;
                Instance.updated = false;
                Instance.UpdateYDL();
            }
        }

        public void EnqueueVideo(VideoData video)
        {
            videoQueue.Enqueue(new VideoDownload(video));
            if (!downloading)
            {
                video.downloadState = DownloadState.Queued;
                downloadProgress?.Invoke(video);
                StartCoroutine(DownloadVideo());
            }
        }

        public void DequeueVideo(VideoData video)
        {
            video.downloadState = DownloadState.Cancelled;
            downloadProgress?.Invoke(video);
        }
        
        private IEnumerator DownloadVideo()
        {
            downloading = true;

            if (!updated) yield return new WaitUntil(() => updated);
            
            VideoDownload download = videoQueue.Peek();
            VideoData video = download.video;

            if (video.downloadState == DownloadState.Cancelled || download.downloadAttempts > MaxRetries)
            {
                // skip
                videoQueue.Dequeue();
                
                if (videoQueue.Count > 0)
                {
                    // Start next download
                    DownloadVideo();
                }
                else
                {
                    // queue empty
                    downloading = false;
                    yield break;
                }
            }

            StopCoroutine(Countdown(download));

            video.downloadState = DownloadState.Downloading;
            downloadProgress?.Invoke(video);
            download.Update();

            string levelPath = VideoLoader.GetLevelPath(video.level);
            if (!Directory.Exists(levelPath)) Directory.CreateDirectory(levelPath);
            
            string videoFileName = video.title;
            // Strip invalid characters
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                videoFileName = videoFileName.Replace(c, '-');
            }
            videoFileName = videoFileName.Replace('\\', '-');
            videoFileName = videoFileName.Replace('/', '-');

            video.videoPath = videoFileName + ".mp4";

            // Download the video via youtube-dl 
            ydl = new Process();

            ydl.StartInfo.FileName = Environment.CurrentDirectory + "\\Youtube-dl\\youtube-dl.exe";
            ydl.StartInfo.Arguments =
                "https://www.youtube.com" + video.URL +
                " -f \"" + VideoQualitySetting.Format(quality) + "\"" + // Formats
                " --no-cache-dir" + // Don't use temp storage
                " -o \"" + levelPath + $"\\{videoFileName}.%(ext)s\"" +
                " --no-playlist" +  // Don't download playlists, only the first video
                " --no-part";  // Don't store download in parts, write directly to file
            
            ydl.StartInfo.RedirectStandardOutput = true;
            ydl.StartInfo.RedirectStandardError = true;
            ydl.StartInfo.UseShellExecute = false;
            ydl.StartInfo.CreateNoWindow = true;
            ydl.EnableRaisingEvents = true;

            ydl.Start();
            
            // Hook up our output to console
            ydl.BeginOutputReadLine();
            ydl.BeginErrorReadLine();

            ydl.OutputDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    Regex rx = new Regex(@"(\d*).\d%+");
                    Match match = rx.Match(e.Data);
                    if (match.Success)
                    {
                        video.downloadProgress = float.Parse(match.Value.Substring(0, match.Value.Length - 1)) / 100;
                        downloadProgress?.Invoke(video);
                        download.Update();
                    }
                }
            };

            ydl.ErrorDataReceived += (sender, e) => {
                if (e.Data.Length < 3) return;
                //to do: check these errors are problems - redownload or skip file when an error occurs
                //video.downloadState = DownloadState.Cancelled;
                downloadProgress?.Invoke(video);
                download.Update();
            };

            ydl.Exited += (sender, e) => {
                
                StopCoroutine(Countdown(download));

                if (video.downloadState == DownloadState.Cancelled)
                {
                    VideoLoader.Instance.DeleteVideo(video);
                }
                else
                {
                    video.downloadState = DownloadState.Downloaded;
                    VideoLoader.SaveVideoToDisk(video);
                    StartCoroutine(VerifyDownload(video));
                }

                videoQueue.Dequeue();

                if (videoQueue.Count > 0)
                {
                    // Start next download
                    DownloadVideo();                    
                }
                else
                {
                    // queue empty
                    downloading = false;
                }
                ydl.Dispose();
            };
        }

        public void OnApplicationQuit()
        {
            StopAllCoroutines();
            ydl.Close(); // or .Kill()
            ydl.Dispose();
        }
        
        private IEnumerator VerifyDownload(VideoData video)
        {
            yield return new WaitForSecondsRealtime(1);

            if (File.Exists(VideoLoader.Instance.GetVideoPath(video)))
            {
                // video okay?
                downloadProgress?.Invoke(video);
            }
        }

        IEnumerator Countdown(VideoDownload download)
        {
            while (download.timeSinceLastUpdate < TimeoutDuration)
            {
                download.timeSinceLastUpdate += Time.deltaTime;
                yield return null;
            }
            Timeout();
        }

        private void Timeout()
        {
            VideoDownload download = videoQueue.Dequeue();
            ydl.Close(); // or .Kill()
            ydl.Dispose();
            download.downloadAttempts++;
            videoQueue.Enqueue(download);
            DownloadVideo();
        }

        private void UpdateYDL()
        {
            ydl = new Process();
            ydl.StartInfo.FileName = Environment.CurrentDirectory + "\\Youtube-dl\\youtube-dl.exe";
            ydl.StartInfo.Arguments = "-U";
            ydl.StartInfo.RedirectStandardOutput = true;
            ydl.StartInfo.RedirectStandardError = true;
            ydl.StartInfo.UseShellExecute = false;
            ydl.StartInfo.CreateNoWindow = true;
            ydl.EnableRaisingEvents = true;

            ydl.Start();

            ydl.BeginOutputReadLine();
            ydl.BeginErrorReadLine();

            ydl.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Plugin.logger.Info(e.Data);
                }
            };

            ydl.ErrorDataReceived += (sender, e) =>
            {
                Plugin.logger.Warn(e.Data);
                //to do: check these errors problems - redownload or skip file when an error occurs
            };

            ydl.Exited += (sender, e) =>
            {
                updated = true;
                ydl.Dispose();
            };
        }

    }
}
