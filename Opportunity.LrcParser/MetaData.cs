using System.Text;

namespace Opportunity.LrcParser
{
    [System.Diagnostics.DebuggerDisplay(@"{ToString(),nq}")]
    public class MetaData
    {
        public MetaData(MetaDataType title)
            : this(title, null) { }

        public MetaData(MetaDataType title, string content)
        {
            this.Title = title ?? throw new System.ArgumentNullException(nameof(title));
            this.Content = content;
        }

        public MetaData(string title)
            : this(title, null) { }

        public MetaData(string title, string content)
        {
            this.Title = MetaDataType.Create(title);
            this.Content = content;
        }

        public MetaDataType Title { get; }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string content;
        public string Content
        {
            get => this.content;
            set
            {
                value = value ?? "";
                value = value.Trim();
                Title.Validate(value);
                this.content = value;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => "[" + Title.Tag + ":" + this.content + "]";
    }
}
