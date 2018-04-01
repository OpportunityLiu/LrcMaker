using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Opportunity.LrcParser
{
    public class LineCollection : List<Line>
    {
        internal LineCollection() : base(25) { }

        public void SortByTimestamp() => Sort(LineComparer.Instance);

        public void ApplyOffset(TimeSpan offset)
        {
            if (offset == default)
                return;
            var i = 0;
            try
            {
                for (; i < this.Count; i++)
                {
                    var line = this[i];
                    line.InternalTimestamp += offset;
                }
            }
            catch
            {
                for (var j = 0; j < i; j++)
                {
                    var line = this[j];
                    line.InternalTimestamp -= offset;
                }
                throw;
            }
        }

        internal StringBuilder ToString(StringBuilder sb)
        {
            foreach (var item in this)
            {
                item.ToString(sb);
            }
            return sb;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(this.Count * 20);
            ToString(sb);
            return sb.ToString();
        }
    }
}
