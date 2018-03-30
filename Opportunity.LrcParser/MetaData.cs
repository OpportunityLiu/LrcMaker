using System.Text;

namespace Opportunity.LrcParser
{
    public class MetaData
    {
        private static char[] invalidContentChars = "]".ToCharArray();

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

        private string content;
        public string Content
        {
            get => this.content;
            set
            {
                value = value ?? "";
                Helper.CheckString(nameof(value), value, invalidContentChars);
                value = value.Trim();
                Title.Validate(value);
                this.content = value;
            }
        }
    }
}
