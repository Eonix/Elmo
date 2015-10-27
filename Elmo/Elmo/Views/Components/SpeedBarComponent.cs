using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Elmo.Views.Components
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

        public static SpeedBarItem GetRssFeed(string rootUrl) => new SpeedBarItem("RSS Feed", "RSS feed of recent errors.", $"{rootUrl}rss");

        public static SpeedBarItem GetRssDigestFeed(string rootUrl) => new SpeedBarItem("RSS Digest", "RSS feed of errors within recent days.", $"{rootUrl}digestrss");

        public static SpeedBarItem GetDownloadLog(string rootUrl) => new SpeedBarItem("Download Log", "Download the entire log as CSV.", $"{rootUrl}download");

        //public static SpeedBarItem GetHelp() => new SpeedBarItem("Help", "Documentation, discussions, issues and more.", "#");

        public static async Task RenderAsync(XmlWriter writer, params SpeedBarItem[] items)
        {
            if (items == null || items.Length == 0)
                return;

            await writer.WriteStartElementAsync(null, "ul", null);
            await writer.WriteAttributeStringAsync(null, "id", null, "SpeedList");

            foreach (var item in items)
            {
                await writer.WriteStartElementAsync(null, "li", null);

                await writer.WriteStartElementAsync(null, "a", null);
                await writer.WriteAttributeStringAsync(null, "href", null, item.Url);
                await writer.WriteAttributeStringAsync(null, "title", null, item.Title);
                await writer.WriteStringAsync(item.Label);
                await writer.WriteEndElementAsync();

                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }
    }
}
