using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Portajel.Connections.Data.Radio.Search
{
    public class RadioSearchHit
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("_score")]
        public double Score { get; set; }

        [JsonPropertyName("_source")]
        public RadioSearchSource Source { get; set; }
    }
}
