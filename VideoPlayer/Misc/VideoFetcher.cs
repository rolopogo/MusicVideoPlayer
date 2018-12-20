using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SongLoaderPlugin;
using UnityEngine;
using System.IO;

namespace MusicVideoPlayer.Misc
{
    public static class VideoFetcher
    {
        public static string FetchURLForPlayingSong()
        {
            var standardLevelSceneSetup = Resources.FindObjectsOfTypeAll<StandardLevelSceneSetup>().FirstOrDefault();
            if (standardLevelSceneSetup == null) return null;

            IBeatmapLevel level = standardLevelSceneSetup.GetPrivateField<StandardLevelSceneSetupDataSO>("_standardLevelSceneSetupData")?.difficultyBeatmap?.level;
            if (level == null) return null;
            
            return FetchURL(level);
        }

        public static string FetchURL(IBeatmapLevel level)
        {

            string songDirectory = PathForLevel(level);
            if (songDirectory == null) return null;

            FileInfo file = new FileInfo(Path.Combine(songDirectory, "video.mp4"));

            if (file.Exists)
            {
                return Path.Combine(songDirectory, "video.mp4");
            }

            return "";
        }

        public static bool SongHasVideo(IBeatmapLevel level)
        {
            return !FetchURL(level).Equals("");
        }

        public static string PathForLevel(IBeatmapLevel level)
        {
            return SongLoader.CustomLevels.Find(x => x.customSongInfo.GetIdentifier() == level.levelID)?.customSongInfo?.path;
        }
    }
}
