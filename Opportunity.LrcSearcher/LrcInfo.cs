using System.Text;
using System;

namespace Opportunity.LrcSearcher
{
    public sealed class LrcInfo
    {
        public string Artist { get; }
        public string Title { get; }
        public string Album { get; }
        public string Lrycis { get; }

        internal LrcInfo(string title, string artist, string album, string lrycis)
        {
            this.Title = title;
            this.Artist = artist;
            this.Lrycis = lrycis;
            this.Album = album;
        }
    }
}