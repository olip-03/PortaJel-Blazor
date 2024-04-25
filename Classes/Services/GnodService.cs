using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes.Services
{
    public class GnodService
    {
        public async Task<string[]> QueryGnod(string query)
        {
            List<string> toReturn = new();
            try
            {
                HttpClient httpClient = new HttpClient();

                string url = "https://www.music-map.com/map-search.php?f=" + query.Trim().Replace(' ', '+');

                string data = await httpClient.GetStringAsync(url);

                // Initialize the parser
                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(data);

                // Select the div with id 'gnodMap'
                var gnodMapDiv = document.QuerySelector("#gnodMap");

                // Iterate over the <a> elements within the selected div
                if(gnodMapDiv != null)
                {
                    foreach (var aElement in gnodMapDiv.QuerySelectorAll("a"))
                    {
                        toReturn.Add(aElement.TextContent);
                    }
                }

            }
            catch 
            {
                // Do nun lil bro
            }

            return toReturn.ToArray();
        }
    }
}
