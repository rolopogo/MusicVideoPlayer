using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MusicVideoPlayer.Util
{
    public enum VideoQuality { Best, High, Medium, Low, Custom };

    public class VideoQualitySetting
    {
        public static string Format(VideoQuality quality)
        {
            switch (quality)
            {
                case VideoQuality.Best:
                    return "(bestvideo/best)[ext=mp4]";
                case VideoQuality.High:
                    return "(bestvideo/best)[height<=720][ext=mp4]";
                case VideoQuality.Medium:
                    return "(bestvideo/best)[height<=480][ext=mp4]";
                case VideoQuality.Low:
                    return "(bestvideo/best)[height<480][ext=mp4]";
                default: // Custom
                    return Plugin.config.GetString("Settings", "CustomDownloadFormat", "bestvideo[filesize<10M][ext=mp4]", true);
            }
        }

        public static float[] Modes()
        {
            return new float[]
            {
                (float)VideoQuality.Best,
                (float)VideoQuality.High,
                (float)VideoQuality.Medium,
                (float)VideoQuality.Low,
                (float)VideoQuality.Custom
            };
        }

        public static string Name(VideoQuality mode)
        {
            switch (mode)
            {
                case VideoQuality.Best:
                    return "Best";
                case VideoQuality.High:
                    return "High";
                case VideoQuality.Medium:
                    return "Medium";
                case VideoQuality.Low:
                    return "Low";
                case VideoQuality.Custom: // Custom
                    return "Custom";
                default:
                    return "?";
            }
        }
    }
}
