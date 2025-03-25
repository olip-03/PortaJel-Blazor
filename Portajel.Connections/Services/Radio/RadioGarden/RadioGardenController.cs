using Portajel.Connections.Data.Radio.Search;
using Portajel.Connections.Interfaces;
using Portajel.Structures.Interfaces.Radio;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Portajel.Connections.Services.Radio.RadioGarden
{
    // https://jonasrmichel.github.io/radio-garden-openapi/
    public class RadioGardenController : IRadioController
    {
        private HttpClient _httpClient;

        public IRadioPlacesController Places => throw new NotImplementedException();

        public RadioGardenController(HttpClient? httpClient = null)
        {
            if(httpClient != null)
            {
                _httpClient = httpClient;
            }
            else
            {
                _httpClient = new HttpClient();
            }
        }

        public async Task<RadioSearchResult?> SearchAsync(string query)
        {
            string encodedQuery = HttpUtility.UrlEncode(query);
            var response = await _httpClient.GetAsync($"http://radio.garden/api/search?q={encodedQuery}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<RadioSearchResult>(content, options);
        }
    }
}
