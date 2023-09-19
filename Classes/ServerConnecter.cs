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
using PortaJel_Blazor.Shared.Music_Displays;

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
        private SearchClient _searchClient;
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
                _searchClient = new(_sdkClientSettings, _httpClient);

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
        public async Task<Album[]> FetchRecentlyAddedAsync(int? _startIndex = null, int? _limit = null, bool? _isFavourite = false)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "DateCreated" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true, isFavorite: _isFavourite);
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
                    Album newAlbum = new();
                    newAlbum.name = item.Name;
                    newAlbum.id = item.Id;
                    newAlbum.songs = null; // TODO: Implement songs

                    if(item.Type != BaseItemKind.MusicAlbum)
                    {
                        newAlbum.isSong = true;
                        if (item.AlbumId != null)
                        {
                            newAlbum.id = (Guid)item.AlbumId;
                        }
                    }

                    // Fetch Artists
                    List<Artist> artists = new List<Artist>();
                    foreach (var artist in item.AlbumArtists)
                    {
                        Artist newArist = new Artist();
                        newArist.id = artist.Id.ToString();
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();

                    if (item.ImageBlurHashes.Primary != null && item.AlbumId != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else if (item.ImageBlurHashes.Primary != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.Id.ToString() + "/Images/Primary";
                    }
                    else
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }
                    newAlbum.lowResImageSrc = newAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

                    albums.Add(newAlbum);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album[]> FetchFavouritesAddedAsync(int? _startIndex = null, int? _limit = null)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum, BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "PlayCount" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true, isFavorite: true);
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
                    Album newAlbum = new();
                    newAlbum.name = item.Name;
                    newAlbum.id = item.Id;
                    newAlbum.songs = null; // TODO: Implement songs

                    if (item.Type != BaseItemKind.MusicAlbum)
                    {
                        newAlbum.isSong = true;
                        if (item.AlbumId != null)
                        {
                            newAlbum.id = (Guid)item.AlbumId;
                        }
                    }

                    // Fetch Artists
                    List<Artist> artists = new List<Artist>();
                    foreach (var artist in item.AlbumArtists)
                    {
                        Artist newArist = new Artist();
                        newArist.id = artist.Id.ToString();
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();

                    if (item.ImageBlurHashes.Primary != null && item.AlbumId != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else if(item.ImageBlurHashes.Primary != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.Id.ToString() + "/Images/Primary";
                    }
                    else
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }
                    newAlbum.lowResImageSrc = newAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

                    albums.Add(newAlbum);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album[]> FetchRecentlyPlayedAsync(int? _startIndex = null, int? _limit = null, bool? _isFavourite = false)
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
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, filters: _itemFilter, recursive: true, enableImages: true, isFavorite: _isFavourite);
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
                // Only collects songs, so we'll need to check to make sure we're only storing info on the album it's from.
                try
                {
                    // item.AlbumId = {395f708f-1c7a-cd4b-377d-5c13bd74bfa6}

                    Album newAlbum = new();
                    newAlbum.name = item.Album;
                    newAlbum.id = (Guid)item.AlbumId;
                    newAlbum.songs = null; // TODO: Implement songs

                    if (item.Type != BaseItemKind.MusicAlbum)
                    {
                        newAlbum.isSong = true;
                        if (item.AlbumId != null)
                        {
                            newAlbum.id = (Guid)item.AlbumId;
                        }
                    }

                    // Fetch Artists
                    List<Artist> artists = new List<Artist>();
                    foreach (var artist in item.AlbumArtists)
                    {
                        Artist newArist = new Artist();
                        newArist.id = artist.Id.ToString();
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();

                    if (item.ImageBlurHashes.Primary != null) 
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }
                    newAlbum.lowResImageSrc = newAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

                    albums.Sort();
                    if(albums.BinarySearch(newAlbum) < 0)
                    { // Check that item isnt in the list already
                        albums.Add(newAlbum);
                    }
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
                    Album newAlbum = new();
                    newAlbum.name = item.Name;
                    newAlbum.id = item.Id;
                    newAlbum.songs = null; // TODO: Implement songs

                    if (item.Type != BaseItemKind.MusicAlbum)
                    {
                        newAlbum.isSong = true;
                        if(item.AlbumId != null)
                        {
                            newAlbum.id = (Guid)item.AlbumId;
                        }
                    }

                    // Fetch Artists
                    List<Artist> artists = new List<Artist>();
                    foreach (var artist in item.AlbumArtists)
                    {
                        Artist newArist = new Artist();
                        newArist.id = artist.Id.ToString();
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();

                    if (item.ImageBlurHashes.Primary != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }
                    newAlbum.lowResImageSrc = newAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

                    albums.Add(newAlbum);
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
                    Album newAlbum = new();
                    newAlbum.name = item.Name;
                    newAlbum.id = item.Id;
                    newAlbum.songs = null; // TODO: Implement songs

                    if (item.Type != BaseItemKind.MusicAlbum)
                    {
                        newAlbum.isSong = true;
                        if (item.AlbumId != null)
                        {
                            newAlbum.id = (Guid)item.AlbumId;
                        }
                    }

                    // Fetch Artists
                    List<Artist> artists = new List<Artist>();
                    foreach (var artist in item.AlbumArtists)
                    {
                        Artist newArist = new Artist();
                        newArist.id = artist.Id.ToString();
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();

                    if (item.ImageTags.Count > 0)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + newAlbum.id + "/Images/Primary";
                    }
                    else if (item.ImageBlurHashes.Primary != null && item.AlbumId != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.AlbumId + "/Images/Primary";
                    }
                    else if(item.ArtistItems.First().Id != null)
                    {
                        newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + item.ArtistItems.First().Id + "/Images/Primary";
                    }
                    newAlbum.lowResImageSrc = newAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

                    albums.Add(newAlbum);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album> FetchAlbumByIDAsync(Guid albumId)
        {
            // SortBy: 'SortName',
            // SortOrder: 'Ascending',
            // IncludeItemTypes: 'MusicAlbum',
            // Recursive: true,
            // Fields: 'PrimaryImageAspectRatio,SortName,BasicSyncInfo',
            // ImageTypeLimit: 1,
            // EnableImageTypes: 'Primary,Backdrop,Banner,Thumb',
            // StartIndex: 0
            // parentId 

            Album getAlbum = new Album();

            List<BaseItemKind> _songItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<BaseItemKind> _albumItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };

            List<String> _sortTypes = new List<string> { "SortName" };
            List<Guid> _filterIds = new List<Guid> { albumId };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult albumResult;
            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                albumResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: _filterIds, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _albumItemTypes, recursive: true, enableImages: true);
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id,sortBy: _sortTypes ,sortOrder: _sortOrder, includeItemTypes: _songItemTypes, parentId: albumId, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            BaseItemDto albumResultItem = albumResult.Items.FirstOrDefault();
            // Set basic Info
            getAlbum.name = albumResultItem.Name;
            getAlbum.id = albumResultItem.Id; 
            getAlbum.isSong = false;

            // Set image
            if (albumResultItem.ImageBlurHashes.Primary != null && albumResultItem.AlbumId != null)
            {
                getAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + albumResultItem.AlbumId + "/Images/Primary";
            }
            else if (albumResultItem.ImageBlurHashes.Primary != null)
            {
                getAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + albumResultItem.Id.ToString() + "/Images/Primary";
            }
            else
            {
                getAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + albumResultItem.ArtistItems.First().Id + "/Images/Primary";
            }
            getAlbum.lowResImageSrc = getAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

            // Set Song information
            List<Song> songList = new List<Song>();
            foreach (var item in songResult.Items)
            {
                Song newSong = new Song();

                newSong.name = item.Name;
                newSong.artist = item.AlbumArtist;
                newSong.id = item.Id.ToString(); // TODO: Change to Guid type

                songList.Add(newSong);
            }
            getAlbum.songs = songList.ToArray();

            // Set Artist information
            List<Artist> artistList = new List<Artist>();
            foreach (var artist in albumResultItem.AlbumArtists)
            {
                Artist newArtists = new Artist();
                newArtists.id = artist.Id.ToString(); // TODO: Change to Guid type
                newArtists.name = artist.Name;
                artistList.Add(newArtists);
            }
            getAlbum.artists = artistList.ToArray();

            return getAlbum;
        }
        public async Task<Album[]> SearchAsync(string _searchTerm, bool? sorted = false)
        {
            if (String.IsNullOrWhiteSpace(_searchTerm))
            {
                return null;
            }
            List<Album> searchResuls = new List<Album>();

            List<BaseItemKind> _albumItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum, BaseItemKind.Audio, BaseItemKind.MusicArtist };

            SearchHintResult searchResult;
            searchResult = await _searchClient.GetAsync(userId: userDto.Id, searchTerm: _searchTerm, limit: 50, includeItemTypes: _albumItemTypes);

            foreach (var item in searchResult.SearchHints)
            {
                try
                {
                    Album newAlbum = new();
                    newAlbum.name = item.Name;
                    newAlbum.id = item.Id;
                    newAlbum.songs = null; // TODO: Implement songs

                    // Fetch Artists
                    List<Artist> artists = new List<Artist>();
                    if(item.Artists != null)
                    {
                        foreach (var artist in item.Artists)
                        {
                            Artist newArist = new Artist();
                            newArist.name = artist;

                            artists.Add(newArist);
                        }
                        newAlbum.artists = artists.ToArray();
                    }

                    newAlbum.imageSrc = "https://media.olisshittyserver.xyz/Items/" + newAlbum.id + "/Images/Primary";

                    newAlbum.lowResImageSrc = newAlbum.imageSrc + "?fillHeight=128&fillWidth=128&quality=96";

                    searchResuls.Add(newAlbum);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            if (sorted == true)
            {
                searchResuls.Sort();
            }

            return searchResuls.ToArray();
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

