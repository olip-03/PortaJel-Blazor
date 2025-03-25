using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Portajel.Connections.Data.Radio.Search
{
    public class RadioSearchResult
    {
        [JsonPropertyName("apiVersion")]
        public int ApiVersion { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("took")]
        public int Took { get; set; }

        [JsonPropertyName("hits")]
        public RadioSearchHits Hits { get; set; }
    }
}
