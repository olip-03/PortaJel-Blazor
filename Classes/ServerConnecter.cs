using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PortaJel_Blazor.Data;

namespace PortaJel_Blazor.Classes
{
    // https://media.olisshittyserver.xyz/api-docs/swagger/index.html
    // {69c72555-b29b-443d-9a17-01d735bd6f9f} UID
    // /Artists to get all artists 
    // 
    // /Users/{userId}/Items/Latest endpoint to fetch MOST RECENT media added to the server
    public class ServerConnecter
    {
        private UserDto userDto = null!;
        private SdkClientSettings _sdkClientSettings;
        private ItemsClient _itemsClient;
        private ImageClient _imageClient;
        private ISystemClient _systemClient;
        private IUserClient _userClient;
       
        private IUserViewsClient _userViewsClient;

        private string Username = null;
        private string StoredPassword = null;

        HttpClient _httpClient;
        public ServerConnecter()
        {
            _sdkClientSettings = new();
            _httpClient = new HttpClient();
        }
        public ServerConnecter(string deviceName, string deviceId, string clientName, string clientVersion)
        {
            _sdkClientSettings = new();
            _httpClient = new HttpClient();

            _sdkClientSettings.DeviceName = deviceName;
            _sdkClientSettings.DeviceId = deviceId;
            _sdkClientSettings.ClientName = clientName;
            _sdkClientSettings.ClientVersion = clientVersion;
        }

        public async Task<bool> AuthenticateAddressAsync(string address)
        {
            _sdkClientSettings.BaseUrl = address;

            _systemClient = new SystemClient(_sdkClientSettings, _httpClient);
            try
            {
                // Get public system info to verify that the url points to a Jellyfin server.
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> AuthenticateAddressAsync()
        {
            return await AuthenticateAddressAsync(_sdkClientSettings.BaseUrl);
        }
        public async Task<bool> AuthenticateUser(string username, string password)
        {
            if (_systemClient == null)
            {
                return false;
            }

            try
            {
                // Get public system info to verify that the url points to a Jellyfin server.
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }

            var validUser = false;
            _userClient = new UserClient(_sdkClientSettings, _httpClient);
            try
            {
                // Authenticate user.
                var authenticationResult = await _userClient.AuthenticateUserByNameAsync(new AuthenticateUserByName
                {
                    Username = username,
                    Pw = password
                }).ConfigureAwait(false);

                _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
                userDto = authenticationResult.User;

                _itemsClient = new(_sdkClientSettings, _httpClient);
                _imageClient = new(_sdkClientSettings, _httpClient);

                Username = username;
                StoredPassword = password;
                validUser = true;
            }
            catch (Exception)
            {
                return false;
            }

            return validUser;
        }
        public async Task<bool> AuthenticateUser()
        {
            return await AuthenticateUser(Username, StoredPassword);
        }
        public async Task<Album[]> FetchRecentlyAddedAsync(int? _startIndex = null, int? _limit = null)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "DateCreated" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            List<Album> albums = new List<Album>();
            if (songResult == null)
            {
                return null;
            }
            foreach (var item in songResult.Items)
            {
                try
                {
                    Album toAdd = new();
                    toAdd.name = item.Name;
                    toAdd.artist = item.Artists.FirstOrDefault();
                    toAdd.id = item.Id.ToString();
                    toAdd.songs = null; // TODO: Implement songs
                    if (item.ImageBlurHashes.Primary != null && item.AlbumId != null)
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else if(item.ImageBlurHashes.Primary != null)
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.Id.ToString() + "/Images/Primary";
                    }
                    else
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }

                    albums.Add(toAdd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album[]> FetchRecentlyPlayedAsync(int? _startIndex = null, int? _limit = null)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "DatePlayed" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };
            List<ItemFilter> _itemFilter = new List<ItemFilter> { ItemFilter.IsPlayed };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, filters: _itemFilter, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            List<Album> albums = new List<Album>();
            if (songResult == null)
            {
                return null;
            }
            foreach (var item in songResult.Items)
            {
                try
                {
                    Album toAdd = new();
                    toAdd.name = item.Name;
                    toAdd.artist = item.Artists.FirstOrDefault();
                    toAdd.id = item.Id.ToString();
                    toAdd.songs = null; // TODO: Implement songs
                    if(item.ImageBlurHashes.Primary != null) 
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }

                    albums.Add(toAdd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album[]> FetchMostPlayedAsync(int? _startIndex = null, int? _limit = null)
        {
            //SortBy: 'PlayCount',
            //SortOrder: 'Descending',
            //IncludeItemTypes: 'Audio',
            //Limit: itemsPerRow(),
            //Recursive: true,
            //Fields: 'PrimaryImageAspectRatio,AudioInfo',
            //Filters: 'IsPlayed',
            //ParentId: parentId,
            //ImageTypeLimit: 1,
            //EnableImageTypes: 'Primary,Backdrop,Banner,Thumb',
            //EnableTotalRecordCount: false
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "PlayCount" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };
            List<ItemFilter> _itemFilter = new List<ItemFilter> { ItemFilter.IsPlayed };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, filters: _itemFilter, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            List<Album> albums = new List<Album>();
            if (songResult == null)
            {
                return null;
            }
            foreach (var item in songResult.Items)
            {
                try
                {
                    Album toAdd = new();
                    toAdd.name = item.Name;
                    toAdd.artist = item.Artists.FirstOrDefault();
                    toAdd.id = item.Id.ToString();
                    toAdd.songs = null; // TODO: Implement songs
                    if (item.ImageBlurHashes.Primary != null)
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }

                    albums.Add(toAdd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album[]> FetchRandomAsync(int? _startIndex = null, int? _limit = null)
        {
            //SortBy: 'PlayCount',
            //SortOrder: 'Descending',
            //IncludeItemTypes: 'Audio',
            //Limit: itemsPerRow(),
            //Recursive: true,
            //Fields: 'PrimaryImageAspectRatio,AudioInfo',
            //Filters: 'IsPlayed',
            //ParentId: parentId,
            //ImageTypeLimit: 1,
            //EnableImageTypes: 'Primary,Backdrop,Banner,Thumb',
            //EnableTotalRecordCount: false
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "Random" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            List<Album> albums = new List<Album>();
            if (songResult == null)
            {
                return null;
            }
            foreach (var item in songResult.Items)
            {
                try
                {
                    Album toAdd = new();
                    toAdd.name = item.Name;
                    toAdd.artist = item.Artists.FirstOrDefault();
                    toAdd.id = item.Id.ToString();
                    toAdd.songs = null; // TODO: Implement songs

                    if(item.ImageTags.Count > 0)
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + toAdd.id + "/Images/Primary";
                    }
                    else if (item.ImageBlurHashes.Primary != null && item.AlbumId != null)
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else if(item.ArtistItems.First().Id != null)
                    {
                        toAdd.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }


                    albums.Add(toAdd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }

        public void SetBaseAddress(string url)
        {
            _sdkClientSettings.BaseUrl = url;
        }
        public void SetUserDetails(string username, string password)
        {
            Username = username;
            StoredPassword = password;
        }
        public string GetProfileImage()
        {
            if(userDto == null)
            {
                return null;
            }
            return userDto.PrimaryImageTag;
        }
        public string GetUsername()
        {
            return Username;
        }
        public string GetPassword()
        {
            return StoredPassword;
        }
        public string GetBaseAddress()
        {
            return _sdkClientSettings.BaseUrl;
        }
    }

}

