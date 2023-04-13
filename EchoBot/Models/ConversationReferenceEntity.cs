using Azure;
using Azure.Data.Tables;
using System;

namespace EchoBot.Models
{
    public class ConversationReferenceEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ConversationReference { get; set; }
    }
}
