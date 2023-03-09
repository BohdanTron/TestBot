using Newtonsoft.Json;
using System;

namespace EchoBot
{
    public class Message
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
