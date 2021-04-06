using System;
using System.Collections.Generic;

namespace AWS_S3_Test.Models
{
    public class JsonObject
    {
        public List<string> Items { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
    }
}