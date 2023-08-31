﻿using System;
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
        public async Task<BaseItemDtoQueryResult> FetchAlbumsAsync(int? _startIndex = null, int? _limit = null)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "DatePlayed" };

            BaseItemDtoQueryResult albumsResult = null;
            // Call GetItemsAsync with the specified parameters
            try
            {
                albumsResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, recursive: true, startIndex: _startIndex, limit: _limit, includeItemTypes: _includeItemTypes, sortBy: _sortTypes, enableImages: true) ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return albumsResult;
        }
        public async Task<Album[]> FetchRecentlyPlayedAsync(int? _startIndex = null, int? _limit = null)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "DatePlayed" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };
            List<ItemFilter> _itemFilter = new List<ItemFilter> { ItemFilter.IsPlayed };

            BaseItemDtoQueryResult songResult = null;
            // Call GetItemsAsync with the specified parameters
            try
            {
                // albumsResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, recursive: true, startIndex: _startIndex, limit: _limit, includeItemTypes: _includeItemTypes, sortBy: _sortTypes) ;

                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, filters: _itemFilter, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            List<Album> albums = new List<Album>();
            foreach (var item in songResult.Items)
            {
                try
                {
                    Album toAdd = new();
                    toAdd.name = item.Name;
                    toAdd.artist = item.Artists.FirstOrDefault();
                    toAdd.id = item.Id.ToString();
                    toAdd.songs = null; // TODO: Implement songs

                    //// Fetch image
                    //var imgFileResponse = await _imageClient.HeadItemImageByIndexAsync(itemId: item.Id, imageType: ImageType.Primary, imageIndex: 0, format: Jellyfin.Sdk.ImageFormat.Png);

                    //string mainDir = FileSystem.Current.AppDataDirectory;
                    //string fileName = toAdd.id + "-primaryimg.png";
                    //string filePath = System.IO.Path.Combine(mainDir, fileName);

                    //using (var fileStream = File.Create(filePath))
                    //{
                    //    await imgFileResponse.Stream.CopyToAsync(fileStream);
                    //    imgFileResponse.Stream.Close();
                    //}
                    //toAdd.imageSrc = imgFileResponse.Headers.First().Value.ToString(); //TODO: Fetch image source

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

