using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MusicVideoPlayer.Util
{
    public enum VideoPlacement { Background, Left, Right, Bottom, Top, Custom };

    public class VideoPlacementSetting
    {   
        public static Vector3 Position(VideoPlacement placement)
        {
            switch (placement)
            {
                case VideoPlacement.Background:
                    return new Vector3(0, 20, 50);
                case VideoPlacement.Left:
                    return new Vector3(-8, 2, 10);
                case VideoPlacement.Right:
                    return new Vector3(8, 2, 10);
                case VideoPlacement.Bottom:
                    return new Vector3(0, -2, 7);
                case VideoPlacement.Top:
                    return new Vector3(0, 5, 7);
                default: // Custom
                    return ModPrefs.GetString(Plugin.PluginName, "CustomPosition", new Vector3(0, 4, 15).ToString(), true).ToVector3();
            }
        }

        public static Vector3 Rotation(VideoPlacement placement)
        {
            switch (placement)
            {
                case VideoPlacement.Background:
                    return new Vector3(0, 0, 0);
                case VideoPlacement.Left:
                    return new Vector3(0, -40, 0);
                case VideoPlacement.Right:
                    return new Vector3(0, 40, 0);
                case VideoPlacement.Bottom:
                    return new Vector3(15, 0, 0);
                case VideoPlacement.Top:
                    return new Vector3(-30, 0, 0);
                default: // Custom
                    return ModPrefs.GetString(Plugin.PluginName, "CustomRotation", new Vector3(0, 0, -10).ToString(), true).ToVector3();
            }
        }

        public static float Scale(VideoPlacement placement)
        {
            switch (placement)
            {
                case VideoPlacement.Background:
                    return 30;
                case VideoPlacement.Left:
                    return 5;
                case VideoPlacement.Right:
                    return 5;
                case VideoPlacement.Bottom:
                    return 3;
                case VideoPlacement.Top:
                    return 3;
                default: // Custom
                    return ModPrefs.GetFloat(Plugin.PluginName, "CustomScale", 8f, true);
            }
        }

        public static float[] Modes()
        {
            return new float[]
            {
                (float)VideoPlacement.Background,
                (float)VideoPlacement.Left,
                (float)VideoPlacement.Right,
                (float)VideoPlacement.Bottom,
                (float)VideoPlacement.Top,
                (float)VideoPlacement.Custom
            };
        }

        public static string Name(VideoPlacement mode)
        {
            switch (mode)
            {
                case VideoPlacement.Background:
                    return "Background";
                case VideoPlacement.Left:
                    return "Left";
                case VideoPlacement.Right:
                    return "Right";
                case VideoPlacement.Bottom:
                    return "Bottom";
                case VideoPlacement.Top:
                    return "Top";
                case VideoPlacement.Custom:
                    return "Custom";
                default:
                    return "?";
            }
        }
    }
}
