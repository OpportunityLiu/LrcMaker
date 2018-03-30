using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opportunity.LrcParser
{
    public abstract class MetaDataType
    {
        private static char[] invalidChars = "]:".ToCharArray();

        protected MetaDataType(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(tag);
            Helper.CheckString(nameof(tag), tag, invalidChars);
            this.Tag = tag.Trim();
        }

        public string Tag { get; }

        protected internal abstract void Validate(string mataDataContent);

        public static MetaDataType Create(string tag)
            => Create(tag, null);

        public static MetaDataType Create(string tag, Action<string> validator)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(tag);
            tag = tag.Trim();
            if (PreDefined.TryGetValue(tag, out var r))
                return r;
            if (validator is null)
                return new NoValidateMetaDataType(tag);
            else
                return new DelegateMetaDataType(tag, validator);
        }

        private sealed class NoValidateMetaDataType : MetaDataType
        {
            public NoValidateMetaDataType(string tag)
                : base(tag) { }

            protected internal override void Validate(string mataDataContent) { }
        }

        private sealed class DelegateMetaDataType : MetaDataType
        {
            public DelegateMetaDataType(string tag, Action<string> validator)
                : base(tag)
            {
                this.validator = validator;
            }

            private readonly Action<string> validator;

            protected internal override void Validate(string mataDataContent) => this.validator(mataDataContent);
        }

        #region Pre-defined
        /// <summary>
        /// Pre-defined <see cref="MetaDataType"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, MetaDataType> PreDefined { get; }
            = new ReadOnlyDictionary<string, MetaDataType>(new Dictionary<string, MetaDataType>(StringComparer.OrdinalIgnoreCase)
            {
                ["Artist"] = Create("ar"),
                ["Album"] = Create("al"),
                ["Title"] = Create("ti"),
                ["Author"] = Create("au"),
                ["Creator"] = Create("by"),
                ["Offset"] = Create("offset", v => double.TryParse(v, out _)),
                ["Editor"] = Create("re"),
                ["Version"] = Create("ve"),
            });

        /// <summary>
        /// Lyrics artist.
        ///</summary>
        public static MetaDataType Artist => PreDefined["Artist"];
        /// <summary>
        /// Album where the song is from.
        ///</summary>
        public static MetaDataType Album => PreDefined["Album"];
        /// <summary>
        /// Lyrics(song) title.
        ///</summary>
        public static MetaDataType Title => PreDefined["Title"];
        /// <summary>
        /// Creator of the songtext.
        ///</summary>
        public static MetaDataType Author => PreDefined["Author"];
        /// <summary>
        /// Creator of the LRC file.
        ///</summary>
        public static MetaDataType Creator => PreDefined["Creator"];
        /// <summary>
        /// Overall timestamp adjustment.
        ///</summary>
        public static MetaDataType Offset => PreDefined["Offset"];
        /// <summary>
        /// The player or editor that created the LRC file.
        ///</summary>
        public static MetaDataType Editor => PreDefined["Editor"];
        /// <summary>
        /// Version of program.
        ///</summary>
        public static MetaDataType Version => PreDefined["Version"];
        #endregion Pre-defined
    }
}
