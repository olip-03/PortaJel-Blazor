using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
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
        private ArtistsClient _artistsClient;
        private ItemsClient _itemsClient;
        private PlaylistsClient _playlistsClient;
        private PlaylistCreationResult _playlistCreationResult;
        private ImageClient _imageClient;
        private MusicGenresClient _genresClient;
        private SearchClient _searchClient;

        private ISystemClient _systemClient;
        private IUserClient _userClient;

        private IUserViewsClient _userViewsClient;

        private string Username = null;
        private string StoredPassword = null;

        private int TotalAlbumRecordCount = -1;
        private int TotalArtistRecordCount = -1;
        private int TotalSongRecordCount = -1;
        private int TotalGenreRecordCount = -1;

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
                _artistsClient = new(_sdkClientSettings, _httpClient);
                _genresClient = new(_sdkClientSettings, _httpClient);
                _playlistsClient = new(_sdkClientSettings, _httpClient);
                _playlistCreationResult = new();

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
        public async Task<Album[]> FetchRecentlyAddedAsync(int? _startIndex = null, int? _limit = null, bool? _isFavourite = false, bool? fetchFullArtist = false)
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
                    if(fetchFullArtist == true)
                    {
                        Album newItem = await AlbumBuilder(item, true);
                        albums.Add(newItem);
                    }
                    else
                    {
                        Album newItem = AlbumBuilder(item);
                        albums.Add(newItem);
                    }
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
                    albums.Add(AlbumBuilder(item));
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

                    Album newAlbum = AlbumBuilder(item);
                    if(item.Album != null) { newAlbum.name = item.Album.ToString(); }
                    newAlbum.sortMethod = Album.AlbumSortMethod.id;


                    albums.Sort();
                    if (albums.BinarySearch(newAlbum) < 0)
                    { // Check that item isnt in the list already
                        albums.Add(newAlbum); // TODO: Fix this shit I'm 99% sure i've fucked everything up
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
                    albums.Add(AlbumBuilder(item));
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
                    albums.Add(AlbumBuilder(item));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<Album> FetchAlbumByIDAsync(Guid albumId, bool? fetchFullArtist = false)
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
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _songItemTypes, parentId: albumId, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            BaseItemDto albumResultItem = albumResult.Items.FirstOrDefault();
            Album getAlbum = null;

            if (fetchFullArtist == true)
            {
                getAlbum = await AlbumBuilder(albumResultItem, true);
            }
            else
            {
                getAlbum = AlbumBuilder(albumResultItem);
            }

            if (getAlbum == null)
            {
                return null;
            }

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
            if (fetchFullArtist == true)
            {
                List<Guid> ids = new List<Guid>();
                foreach (var artist in albumResultItem.AlbumArtists)
                {
                    Artist newArtists = new Artist();
                    newArtists.id = artist.Id;
                    newArtists.name = artist.Name;
                    artistList.Add(newArtists);
                }
            }
            else
            {
                foreach (var artist in albumResultItem.AlbumArtists)
                {
                    Artist newArtists = new Artist();
                    newArtists.id = artist.Id;
                    newArtists.name = artist.Name;
                    artistList.Add(newArtists);
                }
                getAlbum.artists = artistList.ToArray();
            }


            return getAlbum;
        }
        public async Task<Album[]> SearchAsync(string _searchTerm, bool? sorted = false, int? searchLimit = 50)
        {
            if (String.IsNullOrWhiteSpace(_searchTerm))
            {
                return null;
            }

            List<BaseItemKind> _albumItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum, BaseItemKind.Audio, BaseItemKind.MusicArtist };

            SearchHintResult searchResult;
            searchResult = await _searchClient.GetAsync(userId: userDto.Id, searchTerm: _searchTerm, limit: searchLimit, includeItemTypes: _albumItemTypes, includeArtists: true);

            if(searchResult.SearchHints.Count() == 0)
            {
                return new Album[0];
            }

            List<Guid> searchedIds = new List<Guid>();
            foreach (var item in searchResult.SearchHints)
            {
               searchedIds.Add(item.Id);
            }

            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult itemsResult;
            try
            {
                itemsResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: searchedIds, sortOrder: _sortOrder, includeItemTypes: _albumItemTypes, limit: searchLimit, recursive: true, enableImages: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            List<Album> albums = new List<Album>();
            if (searchResult == null)
            {
                return null;
            }
            foreach (var item in itemsResult.Items)
            {
                try
                {
                    Album toAdd = AlbumBuilder(item);   
                    if(item.Type == BaseItemKind.MusicArtist)
                    {
                        toAdd.isArtist = true;
                    }
                    albums.Add(toAdd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            if(sorted == true)
            {
                albums.Sort();
            }

            return albums.ToArray();
        }
        public async Task<int> GetTotalAlbumCount()
        {
            if(TotalAlbumRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
                BaseItemDtoQueryResult recordCount = new BaseItemDtoQueryResult();
                try
                {
                    recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true);
                }
                catch (Jellyfin.Sdk.ItemsException itemException)
                {
                    if (itemException.StatusCode == 401)
                    {
                        // UNAUTHORISED
                        // TODO: Add specific message for this error (what the fuck why are we getting this???)
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                return recordCount.TotalRecordCount;
            }
            return TotalAlbumRecordCount;
        }
        public async Task<Album[]> GetAlbumsAsync(int? limit = 50, int? startFromIndex = 0, bool? favourites = false)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {  
                if(favourites == true)
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, isFavorite: true, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true);
                }
                else
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true);
                }
                TotalAlbumRecordCount = songResult.TotalRecordCount;
            }
            catch (Jellyfin.Sdk.ItemsException itemException)
            {
                if(itemException.StatusCode == 401)
                {
                    // UNAUTHORISED
                    // TODO: Add specific message for this error (what the fuck why are we getting this???)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null) { return null; }
            if (songResult.Items == null) { return null; }

            List<Album> albums = new List<Album>();
            foreach (var item in songResult.Items)
            {
                try
                {
                    albums.Add(AlbumBuilder(item));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<int> GetTotalArtistCount()
        {
            if (TotalArtistRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicArtist };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true);

                return recordCount.TotalRecordCount;
            }
            return TotalArtistRecordCount;
        }
        public async Task<Album[]> GetAllArtistsAsync(int? limit = 50, int? startFromIndex = 0, bool? favourites = false)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicArtist };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                if (favourites == true)
                {
                    songResult = await _itemsClient.GetItemsAsync(isFavorite: true, userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true); ;
                }
                else
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true);
                }
                TotalArtistRecordCount = songResult.TotalRecordCount;
            }
            catch (Jellyfin.Sdk.ItemsException itemException)
            {
                if (itemException.StatusCode == 401)
                {
                    // UNAUTHORISED
                    // TODO: Add specific message for this error (what the fuck why are we getting this???)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            // Catch blocks
            if (songResult == null) { return null; }
            if (songResult.Items == null) { return null; }

            List<Album> artists = new List<Album>();
            foreach (var item in songResult.Items)
            {
                Album itemToAdd = AlbumBuilder(item);
                itemToAdd.isArtist = true;
                artists.Add(itemToAdd);
                Debug.WriteLine("yo");
            }

            return artists.ToArray();
        }
        public async Task<int> GetTotalSongCount()
        {
            if (TotalSongRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true);

                return recordCount.TotalRecordCount;
            }
            return TotalSongRecordCount;
        }
        public async Task<Album[]> GetAllSongsAsync(int? limit= 50, int? startFromIndex = 0, bool? favourites = false)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                if(favourites == true)
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, isFavorite: true, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true);
                }
                else
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true);
                }
                TotalSongRecordCount = songResult.TotalRecordCount;
            }
            catch (Jellyfin.Sdk.ItemsException itemException)
            {
                if (itemException.StatusCode == 401)
                {
                    // UNAUTHORISED
                    // TODO: Add specific message for this error (what the fuck why are we getting this???)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            // Catch blocks
            if (songResult == null) { return null; }
            if (songResult.Items == null) { return null; }

            List<Album> songs = new List<Album>();
            foreach (var item in songResult.Items)
            {
                Album itemToAdd = AlbumBuilder(item);
                songs.Add(itemToAdd);
            }

            return songs.ToArray();
        }
        public async Task<int> GetTotalGenreCount()
        {
            if (TotalGenreRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _genresClient.GetMusicGenresAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, enableImages: false, enableTotalRecordCount: true);

                return recordCount.TotalRecordCount;
            }
            return TotalGenreRecordCount;
        }
        public async Task<Album[]> GetAllGenresAsync(int? limit = 50, int? startFromIndex = 0)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _genresClient.GetMusicGenresAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, enableTotalRecordCount: true);
                TotalGenreRecordCount = songResult.TotalRecordCount;
            }
            catch (Jellyfin.Sdk.ItemsException itemException)
            {
                if (itemException.StatusCode == 401)
                {
                    // UNAUTHORISED
                    // TODO: Add specific message for this error (what the fuck why are we getting this???)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            // Catch blocks
            if (songResult == null) { return null; }
            if (songResult.Items == null) { return null; }

            List<Album> songs = new List<Album>();
            foreach (var item in songResult.Items)
            {
                Album itemToAdd = await GenreBuilder(item);
                songs.Add(itemToAdd);
            }

            return songs.ToArray();
        }
        public async Task<Artist> GetArtistAsync(Guid artistId)
        {
            List<BaseItemKind> _includeItemTypesArtist = new List<BaseItemKind> { BaseItemKind.MusicArtist };
            List<BaseItemKind> _includeItemTypesAlbums = new List<BaseItemKind> { BaseItemKind.MusicAlbum };

            List<Guid> _searchIds = new List<Guid> { artistId };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult artistInfo = new BaseItemDtoQueryResult();
            BaseItemDtoQueryResult albumResult = new BaseItemDtoQueryResult();

            // Call GetItemsAsync with the specified parameters
            try
            {
                artistInfo = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: _searchIds, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypesArtist, recursive: true, enableImages: true, enableTotalRecordCount: true);
                albumResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, artistIds: _searchIds, includeItemTypes: _includeItemTypesAlbums, recursive: true);
                // TotalArtistRecordCount = songResult.TotalRecordCount;
            }
            catch (Jellyfin.Sdk.ItemsException itemException)
            {
                if (itemException.StatusCode == 401)
                {
                    // UNAUTHORISED
                    // TODO: Add specific message for this error (what the fuck why are we getting this???)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            // Catch blocks
            if (artistInfo == null) { return null; }
            if (artistInfo.Items == null) { return null; }

            Artist returnArtist = new Artist();
            foreach (var item in artistInfo.Items)
            {
                returnArtist = ArtistBuilder(item);
                List<Album> albums = new List<Album>();
                foreach (var album in albumResult.Items)
                {
                    albums.Add(AlbumBuilder(album));
                }
                returnArtist.artistAlbums = albums.ToArray();
            }

            return returnArtist;
        }
        public async Task<Album[]> GetPlaylistAsycn(int? limit = 50, int? startFromIndex = 0, bool? favourites = false)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, isFavorite: favourites, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true);
                TotalAlbumRecordCount = songResult.TotalRecordCount;
            }
            catch (Jellyfin.Sdk.ItemsException itemException)
            {
                if (itemException.StatusCode == 401)
                {
                    // UNAUTHORISED
                    // TODO: Add specific message for this error (what the fuck why are we getting this???)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null) { return null; }
            if (songResult.Items == null) { return null; }

            List<Album> albums = new List<Album>();
            foreach (var item in songResult.Items)
            {
                try
                {
                    albums.Add(AlbumBuilder(item));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return albums.ToArray();
        }
        public async Task<string[]> GetBase64ImgAsync(Guid[] guids)
        {
            string[] images = new string[0];
            return images;
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
            if (userDto == null)
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
        public UserDto GetUserDto()
        {
            return userDto;
        }
        public ImageClient GetImageClient()
        {
            return _imageClient;
        }
        public ItemsClient GetItemsClient()
        {
            return _itemsClient;
        }
        public ArtistsClient GetArtistClient()
        {
            return _artistsClient;
        }
        private Album AlbumBuilder(BaseItemDto baseItem)
        {
            return AlbumBuilder(baseItem, false).Result;
        }
        private async Task<Album> AlbumBuilder(BaseItemDto baseItem, bool fetchFullArtists)
        {
            if(baseItem == null)
            {
                return null;
            }
            Album newAlbum = new();
            newAlbum.name = baseItem.Name;
            newAlbum.id = baseItem.Id;
            newAlbum.songs = null; // TODO: Implement songs

            if (baseItem.Type != BaseItemKind.MusicAlbum)
            {
                newAlbum.isSong = true;
                if (baseItem.AlbumId != null)
                {
                    newAlbum.id = (Guid)baseItem.AlbumId;
                }
            }

            // Artists
            if(baseItem.Type != BaseItemKind.MusicArtist) 
            {
                if (fetchFullArtists == true && baseItem.AlbumArtists.Count > 0)
                {
                    List<BaseItemKind> _includeItemTypesArtist = new List<BaseItemKind> { BaseItemKind.MusicArtist };
                    List<Guid> _searchIds = new List<Guid>();
                    List<String> _sortTypes = new List<string> { "SortName" };
                    List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };
                    BaseItemDtoQueryResult artistFetchRequest = new BaseItemDtoQueryResult();

                    foreach (var artist in baseItem.AlbumArtists)
                    {
                        _searchIds.Add(artist.Id);
                    }

                    artistFetchRequest = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: _searchIds, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypesArtist, recursive: true, enableImages: true, enableTotalRecordCount: true);

                    List<Artist> artists = new List<Artist>();
                    foreach (var artist in artistFetchRequest.Items)
                    {
                        Artist newArist = ArtistBuilder(artist);
                         artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();
                }
                else
                {
                    List<Artist> artists = new List<Artist>();

                    foreach (var artist in baseItem.AlbumArtists)
                    {
                        Artist newArist = new Artist();
                        newArist.id = artist.Id;
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();
                }
                // Fetch Artists
                // Do need to do another request here

            }

            // Favourite Info
            if(baseItem.UserData.IsFavorite)
            {
                newAlbum.isFavourite = true;
            }

            // 69c72555-b29b-443d-9a17-01d735bd6f9f
            // https://media.olisshittyserver.xyz/Items/cc890a31-1449-ec9c-b428-24ec98127fdb/Images/Primary
            try
            {
                if (baseItem.ImageBlurHashes.Primary != null && baseItem.AlbumId != null)
                {
                    newAlbum.imageSrc = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.AlbumId + "/Images/Primary?format=jpg";
                }
                else if (baseItem.ImageBlurHashes.Primary != null)
                {
                    newAlbum.imageSrc = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id.ToString() + "/Images/Primary?format=jpg";
                }
                else
                {
                    newAlbum.imageSrc = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.ArtistItems.First().Id + "/Images/Primary?format=jpg";
                }
                newAlbum.lowResImageSrc = newAlbum.imageSrc;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to assign image to item ID " + baseItem.Id);
            }

            return newAlbum;
        }
        private Artist ArtistBuilder(BaseItemDto baseItem)
        {
            if (baseItem == null)
            {
                return null;
            }
            Artist newArtist= new();
            newArtist.name = baseItem.Name;
            newArtist.id = baseItem.Id;

            // 69c72555-b29b-443d-9a17-01d735bd6f9f
            // https://media.olisshittyserver.xyz/Items/cc890a31-1449-ec9c-b428-24ec98127fdb/Images/Primary

            // Set primary image
            if (baseItem.ImageBlurHashes.Primary != null)
            {
                newArtist.imgSrc = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Primary?format=jpg";
            }
            else
            {
                newArtist.imgSrc = "/images/emptyAlbum.png";
            }

            // Set background
            if (baseItem.ImageBlurHashes.Backdrop != null)
            {
                newArtist.backgroundImgSrc = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Backdrop?format=jpg";
            }
            else if (baseItem.ImageBlurHashes.Primary != null)
            {
                newArtist.backgroundImgSrc = newArtist.imgSrc;
            }

            // Set logo
            if (baseItem.ImageBlurHashes.Logo != null)
            {
                newArtist.logoImgSrc = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Logo";
            }

            return newArtist;
        }
        private async Task<Album> GenreBuilder(BaseItemDto baseItem)
        {
            // TODO: This is just a rehash of the AlbumBuilder to get the page I needed
            // working really quick. Ideally this'd be redone. 
            if (baseItem == null)
            {
                return null;
            }
            Album newAlbum = new();
            newAlbum.name = baseItem.Name;
            newAlbum.id = baseItem.Id;
            newAlbum.songs = null; // TODO: Implement songs

            if (baseItem.Type != BaseItemKind.MusicAlbum)
            {
                newAlbum.isSong = true;
                if (baseItem.AlbumId != null)
                {
                    newAlbum.id = (Guid)baseItem.AlbumId;
                }
            }
            
            // TODO: Implement getting album images for each genre 
           
            return newAlbum;
        }

    }

}

