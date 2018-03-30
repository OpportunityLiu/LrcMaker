using System.Collections.Generic;

namespace Opportunity.LrcParser
{
    internal class LineComparer : Comparer<Line>
    {
        public static LineComparer Instance { get; } = new LineComparer();

        public override int Compare(Line x, Line y)
        {
            if (x is null) return y is null ? 0 : -1;
            if (y is null) return 1;
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }
}
