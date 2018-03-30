﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Opportunity.LrcParser
{
    internal class Parser
    {
        private static readonly char[] lineBreaks = "\r\n\u0085\u2028\u2029".ToCharArray();
        private readonly string data;

        private int currentPosition = 0;

        public readonly List<MetaData> MetaData = new List<MetaData>(4);
        public readonly List<Line> Lines = new List<Line>(25);

        public Parser(string data)
        {
            this.data = data ?? "";
        }

        private void skipWhitespaces()
        {
            for (; this.currentPosition < this.data.Length; this.currentPosition++)
            {
                if (!char.IsWhiteSpace(this.data[this.currentPosition]))
                {
                    break;
                }
            }
        }

        private int readLine()
        {
            if (this.currentPosition >= this.data.Length)
                return -1;
            this.currentPosition = this.data.IndexOf('[', this.currentPosition);
            if (this.currentPosition < 0)
            {
                this.currentPosition = this.data.Length;
                return -1;
            }
            var nextPosition = this.currentPosition + 1;
            if (nextPosition >= this.data.Length)
                return nextPosition;

            nextPosition = this.data.IndexOfAny(lineBreaks, nextPosition);
            if (nextPosition < 0)
            {
                return this.data.Length;
            }

            nextPosition = this.data.IndexOf('[', nextPosition);
            if (nextPosition < 0)
            {
                return this.data.Length;
            }
            return nextPosition;
        }

        public IEnumerable<string> ReadLines()
        {
            while (true)
            {
                var nextPosition = readLine();
                if (nextPosition < 0)
                    yield break;
                yield return this.data.Substring(this.currentPosition, nextPosition - this.currentPosition);
                this.currentPosition = nextPosition;
            }
        }

        private int readTag(int next, out int tagStart, out int tagEnd)
        {
            tagStart = -1;
            tagEnd = -1;
            for (var i = this.currentPosition + 1; i < next; i++)
            {
                if (!char.IsWhiteSpace(this.data[i]))
                {
                    tagStart = i;
                    break;
                }
            }
            if (tagStart < 0)
            {
                return next;
            }
            var end = 0;
            if (char.IsDigit(this.data[tagStart]))
            {
                // timestamp
                end = this.data.IndexOf(']', tagStart, next - tagStart) + 1;
            }
            else
            {
                // ID tag
                end = this.data.LastIndexOf(']', next - 1, next - tagStart) + 1;
            }
            if (end <= 0)
                return next;

            if (!char.IsWhiteSpace(this.data[end - 1]))
            {
                tagEnd = end - 1;
            }
            else
            {
                for (var i = end - 1; i >= tagStart; i--)
                {
                    if (!char.IsWhiteSpace(this.data[i]))
                    {
                        tagEnd = i + 1;
                        break;
                    }
                }
            }
            return end;
        }

        private static string[] timestampFormats = new[]
        {
            "m:s.f",
            "m:s.ff",
            "m:s.fff",
            "m:s.ffff",
            "m:s.fffff",
            "m:s.ffffff",
            "h:m:s.f",
            "h:m:s.ff",
            "h:m:s.fff",
            "h:m:s.ffff",
            "h:m:s.fffff",
            "h:m:s.ffffff",
        };

        private void analyzeLine(int next)
        {
            var current = this.currentPosition;
            var lineStart = this.Lines.Count;
            while (true)
            {
                var tagNext = readTag(next, out var tagStart, out var tagEnd);
                if (tagEnd < 0 || tagStart < 0)
                    break;
                this.currentPosition = tagNext;
                if (!char.IsDigit(this.data[tagStart]))
                {
                    var colum = this.data.IndexOf(':', tagStart, tagEnd - tagStart);
                    try
                    {
                        if (colum < 0)
                            this.MetaData.Add(new MetaData(MetaDataType.Create(this.data.Substring(tagStart, tagEnd - tagStart))));
                        else
                            this.MetaData.Add(new MetaData(MetaDataType.Create(this.data.Substring(tagStart, colum - tagStart)),
                                this.data.Substring(colum + 1, tagEnd - colum - 1)));
                    }
                    catch { }
                }
                else
                {
                    var timestamp = this.data.Substring(tagStart, tagEnd - tagStart);
                    if (DateTime.TryParseExact(timestamp, timestampFormats, null, System.Globalization.DateTimeStyles.NoCurrentDateDefault | System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var time))
                        this.Lines.Add(new Line { InternalTimestamp = time });
                }
            }
            if (this.Lines.Count != lineStart)
            {
                skipWhitespaces();
                var end = next;
                for (var i = next - 1; i >= this.currentPosition; i--)
                {
                    if (!char.IsWhiteSpace(this.data[i]))
                    {
                        end = i + 1;
                        break;
                    }
                }
                var content = this.data.Substring(this.currentPosition, end - this.currentPosition);
                for (var i = lineStart; i < this.Lines.Count; i++)
                {
                    this.Lines[i].InternalContent = content;
                }
            }
            this.currentPosition = next;
        }

        public void Analyze()
        {
            while (true)
            {
                var nextPosition = readLine();
                if (nextPosition < 0)
                    return;
                analyzeLine(nextPosition);
            }
        }
    }
}
