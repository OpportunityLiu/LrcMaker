using System;
using System.Collections.Generic;
using System.Text;

namespace Opportunity.LrcParser
{
    [System.Diagnostics.DebuggerDisplay(@"{ToString(),nq}")]
    public class Line
    {
        public Line() { }

        public Line(DateTime timestamp, string content)
        {
            Timestamp = timestamp;
            Content = content;
        }

        private static DateTime ONE_HOUR = new DateTime(1, 1, 1, 1, 0, 0);

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal DateTime InternalTimestamp;
        public DateTime Timestamp
        {
            get => this.InternalTimestamp;
            set
            {
                if (value.Kind != DateTimeKind.Unspecified)
                    throw new ArgumentException("Kind of value should be DateTimeKind.Unspecified");
                if (value.Year > 1000) //Auto correct.
                    value = new DateTime(1, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond);
                this.InternalTimestamp = value;
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal string InternalContent = "";
        public string Content { get => this.InternalContent; set => this.InternalContent = (value ?? "").Trim(); }

        internal StringBuilder ToString(StringBuilder sb)
        {
            var tts = this.InternalTimestamp;
            var ts = tts >= ONE_HOUR
                ? tts.ToString("HH:mm:ss.ff")
                : tts.ToString("mm:ss.ff");
            return sb.Append('[')
                .Append(ts)
                .Append(']')
                .AppendLine(this.InternalContent);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(this.InternalContent.Length + 10);
            ToString(sb);
            return sb.ToString();
        }
    }
}
