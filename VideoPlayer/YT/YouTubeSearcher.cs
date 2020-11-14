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

        static IEnumerator SearchYoutubeCoroutine(string search, Action callback)
        {
            searchInProgress = true;
            searchResults = new List<YTResult>();

            // get youtube results
            string url = "https://www.youtube.com/results?search_query=" + search;
            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                Plugin.logger.Warn("Search: An Error occured while searching. " + www.error);
                yield break;
            }

            MemoryStream stream = new MemoryStream(www.bytes);

            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream, System.Text.Encoding.UTF8);

            var videoNodes = doc.DocumentNode.SelectNodes("//*[contains(concat(' ', @class, ' '),'yt-lockup-video')]");
            if (videoNodes == null)
            {
                Plugin.logger.Info("[MVP] Search: No results found matching: " + search);
            }
            else
            {
                for (int i = 0; i < Math.Min(MaxResults, videoNodes.Count); i++)
                {
                    var node = HtmlNode.CreateNode(videoNodes[i].InnerHtml);
                    YTResult data = new YTResult();
                    
                    // title
                    var titleNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-uix-tile-link')]");
                    if (titleNode == null)
                    {
                        continue;
                    }
                    data.title = HttpUtility.HtmlDecode(titleNode.InnerText);
                    
                    // description
                    var descNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-lockup-description')]");
                    if (descNode == null)
                    {
                        continue;
                    }
                    data.description = HttpUtility.HtmlDecode(descNode.InnerText);
                    
                    // duration
                    var durationNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'video-time')]");
                    if (durationNode == null)
                    {
                        // no duration means this is a live streamed video
                        continue;
                    }
                    data.duration = HttpUtility.HtmlDecode(durationNode.InnerText);
                    
                    // author node
                    var authorNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-lockup-byline')]");
                    if (authorNode == null)
                    {
                        continue;
                    }
                    data.author = HttpUtility.HtmlDecode(authorNode.InnerText);
                    
                    // url
                    var urlNode = node.SelectSingleNode("//*[contains(concat(' ', @class, ' '),'yt-uix-tile-link')]");
                    if (urlNode == null)
                    {
                        continue;
                    }
                    data.URL = urlNode.Attributes["href"].Value;

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
                    // append data to results
                    searchResults.Add(data);
                }
            }
            if (callback != null) callback.Invoke();
            searchInProgress = false;
        }

        public static void Search(string query, Action callback)
        {
            if (searchInProgress) SharedCoroutineStarter.instance.StopCoroutine("SearchYoutubeCoroutine");
            SharedCoroutineStarter.instance.StartCoroutine(SearchYoutubeCoroutine(query, callback));
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
            return String.Format("{0} by {1} [{2}] \n {3} \n {4} \n {5}", title, author, duration, URL, description, thumbnailURL);
        }
    }
}
