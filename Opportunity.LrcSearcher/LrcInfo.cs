using System.Text;
using System;

namespace Opportunity.LrcSearcher
{
    public sealed class LrcInfo
    {
        public string Artist { get; }
        public string Title { get; }
        public string Album { get; }
        public Uri Uri { get; }

        internal LrcInfo(string title, string artist, string album, Uri uri)
        {
            this.Title = title;
            this.Artist = artist;
            this.Uri = uri;
            this.Album = album;
        }
    }
}