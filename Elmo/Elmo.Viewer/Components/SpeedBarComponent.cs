﻿using System.Threading.Tasks;
using System.Xml;
using Elmo.Viewer.Utilities;

namespace Elmo.Viewer.Components
{
    internal class SpeedBarItem
    {
        public string Label { get; }
        public string Title { get; }
        public string Url { get; }

        public SpeedBarItem(string label, string title, string url)
        {
            Label = label;
            Title = title;
            Url = url;
        }
    }

    internal static class SpeedBarComponent
    {
        public static SpeedBarItem GetHome(string url) => new SpeedBarItem("Errors", "List of logged errors.", url);

        public static SpeedBarItem GetRssFeed(string rootUrl) => new SpeedBarItem("RSS Feed", "RSS feed of recent errors.", $"{rootUrl}/rss");

        public static SpeedBarItem GetRssDigestFeed(string rootUrl) => new SpeedBarItem("RSS Digest", "RSS feed of errors within recent days.", $"{rootUrl}/digestrss");

        public static SpeedBarItem GetDownloadLog(string rootUrl) => new SpeedBarItem("Download Log", "Download the entire log as CSV.", $"{rootUrl}/download");

        public static async Task RenderAsync(XmlWriter writer, params SpeedBarItem[] items)
        {
            if (items == null || items.Length == 0)
                return;

            await writer.WriteStartElementAsync("ul");
            await writer.WriteAttributeStringAsync("id", "SpeedList");

            foreach (var item in items)
            {
                await writer.WriteStartElementAsync("li");

                await writer.WriteStartElementAsync("a");
                await writer.WriteAttributeStringAsync("href", item.Url);
                await writer.WriteAttributeStringAsync("title", item.Title);
                await writer.WriteStringAsync(item.Label);
                await writer.WriteEndElementAsync();

                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }
    }
}
