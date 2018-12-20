using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MusicVideoPlayer.Misc;
using System.Diagnostics;
using IllusionPlugin;
using System.Text.RegularExpressions;

namespace MusicVideoPlayer
{
    public class YoutubeDownloader : MonoBehaviour
    {
        public event Action<Video> videoDownloaded;

        public event Action<Video> downloadProgress;

        public VideoQuality quality = VideoQuality.Medium;

        Queue<Video> videoQueue;
        bool downloading;

        Process ydl;

        private static YoutubeDownloader _instance = null;
        public static YoutubeDownloader Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = new GameObject("YoutubeDownloader").AddComponent<YoutubeDownloader>();
                    DontDestroyOnLoad(_instance);
                    _instance.videoQueue = new Queue<Video>();
                    _instance.quality = (VideoQuality)ModPrefs.GetInt(Plugin.PluginName, "VideoDownloadQuality", (int)VideoQuality.Medium, true);
                    _instance.downloading = false;
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public void EnqueueVideo(Video video)
        {
            videoQueue.Enqueue(video);
            if (!downloading)
            {
                DownloadVideo();
            }
        }

        private void DownloadVideo()
        {
            Video video = videoQueue.Peek();
            downloading = true;
            string videoPath = VideoFetcher.PathForLevel(video.level); 
            
            // Download the video via youtube-dl 
            ydl = new Process();

            ydl.StartInfo.FileName = Environment.CurrentDirectory + "/Youtube-dl/youtube-dl.exe";
            ydl.StartInfo.Arguments =
                "https://www.youtube.com" + video.URL +
                " -f \"" + VideoQualitySetting.Format(quality) + "\"" + // Formats
                " --no-cache-dir" + // Don't use temp storage
                " -o \"" + VideoFetcher.PathForLevel(video.level) + "\\video.%(ext)s\"" +
                " --no-playlist" +  // Don't download playlists, only the first video
                " --no-part";  // Don't store download in parts, write directly to file

            Console.WriteLine(ydl.StartInfo.Arguments);

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
                    Console.WriteLine(e.Data);

                    //[download]  81.8% of 40.55MiB at  4.80MiB/s ETA 00:01
                    //[download] Resuming download at byte 48809440
                    //
                    Regex rx = new Regex(@"(\d+).\d%+");
                    Match match = rx.Match(e.Data);
                    if (match.Success)
                    {
                        Console.WriteLine(match.Value);
                        video.downLoadProgress = float.Parse(match.Value.Substring(0, match.Value.Length - 2)) / 100;
                        downloadProgress?.Invoke(video);
                    }
                }
            };

            ydl.ErrorDataReceived += (sender, e) => {
                Console.WriteLine(e.Data);
                //if (e.Data != null && !e.Data.Contains("unable to obtain file audio codec"))
                //{
                //}
            };

            ydl.Exited += (sender, e) => {
                // to do: check that the file was indeed downloaded correctly before unblocking
                Console.WriteLine("Exited.");

                //// move
                //File.Move(
                //    Path.Combine(Environment.CurrentDirectory, "Youtube-dl", "temp.mp4"), 
                //    Path.Combine(VideoFetcher.PathForLevel(video.level), "video.mp4")
                //);

                Video old = videoQueue.Dequeue();
                videoDownloaded?.Invoke(old);

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
            };
        }

        public void OnApplicationQuit()
        {
            ydl.Close(); // or .Kill()
            ydl.Dispose();
        }

        public Video GetDownloadingVideo(IBeatmapLevel level)
        {
            return videoQueue.FirstOrDefault(x => x.level == level);
        }

    }
}
