using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MusicVideoPlayer.Util
{
    public enum VideoQuality { Best, High, Medium, Low };

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
                case VideoQuality.Low:
                    return "(bestvideo/best)[height<480][ext=mp4]";
                default:
                    return "(bestvideo/best)[height<=480][ext=mp4]";
            }
        }

        public static float[] Modes()
        {
            return new float[]
            {
                (float)VideoQuality.Best,
                (float)VideoQuality.High,
                (float)VideoQuality.Medium,
                (float)VideoQuality.Low
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
                default:
                    return "?";
            }
        }
    }
}
