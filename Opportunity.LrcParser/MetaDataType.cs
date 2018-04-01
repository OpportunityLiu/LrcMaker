using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opportunity.LrcParser
{
    [System.Diagnostics.DebuggerDisplay(@"{Tag}")]
    public abstract class MetaDataType
    {
        private static char[] invalidChars = "]:".ToCharArray();

        protected MetaDataType(string tag, Type dataType)
            : this(tag, dataType, false) { }

        internal MetaDataType(string tag, Type dataType, bool isSafe)
        {
            if (!isSafe)
            {
                tag = checkTag(tag);
            }
            this.DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            this.Tag = tag;
        }

        private static string checkTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(tag);
            Helper.CheckString(nameof(tag), tag, invalidChars);
            tag = tag.Trim();
            return tag;
        }

        public string Tag { get; }

        public override string ToString() => Tag;

        protected internal abstract object Parse(string mataDataContent);

        protected internal virtual string Stringify(object mataDataContent) => (mataDataContent ?? "").ToString().Trim();

        public Type DataType { get; }

        public static MetaDataType Create(string tag)
        {
            tag = checkTag(tag);
            if (PreDefined.TryGetValue(tag, out var r))
                return r;
            return new NoValidateMetaDataType(tag);
        }

        public static MetaDataType Create<T>(string tag, Func<string, T> parser)
            => Create(tag, parser, null);

        public static MetaDataType Create<T>(string tag, Func<string, T> parser, Func<T, string> stringifier)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            tag = checkTag(tag);
            if (PreDefined.TryGetValue(tag, out var r))
                return r;
            return new DelegateMetaDataType<T>(tag, parser, stringifier);
        }

        private sealed class NoValidateMetaDataType : MetaDataType
        {
            public NoValidateMetaDataType(string tag)
                : base(tag, typeof(string), true) { }

            protected internal override object Parse(string mataDataContent) => (mataDataContent ?? "").Trim();
        }

        private sealed class DelegateMetaDataType<T> : MetaDataType
        {
            public DelegateMetaDataType(string tag, Func<string, T> parser, Func<T, string> stringifier)
                : base(tag, typeof(T), true)
            {
                this.parser = parser;
                this.stringifier = stringifier;
            }

            private readonly Func<string, T> parser;
            private readonly Func<T, string> stringifier;

            protected internal override object Parse(string mataDataContent) => this.parser((mataDataContent ?? "").Trim());

            protected internal override string Stringify(object mataDataContent)
            {
                if (mataDataContent is T data && this.stringifier is Func<T, string> func)
                    return base.Stringify(func(data));
                return base.Stringify(mataDataContent);
            }
        }

        #region Pre-defined
        /// <summary>
        /// Pre-defined <see cref="MetaDataType"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, MetaDataType> PreDefined { get; }
            = new ReadOnlyDictionary<string, MetaDataType>(new Dictionary<string, MetaDataType>(StringComparer.OrdinalIgnoreCase)
            {
                ["ar"] = new NoValidateMetaDataType("ar"),
                ["al"] = new NoValidateMetaDataType("al"),
                ["ti"] = new NoValidateMetaDataType("ti"),
                ["au"] = new NoValidateMetaDataType("au"),
                ["by"] = new NoValidateMetaDataType("by"),
                ["offset"] = new DelegateMetaDataType<TimeSpan>("offset", v => TimeSpan.FromTicks((long)(double.Parse(v, System.Globalization.NumberStyles.Any) * 10000)), ts => ts.TotalMilliseconds.ToString("+0.#;-0.#")),
                ["re"] = new NoValidateMetaDataType("re"),
                ["ve"] = new NoValidateMetaDataType("ve"),
            });

        /// <summary>
        /// Lyrics artist.
        ///</summary>
        public static MetaDataType Artist => PreDefined["ar"];
        /// <summary>
        /// Album where the song is from.
        ///</summary>
        public static MetaDataType Album => PreDefined["al"];
        /// <summary>
        /// Lyrics(song) title.
        ///</summary>
        public static MetaDataType Title => PreDefined["ti"];
        /// <summary>
        /// Creator of the songtext.
        ///</summary>
        public static MetaDataType Author => PreDefined["au"];
        /// <summary>
        /// Creator of the LRC file.
        ///</summary>
        public static MetaDataType Creator => PreDefined["by"];
        /// <summary>
        /// Overall timestamp adjustment.
        ///</summary>
        public static MetaDataType Offset => PreDefined["offset"];
        /// <summary>
        /// The player or editor that created the LRC file.
        ///</summary>
        public static MetaDataType Editor => PreDefined["re"];
        /// <summary>
        /// Version of program.
        ///</summary>
        public static MetaDataType Version => PreDefined["ve"];
        #endregion Pre-defined
    }
}
