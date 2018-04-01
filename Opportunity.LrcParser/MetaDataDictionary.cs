﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Opportunity.LrcParser
{
    public class MetaDataDictionary : Dictionary<MetaDataType, object>
    {
        private object tryGet(MetaDataType key)
        {
            if (TryGetValue(key, out var r))
                return r;
            return null;
        }

        private static string prettify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value.Trim();
        }

        private void setString(MetaDataType key, string value)
        {
            value = prettify(value);
            if (value == default)
                this.Remove(key);
            else
                this[key] = value;
        }

        /// <summary>
        /// Lyrics artist.
        ///</summary>
        public string Artist
        {
            get => tryGet(MetaDataType.Artist) is string s ? s : "";
            set => setString(MetaDataType.Artist, value);
        }

        /// <summary>
        /// Album where the song is from.
        ///</summary>
        public string Album
        {
            get => tryGet(MetaDataType.Album) is string s ? s : "";
            set => setString(MetaDataType.Album, value);
        }

        /// <summary>
        /// Lyrics(song) title.
        ///</summary>
        public string Title
        {
            get => tryGet(MetaDataType.Title) is string s ? s : "";
            set => setString(MetaDataType.Title, value);
        }

        /// <summary>
        /// Creator of the songtext.
        ///</summary>
        public string Author
        {
            get => tryGet(MetaDataType.Author) is string s ? s : "";
            set => setString(MetaDataType.Author, value);
        }

        /// <summary>
        /// Creator of the LRC file.
        ///</summary>
        public string Creator
        {
            get => tryGet(MetaDataType.Creator) is string s ? s : "";
            set => setString(MetaDataType.Creator, value);
        }

        /// <summary>
        /// Overall timestamp adjustment.
        ///</summary>
        public TimeSpan Offset
        {
            get => tryGet(MetaDataType.Offset) is TimeSpan s ? s : default;
            set
            {
                if (value == default)
                    this.Remove(MetaDataType.Offset);
                else
                    this[MetaDataType.Offset] = value;
            }
        }

        /// <summary>
        /// The player or editor that created the LRC file.
        ///</summary>
        public string Editor
        {
            get => tryGet(MetaDataType.Editor) is string s ? s : "";
            set => setString(MetaDataType.Editor, value);
        }

        /// <summary>
        /// Version of program.
        ///</summary>
        public string Version
        {
            get => tryGet(MetaDataType.Version) is string s ? s : "";
            set => setString(MetaDataType.Version, value);
        }

        internal StringBuilder ToString(StringBuilder sb)
        {
            foreach (var item in this)
            {
                var v = item.Key.Stringify(item.Value);
                if (string.IsNullOrEmpty(v))
                    continue;
                sb.Append('[')
                    .Append(item.Key.Tag)
                    .Append(':')
                    .Append(v)
                    .Append(']')
                    .AppendLine();
            }
            return sb;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(this.Count * 10);
            ToString(sb);
            return sb.ToString();
        }
    }
}
