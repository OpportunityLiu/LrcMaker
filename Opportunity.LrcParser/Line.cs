using System;
using System.Collections.Generic;
using System.Text;

namespace Opportunity.LrcParser
{
    public class Line
    {
        public DateTime Timestamp { get; set; }

        public string Speaker { get; set; }

        public string Content { get; set; }
    }
}
