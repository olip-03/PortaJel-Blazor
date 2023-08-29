using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PortaJel_Blazor.Classes
{
    // https://media.olisshittyserver.xyz/api-docs/swagger/index.html
    public class ServerConnecter
    {
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public async Task<bool> AuthenticateAddressAsync(string address)
        {
            try
            {
                HttpClient client = new HttpClient();

                using HttpResponseMessage response = await client.GetAsync(address);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Address = address;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> AuthenticateUser(string username, string password)
        {
            if (Address == null)
            {
                return false;
            }

            using (HttpClient client = new HttpClient())
            {
                // Set the base URL of the JellyFin Server API
                client.BaseAddress = new Uri(Address);

                // Prepare the request body
                var requestBody = new
                {
                    Username = username,
                    Pw = password
                };

                try
                {
                    // Send the POST request to authenticate the user
                    HttpResponseMessage response = await client.PostAsJsonAsync("/Users/AuthenticateByName", requestBody);

                    // Check if the authentication was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Authentication successful
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occurred during the authentication process
                    Console.WriteLine($"An error occurred during authentication: {ex.Message}");
                }

                // Authentication failed
                return false;
            }
        }

    }
}
