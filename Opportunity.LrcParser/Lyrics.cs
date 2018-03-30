using System;
using System.Collections.Generic;

namespace Opportunity.LrcParser
{
    public class Lyrics
    {
        public static Lyrics Parse(string content)
        {
            return new Lyrics(new Parser(content));
        }

        internal Lyrics(Parser parser)
        {
            parser.Analyze();
            this.Lines = new SortedSet<Line>(parser.Lines, LineComparer.Instance);
        }

        public SortedSet<Line> Lines { get; }

        public MetaDataCollection MetaData { get; } = new MetaDataCollection();
    }
}
