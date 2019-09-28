using System;
using HtmlAgilityPack;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace MusicVideoPlayer.YT
{
    public class YouTubeSearcher
    {
        const int MaxResults = 15;
        public static List<YTResult> searchResults;
        static bool searchInProgress = false;

        static IEnumerator SearchYoutubeCoroutine(string search, IPreviewBeatmapLevel level, Action callback)
        {
            searchInProgress = true;
            searchResults = new List<YTResult>();
            Plugin.logger.Info("SYC");

            // get youtube results
            string url = "https://www.youtube.com/results?search_query=" + search;
            WWW www = new WWW(url);
            yield return www;
            Plugin.logger.Info("got url");

            if (www.error != null)
            {
                Plugin.logger.Warn("Search: An Error occured while searching. " + www.error);
                yield break;
            }

            MemoryStream stream = new MemoryStream(www.bytes);

            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream, System.Text.Encoding.UTF8);

            var videoNodes = doc.DocumentNode.SelectNodes("//*[contains(concat(' ', @class, ' '),'yt-lockup-video')]");
            Plugin.logger.Info("Nodes Selected");
            if (videoNodes == null)
            {
                Plugin.logger.Info("[MVP] Search: No results found matching: " + search);
            }
            else
            {
                for (int i = 0; i < Math.Min(MaxResults, videoNodes.Count); i++)
                {
                    Plugin.logger.Info("For each: " + i);
                    var node = HtmlNode.CreateNode(videoNodes[i].InnerHtml);
                    YTResult data = new YTResult();
                    
                    // title
                    var titleNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-uix-tile-link')]");
                    if (titleNode == null)
                    {
                        continue;
                    }
                    data.title = HttpUtility.HtmlDecode(titleNode.InnerText);
                    Plugin.logger.Info("title");
                    
                    // description
                    var descNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-lockup-description')]");
                    if (descNode == null)
                    {
                        continue;
                    }
                    data.description = HttpUtility.HtmlDecode(descNode.InnerText);
                    Plugin.logger.Info("desc");
                    
                    // duration
                    var durationNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'video-time')]");
                    if (durationNode == null)
                    {
                        // no duration means this is a live streamed video
                        continue;
                    }
                    data.duration = HttpUtility.HtmlDecode(durationNode.InnerText);
                    Plugin.logger.Info("dur");
                    
                    // author node
                    var authorNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-lockup-byline')]");
                    if (authorNode == null)
                    {
                        continue;
                    }
                    data.author = HttpUtility.HtmlDecode(authorNode.InnerText);
                    Plugin.logger.Info("author");
                    
                    // url
                    var urlNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-uix-tile-link')]");
                    if (urlNode == null)
                    {
                        continue;
                    }
                    data.URL = urlNode.Attributes["href"].Value;
                    Plugin.logger.Info("url");

                    var thumbnailNode = node.SelectSingleNode("//img");
                    if (thumbnailNode == null)
                    {
                        continue;
                    }
                    if (thumbnailNode.Attributes["data-thumb"] != null)
                    {
                        data.thumbnailURL = thumbnailNode.Attributes["data-thumb"].Value;
                    }
                    else
                    {
                        data.thumbnailURL = thumbnailNode.Attributes["src"].Value;
                    }
                    Plugin.logger.Info("thumb");
                    // append data to results
                    searchResults.Add(data);
                    Plugin.logger.Info("appended");
                }
                foreach (YTResult result in searchResults)
                {
                    Plugin.logger.Info(result.ToString());
                }
            }
            if (callback != null) callback.Invoke();
            searchInProgress = false;
        }

        public static void Search(string query, IPreviewBeatmapLevel level, Action callback)
        {
            if (searchInProgress) SharedCoroutineStarter.instance.StopCoroutine("SearchYoutubeCoroutine");
            SharedCoroutineStarter.instance.StartCoroutine(SearchYoutubeCoroutine(query, level, callback));
        }
    }


    public class YTResult
    {
        public string title;
        public string author;
        public string description;
        public string duration;
        public string URL;
        public string thumbnailURL;

        public new string ToString()
        {
            return $"{title} by {author} [{duration}]\n{URL}\n{description}\n{thumbnailURL}";
        }
    }
}
