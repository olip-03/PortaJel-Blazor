using Jellyfin.Sdk;
using Newtonsoft.Json.Linq;
using PortaJel_Blazor.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PortaJel_Blazor.Classes
{
    // https://media.olisshittyserver.xyz/api-docs/swagger/index.html
    // {69c72555-b29b-443d-9a17-01d735bd6f9f} UID
    // /Artists to get all artists 
    // 
    // /Users/{userId}/Items/Latest endpoint to fetch MOST RECENT media added to the server
    public class ServerConnecter
    {
        private UserDto? userDto = null;

        private SdkClientSettings _sdkClientSettings = new();
        private ArtistsClient? _artistsClient = null;
        private ItemsClient? _itemsClient = null;
        private ItemLookupClient? _itemLookupClient = null;
        private ItemUpdateClient? _itemUpdateClient = null;
        private PlaylistsClient? _playlistsClient = null;
        private PlaylistCreationResult? _playlistCreationResult = null;
        private MediaInfoClient? _mediaInfoClient = null;
        private UserLibraryClient? _userLibraryClient = null;
        private AudioClient? _audioClient = null;
        private ImageClient? _imageClient = null;
        private MusicGenresClient? _genresClient = null;
        private SearchClient? _searchClient = null;

        private ISystemClient? _systemClient = null;
        private IUserClient? _userClient = null;

        private string Username = String.Empty;
        private string StoredPassword = String.Empty;

        private ConcurrentDictionary<Guid, Album> albumCache = new();
        private ConcurrentDictionary<Guid, Song> songCache = new();
        public ConcurrentDictionary<Guid, Artist> artistCache = new();
        public ConcurrentDictionary<Guid, Playlist> playlistCache = new();
        public ConcurrentDictionary<Guid, Genre> genreCache = new();

        private int TotalAlbumRecordCount = -1;
        private int TotalPlaylistRecordCount = -1;
        private int TotalArtistRecordCount = -1;
        private int TotalSongRecordCount = -1;
        private int TotalGenreRecordCount = -1;

        public bool isOffline { get; private set; } = false;
        public bool aggressiveCacheRefresh { get; private set; } = false;

        public CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private CancellationToken token;

        HttpClient _httpClient = new();
        public ServerConnecter()
        {
            _sdkClientSettings = new();
            _httpClient = new HttpClient();

            _sdkClientSettings.DeviceName = Microsoft.Maui.Devices.DeviceInfo.Current.Name;
            _sdkClientSettings.DeviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            _sdkClientSettings.ClientName = MauiProgram.applicationName;
            _sdkClientSettings.ClientVersion = MauiProgram.applicationClientVersion;

            token = cancelTokenSource.Token;

            _systemClient = new SystemClient(_sdkClientSettings, _httpClient);
            _userClient = new UserClient(_sdkClientSettings, _httpClient);
            _itemsClient = new(_sdkClientSettings, _httpClient);
            _imageClient = new(_sdkClientSettings, _httpClient);
            _searchClient = new(_sdkClientSettings, _httpClient);
            _artistsClient = new(_sdkClientSettings, _httpClient);
            _genresClient = new(_sdkClientSettings, _httpClient);
            _playlistsClient = new(_sdkClientSettings, _httpClient);
            _userLibraryClient = new(_sdkClientSettings, _httpClient);
            _mediaInfoClient = new(_sdkClientSettings, _httpClient);
            _itemUpdateClient = new(_sdkClientSettings, _httpClient);
            _itemLookupClient = new(_sdkClientSettings, _httpClient);
            _audioClient = new(_sdkClientSettings, _httpClient);
        }

        #region Authentication
        /// <summary>
        /// Asynchronously authenticates the provided address by setting it as the base URL, 
        /// initializing a SystemClient instance, and attempting to retrieve public system 
        /// information to verify that the URL points to a Jellyfin server.
        /// </summary>
        /// <param name="address">The address to authenticate.</param>
        /// <returns>A task representing the asynchronous operation. The task result is true if 
        /// authentication is successful, otherwise false.</returns>
        public async Task<bool> AuthenticateAddressAsync(string address)
        {
            if (_systemClient == null)
            {
                return false;
            }

            try
            {
                _sdkClientSettings.BaseUrl = address;
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync(token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    cancelTokenSource = new();
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously authenticates the base URL set in the SDK client settings by initializing 
        /// a SystemClient instance and attempting to retrieve public system information to verify 
        /// that the URL points to a Jellyfin server.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is true if authentication 
        /// is successful, otherwise false.
        /// </returns>
        /// 
        public async Task<bool> AuthenticateAddressAsync()
        {
            return await AuthenticateAddressAsync(_sdkClientSettings.BaseUrl);
        }

        /// <summary>
        /// Asynchronously authenticates a user with the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user to authenticate.</param>
        /// <param name="password">The password of the user to authenticate.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is true if authentication 
        /// is successful, otherwise false.
        /// </returns>
        /// 
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            if (_systemClient == null || _userClient == null)
            {
                return false;
            }

            try
            {
                // Authenticate user.
                AuthenticationResult authenticationResult = await _userClient.AuthenticateUserByNameAsync(new AuthenticateUserByName
                {
                    Username = username,
                    Pw = password,
                }, token);

                _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
                userDto = authenticationResult.User;

                Username = username;
                StoredPassword = password;
                isOffline = false;
            }
            catch (Exception)
            {
                isOffline = true;
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Asynchronously authenticates a user with the stored username and password.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is true if authentication 
        /// is successful, otherwise false.
        /// </returns>
        /// 
        public async Task<bool> AuthenticateUserAsync()
        {
            return await AuthenticateUserAsync(Username, StoredPassword);
        }
        #endregion

        #region Albums
        /// <summary>
        /// Retrieves all albums based on the specified parameters asynchronously.
        /// </summary>
        /// <param name="setLimit">The maximum number of albums to retrieve.</param>
        /// <param name="setStartIndex">The index from which to start retrieving albums.</param>
        /// <param name="setFavourites">A boolean value indicating whether to retrieve only favorite albums.</param>
        /// <param name="setSortTypes">Optional. Specify one or more sort orders, comma delimited. Options: Album, AlbumArtist, Artist, Budget, CommunityRating, CriticRating, DateCreated, DatePlayed, PlayCount, PremiereDate, ProductionYear, SortName, Random, Revenue, Runtime.</param>
        /// <param name="setSortOrder">The sort order for the albums.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is an array of albums 
        /// matching the specified criteria.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the server connector has not been initialized. Ensure that 
        /// AuthenticateUserAsync method has been called.
        /// </exception>
        public async Task<Album[]> GetAllAlbumsAsync(int? setLimit = null, int? setStartIndex = 0, bool? setFavourites = false, IEnumerable<String>? setSortTypes = null, SortOrder setSortOrder = SortOrder.Descending)
        {
            if (_itemsClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }


            List<Album> toReturn = new();
            // Function to run that returns data from the cache
            void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                List<Album> filteredCache = albumCache.Values.ToList();

                // Apply filters based on parameters
                if (setFavourites == true)
                {
                    filteredCache = filteredCache.Where(album => album.isFavourite == setFavourites.Value).ToList();
                }

                // Apply sorting if sort types are providedObject reference not set to an instance of an object.'
                if (setSortTypes != null && setSortTypes.Any())
                {
                    foreach (var sortType in setSortTypes)
                    {
                        switch (sortType)
                        {
                            // Implement sorting logic based on sort types
                            // For example, sorting by album name
                            case "Name":
                                filteredCache = setSortOrder == SortOrder.Ascending ?
                                    filteredCache.OrderBy(album => album.name).ToList() :
                                    filteredCache.OrderByDescending(album => album.name).ToList();
                                break;
                                // Add cases for other sort types if needed
                        }
                    }
                }

                // Apply count limits
                if (setStartIndex.HasValue)
                {
                    filteredCache = filteredCache.Skip(setStartIndex.Value).ToList();
                }
                if (setLimit.HasValue)
                {
                    filteredCache = filteredCache.Take(setLimit.Value).ToList();
                }

                // Update toReturn with filtered and sorted cache items
                toReturn.AddRange(filteredCache);
            }

            if(setSortTypes == null)
            {
                setSortTypes = new List<String> { "Name" };
            }

            if (isOffline)
            { // If offline, return all from the cache
                ReturnFromCache();
            }
            else
            { // Call server
                try
                {
                    // Make server call
                    BaseItemDtoQueryResult serverResults = await _itemsClient.GetItemsAsync(
                        userId: userDto.Id,
                        isFavorite: setFavourites,
                        sortBy: setSortTypes,
                        sortOrder: new List<SortOrder> { setSortOrder },
                        includeItemTypes: new List<BaseItemKind> { BaseItemKind.MusicAlbum },
                        limit: setLimit,
                        startIndex: setStartIndex,
                        recursive: true,
                        enableImages: true,
                        enableTotalRecordCount: true);
                    for (int i = 0; i < serverResults.Items.Count(); i++)
                    {
                        Album newAlbum = AlbumBuilder(serverResults.Items[i]);
                        toReturn.Add(newAlbum);
                        if (!albumCache.TryAdd(newAlbum.id, newAlbum))
                        { // Add to cache
                            albumCache[newAlbum.id] = newAlbum;
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    isOffline = true;
                    ReturnFromCache();
                }
            }

            return toReturn.ToArray();
        }

        /// <summary>
        /// Asynchronously retrieves an album from the server.
        /// </summary>
        /// <param name="setId">The optional ID of the album to retrieve. If null, returns an empty album.</param>
        /// <returns>Returns the retrieved album asynchronously.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the server connector has not been initialized. Ensure that AuthenticateUserAsync has been called.</exception>
        public async Task<Album> GetAlbumAsync(Guid setId)
        {
            Album toReturn = Album.Empty;
            if(_itemsClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                Album? fromCache = null;
                albumCache.TryGetValue(setId, out fromCache);
                if(fromCache != null)
                {
                    toReturn = fromCache;
                }
            }

            if (isOffline)
            {
                ReturnFromCache();
            }
            else
            {
                try
                {
                    Task<BaseItemDtoQueryResult> albumResult = _itemsClient.GetItemsAsync(
                        userId: userDto.Id,
                        ids: new List<Guid> { setId },
                        includeItemTypes: new List<BaseItemKind> { BaseItemKind.MusicAlbum },
                        recursive: true,
                        enableImages: true,
                        cancellationToken: token);
                    Task<BaseItemDtoQueryResult> songResult = _itemsClient.GetItemsAsync(
                        userId: userDto.Id,
                        includeItemTypes: new List<BaseItemKind> { BaseItemKind.Audio },
                        sortBy: new List<String> { "Album", "SortName" },
                        fields: new List<ItemFields> { ItemFields.ParentId, ItemFields.Path },
                        sortOrder: new List<SortOrder> { SortOrder.Ascending },
                        parentId: setId,
                        recursive: true,
                        enableImages: true,
                        cancellationToken: token);
                    await Task.WhenAll(albumResult, songResult);

                    BaseItemDto? album = albumResult.Result.Items.FirstOrDefault();
                    if (album == null)
                    {
                        return Album.Empty;
                    }
                    toReturn = await AlbumBuilder(album, true);

                    // Set Song information
                    List<Song> songList = new List<Song>();
                    foreach (var song in songResult.Result.Items)
                    {
                        // Create object
                        Song newSong = SongBuilder(song);
                        if (songCache.ContainsKey(newSong.id))
                        {
                            songList.Add(songCache[newSong.id]);
                            continue;
                        }
                        newSong.album = toReturn;

                        // Create Artists
                        List<Artist> artistList = new List<Artist>();
                        foreach (NameGuidPair partialArtist in song.ArtistItems)
                        {
                            if (artistCache.ContainsKey(partialArtist.Id))
                            {
                                artistList.Add(artistCache[partialArtist.Id]);
                            }
                            artistList.Add(ArtistBuilder(partialArtist));
                        }
                        newSong.artists = artistList.ToArray();

                        songList.Add(newSong);
                    }
                    toReturn.songs = songList.ToArray();
                }
                catch (HttpRequestException)
                {
                    isOffline = true;
                    ReturnFromCache();
                }

            }

            return toReturn;
        }
        #endregion

        #region Songs
        /// <summary>
        /// Retrieves all songs asynchronously.
        /// </summary>
        /// <param name="setLimit">Optional limit for the number of songs to retrieve.</param>
        /// <param name="setStartIndex">Optional start index for retrieving songs.</param>
        /// <param name="setFavourites">Optional flag to filter songs by favorites.</param>
        /// <param name="setSortTypes">Optional. Specify one or more sort orders, comma delimited. Options: Album, AlbumArtist, Artist, Budget, CommunityRating, CriticRating, DateCreated, DatePlayed, PlayCount, PremiereDate, ProductionYear, SortName, Random, Revenue, Runtime.</param>
        /// <param name="setSortOrder">Specifies the sort order for the songs.</param>
        /// <returns>An array of songs.</returns>
        public async Task<Song[]> GetAllSongsAsync(int? setLimit = null, int? setStartIndex = 0, bool? setFavourites = false, IEnumerable<String>? setSortTypes = null, SortOrder setSortOrder = SortOrder.Descending)
        {
            if (_itemsClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Song> toReturn = new();
            // Function to run that returns data from the cache
            void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                List<Song> filteredCache = songCache.Values.ToList();

                // Apply filters based on parameters
                if (setFavourites == true)
                {
                    filteredCache = filteredCache.Where(song => song.isFavourite == setFavourites.Value).ToList();
                }

                // Apply sorting if sort types are provided
                if (setSortTypes != null && setSortTypes.Any())
                {
                    foreach (var sortType in setSortTypes)
                    {
                        switch (sortType)
                        {
                            // Implement sorting logic based on sort types
                            // For example, sorting by album name
                            case "Name":
                                filteredCache = setSortOrder == SortOrder.Ascending ?
                                    filteredCache.OrderBy(song => song.name).ToList() :
                                    filteredCache.OrderByDescending(song => song.name).ToList();
                                break;
                            case "PlayCount":
                                filteredCache = setSortOrder == SortOrder.Ascending ?
                                    filteredCache.OrderBy(song => song.playCount).ToList() :
                                    filteredCache.OrderByDescending(song => song.playCount).ToList();
                                break;

                            case "Random":
                                Random random = new Random();
                                filteredCache = filteredCache.OrderBy(song => random.Next()).ToList();
                                break;
                            // Add cases as needed
                        }
                    }
                }

                // Apply count limits
                if (setStartIndex.HasValue)
                {
                    filteredCache = filteredCache.Skip(setStartIndex.Value).ToList();
                }
                if (setLimit.HasValue)
                {
                    filteredCache = filteredCache.Take(setLimit.Value).ToList();
                }

                // Update toReturn with filtered and sorted cache items
                toReturn.AddRange(filteredCache);
            }

            if (isOffline)
            { // If offline, return all from the cache
                ReturnFromCache();
            }
            else
            { // Call server
                try
                {
                    // Make server call
                    BaseItemDtoQueryResult serverResults = await _itemsClient.GetItemsAsync(
                        userId: userDto.Id,
                        isFavorite: setFavourites,
                        sortBy: setSortTypes,
                        sortOrder: new List<SortOrder> { setSortOrder },
                        includeItemTypes: new List<BaseItemKind> { BaseItemKind.Audio },
                        limit: setLimit,
                        startIndex: setStartIndex,
                        recursive: true,
                        enableImages: true,
                        enableTotalRecordCount: true);
                    for (int i = 0; i < serverResults.Items.Count(); i++)
                    {
                        Song newSong = SongBuilder(serverResults.Items[i]);
                        toReturn.Add(newSong);
                        if (!songCache.TryAdd(newSong.id, newSong))
                        {
                            songCache[newSong.id] = newSong;
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    isOffline = true;
                    ReturnFromCache();
                }
            }

            return toReturn.ToArray();
        }

        /// <summary>
        /// Retrieves a song asynchronously based on the provided ID.
        /// </summary>
        /// <param name="setId">The ID of the song to retrieve.</param>
        /// <returns>The song corresponding to the provided ID, or an empty song if not found.</returns>
        public async Task<Song> GetSongAsync(Guid setId)
        {
            Song toReturn = Song.Empty;
            if (_itemsClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                Song? fromCache = Song.Empty;
                songCache.TryGetValue((Guid)setId, out fromCache);
                if (fromCache != null)
                {
                    toReturn = fromCache;
                }
            }

            if (isOffline)
            {
                ReturnFromCache();
            }
            else
            {
                try
                {
                    Task<BaseItemDtoQueryResult> songResult = _itemsClient.GetItemsAsync(
                        userId: userDto.Id,
                        includeItemTypes: new List<BaseItemKind> { BaseItemKind.Audio },
                        ids: new List<Guid> { setId },
                        recursive: true,
                        enableImages: true,
                        cancellationToken: token);
                    // TODO: Include artistResults, and call API synchronously
                    await Task.WhenAll(songResult);

                    BaseItemDto? song = songResult.Result.Items.FirstOrDefault();
                    if (song == null)
                    {
                        return Song.Empty;
                    }
                    toReturn = SongBuilder(song);
                    if (!songCache.TryAdd(toReturn.id, toReturn))
                    { // Add item to cache
                        songCache[toReturn.id] = toReturn;
                    }
                }
                catch (Exception)
                {
                    isOffline = true;
                    ReturnFromCache();
                }
            }

            return toReturn;
        }
        #endregion

        #region Artists      
        // TODO: Update to include caching la la la
        public async Task<Artist[]> GetAllArtistsAsync(int? limit = 50, int? startFromIndex = 0, bool? favourites = false, CancellationToken? cancellationToken = null)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicArtist };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            CancellationToken ctoken = new();
            if (cancellationToken != null) { ctoken = (CancellationToken)cancellationToken; }

            BaseItemDtoQueryResult artistResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                if (favourites == true)
                {
                    artistResult = await _itemsClient.GetItemsAsync(isFavorite: true, userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: ctoken);
                }
                else
                {
                    artistResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: ctoken);
                }
                TotalArtistRecordCount = artistResult.TotalRecordCount;
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

            if (artistResult == null || ctoken.IsCancellationRequested) { return new Artist[0]; }
            if (artistResult.Items == null || ctoken.IsCancellationRequested) { return new Artist[0]; }

            List<Artist> artists = new List<Artist>();
            foreach (var item in artistResult.Items)
            {
                Artist toAdd = ArtistBuilder(item);
                toAdd.image = MusicItemImageBuilder(item);
                artists.Add(toAdd);
            }

            return artists.ToArray();
        }

        // TODO: Update to include caching
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
                artistInfo = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: _searchIds, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypesArtist, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: token);
                if (token.IsCancellationRequested) { return Artist.Empty; }
                albumResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, artistIds: _searchIds, includeItemTypes: _includeItemTypesAlbums, recursive: true, cancellationToken: token);
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
            if (artistInfo == null || token.IsCancellationRequested) { return Artist.Empty; }
            if (artistInfo.Items.Count <= 0 || token.IsCancellationRequested) { return Artist.Empty; }

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

            if (!MauiProgram.artistDictionary.ContainsKey(returnArtist.id))
            {
                MauiProgram.artistDictionary.Add(returnArtist.id, returnArtist);
            }

            return returnArtist;
        }
        #endregion

        public async Task<Playlist[]> GetPlaylistsAsync(int? limit = 50, int? startFromIndex = 0)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Playlist };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            BaseItemDtoQueryResult playlistResult = new BaseItemDtoQueryResult();
            try
            {
                playlistResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: token);
                TotalPlaylistRecordCount = playlistResult.TotalRecordCount;
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

            if (playlistResult == null || token.IsCancellationRequested) { return new Playlist[0]; }
            if (playlistResult.Items == null || token.IsCancellationRequested) { return new Playlist[0]; }

            List<Playlist> playlists = new List<Playlist>();
            await Parallel.ForEachAsync(playlistResult.Items, async (i, ct) => {
                // Create new webClient and UserLibraryClient so we aren't still waiting on the last 
                // HttpClient to finish :3
                // This does cause the operation to run a fuckton slower but idek what to do about it
                Playlist tempPlaylist = PlaylistBuilder(i);

                if (MauiProgram.hideM3u)
                {
                    HttpClient webClient = new();
                    UserLibraryClient tempClient = new(_sdkClientSettings, webClient);
                    BaseItemDto extraInfo = await tempClient.GetItemAsync(userId: userDto.Id, itemId: i.Id, cancellationToken: token);

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    tempPlaylist.path = extraInfo.Path;
                    if (!tempPlaylist.path.EndsWith(".m3u") && !tempPlaylist.path.EndsWith(".m3u8"))
                    {
                        playlists.Add(tempPlaylist);
                    }
                }
                else
                {
                    playlists.Add(tempPlaylist);
                }
            });
            return playlists.ToArray();
        }
        public async Task<Playlist?> FetchPlaylistByIDAsync(Guid playlistId)
        {
            List<Guid> _filterIds = new List<Guid> { playlistId };

            BaseItemDtoQueryResult? playlistSongResult;
            BaseItemDtoQueryResult? playlistResult;

            try
            {
                playlistResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: _filterIds, recursive: true, enableImages: true, cancellationToken: token);
                if (token.IsCancellationRequested) { cancelTokenSource = new(); return Playlist.Empty; }
                playlistSongResult = await _playlistsClient.GetPlaylistItemsAsync(playlistId: playlistId, userId: userDto.Id, enableImages: true, cancellationToken: token);
            }
            catch (Exception)
            {
                throw;
            }

            if (playlistResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return Playlist.Empty;
            }

            Playlist newPlaylist = new Playlist();
            foreach (BaseItemDto item in playlistResult.Items)
            {
                if(item.Id == playlistId)
                {
                    newPlaylist.id = playlistId;
                    newPlaylist.name = item.Name;
                    newPlaylist.isFavourite = item.UserData.IsFavorite;
                    newPlaylist.image = MusicItemImageBuilder(item);
                }
            }

            List<PlaylistSong> songList = new();
            foreach (BaseItemDto songItem in playlistSongResult.Items)
            {
 
                List<Guid> artistIds = new();
                foreach (NameGuidPair artist in songItem.AlbumArtists)
                {
                    artistIds.Add(artist.Id);
                }

                PlaylistSong newSong = new(
                    setGuid: songItem.Id,
                    setPlaylistId: songItem.PlaylistItemId,
                    setName: songItem.Name,
                    setArtistIds: artistIds.ToArray(),
                    setAlbumID: songItem.AlbumId,
                    setDiskNum: 0, //TODO: Fix disk num
                    setIsFavourite: songItem.UserData.IsFavorite);

                MusicItemImage image = MusicItemImageBuilder(songItem);
                newSong.image = image;

                if (!MauiProgram.songDictionary.ContainsKey(newSong.id))
                {
                    MauiProgram.songDictionary.Add(newSong.id, newSong);
                }
                else
                {
                    MauiProgram.songDictionary[newSong.id] = newSong;
                }
                
                songList.Add(newSong);
            }
            newPlaylist.songs = songList.ToArray();

            if (!MauiProgram.playlistDictionary.ContainsKey(newPlaylist.id))
            {
                MauiProgram.playlistDictionary.Add(newPlaylist.id, newPlaylist);
            }
            else
            {
                MauiProgram.playlistDictionary[newPlaylist.id] = newPlaylist;
            }

            return newPlaylist;
        }
        public async Task<bool> MovePlaylistItem(Guid playlistId, string itemServerId, int newIndex)
        {
            try
            {
                string apiPlaylistId = playlistId.ToString().Replace("-", string.Empty);
                string apiitemId = itemServerId.Replace("-", string.Empty);

                await _playlistsClient.MoveItemAsync(apiPlaylistId, apiitemId, newIndex);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> RemovePlaylistItem(Guid playlistId, string itemPlaylistId)
        {
            List<string> toRemove = new List<String> { itemPlaylistId };

            string apiPlaylistId = playlistId.ToString().Replace("-", string.Empty);

            await _playlistsClient.RemoveFromPlaylistAsync(apiPlaylistId, toRemove);
            return true;
        }
        public async Task<bool> RemovePlaylistItem(Guid playlistId, string[] itemPlaylistId)
        {
            List<string> toRemove = itemPlaylistId.ToList();

            string apiPlaylistId = playlistId.ToString().Replace("-", string.Empty);

            await _playlistsClient.RemoveFromPlaylistAsync(apiPlaylistId, toRemove);
            return true;
        }
        public async Task<BaseMusicItem[]> SearchAsync(string _searchTerm, bool? sorted = false, int? searchLimit = 50)
        {
            if (String.IsNullOrWhiteSpace(_searchTerm))
            {
                return new BaseMusicItem[0];
            }

            List<BaseItemKind> _albumItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum, BaseItemKind.Audio, BaseItemKind.MusicArtist, BaseItemKind.Playlist };

            SearchHintResult searchResult;
            searchResult = await _searchClient.GetAsync(userId: userDto.Id, searchTerm: _searchTerm, limit: searchLimit, includeItemTypes: _albumItemTypes, includeArtists: true, cancellationToken: token);
            if (token.IsCancellationRequested) { cancelTokenSource = new(); return new Album[0]; }

            if (searchResult.SearchHints.Count() == 0)
            {
                return new BaseMusicItem[0];
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
                itemsResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: searchedIds, sortOrder: _sortOrder, includeItemTypes: _albumItemTypes, limit: searchLimit, recursive: true, enableImages: true, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (itemsResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return new Album[0];
            }

            List<BaseMusicItem> searchResults = new List<BaseMusicItem>();
            foreach (var item in itemsResult.Items)
            {
                try
                {
                    switch (item.Type)
                    {
                        case BaseItemKind.Audio:
                            searchResults.Add(SongBuilder(item));
                            break;
                        case BaseItemKind.MusicAlbum:
                            searchResults.Add(AlbumBuilder(item));
                            break;
                        case BaseItemKind.MusicArtist:
                            searchResults.Add(ArtistBuilder(item));
                            break;
                        //case BaseItemKind.MusicGenre:
                        //    searchResults.Add(GenreBuilder(item));
                        //    break;
                        case BaseItemKind.Playlist:
                            searchResults.Add(PlaylistBuilder(item));
                            break;
                        default:
                            searchResults.Add(AlbumBuilder(item));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            //if(sorted == true)
            //{
            //    searchResults.Sort();
            //}

            return searchResults.ToArray();
        }
        public async Task<int> GetTotalAlbumCount()
        {
            if(TotalAlbumRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
                BaseItemDtoQueryResult recordCount = new BaseItemDtoQueryResult();
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true, cancellationToken: token);
                if (recordCount == null || token.IsCancellationRequested)
                {
                    cancelTokenSource = new();
                    return -1;
                }
                return recordCount.TotalRecordCount;
            }
            return TotalAlbumRecordCount;
        }
        public async Task<int> GetTotalArtistCount()
        {
            if (TotalArtistRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicArtist };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true, cancellationToken: token);

                if (recordCount == null || token.IsCancellationRequested)
                {
                    return -1;
                }

                return recordCount.TotalRecordCount;
            }
            return TotalArtistRecordCount;
        }
        public async Task<int> GetTotalSongCount()
        {
            if (TotalSongRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true, cancellationToken: token);
                if (token.IsCancellationRequested)
                {
                    return -1;
                }
                return recordCount.TotalRecordCount;
            }
            return TotalSongRecordCount;
        }
        public async Task<Song[]> GetAllSongsAsync(int? limit= 50, int? startFromIndex = 0, bool? favourites = false, CancellationToken? cancellationToken = null)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            CancellationToken ctoken = new();
            if (cancellationToken != null) { ctoken = (CancellationToken)cancellationToken; }

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                if(favourites == true)
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, isFavorite: true, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: ctoken);
                }
                else
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: ctoken);
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
            if (songResult == null || ctoken.IsCancellationRequested) { return new Song[0]; }
            if (songResult.Items == null || ctoken.IsCancellationRequested) { return new Song[0]; }

            List<Song> songs = new List<Song>();
            foreach (var item in songResult.Items)
            {
                Song toAdd = SongBuilder(item);
                toAdd.image = MusicItemImageBuilder(item);
                songs.Add(toAdd);
            }

            return songs.ToArray();
        }
        public async Task<int> GetTotalGenreCount()
        {
            if (TotalGenreRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _genresClient.GetMusicGenresAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, enableImages: false, enableTotalRecordCount: true, cancellationToken:token);
                if (recordCount == null || token.IsCancellationRequested)
                {
                    return -1;
                }
                return recordCount.TotalRecordCount;
            }
            return TotalGenreRecordCount;
        }
        public async Task<Genre[]> GetAllGenresAsync(int? limit = 50, int? startFromIndex = 0, CancellationToken? cancellationToken = null)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            CancellationToken ctoken = new();
            if (cancellationToken != null) { ctoken = (CancellationToken)cancellationToken; }

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _genresClient.GetMusicGenresAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, enableTotalRecordCount: true, cancellationToken: ctoken);
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
            if (songResult == null || ctoken.IsCancellationRequested) { return new Genre[0]; }
            if (songResult.Items == null || ctoken.IsCancellationRequested) { return new Genre[0]; }

            List<Genre> genres = new List<Genre>();
            foreach (var item in songResult.Items)
            {
                Genre itemToAdd = await GenreBuilder(item);
                genres.Add(itemToAdd);
            }

            return genres.ToArray();
        }
        public async Task FavouriteItem(Guid id, bool setState)
        {
            if (setState)
            {
                await _userLibraryClient.MarkFavoriteItemAsync(userDto.Id, id);
            }
            else
            {
                await _userLibraryClient.UnmarkFavoriteItemAsync(userDto.Id, id);
            }
        }
        public async Task<int> GetTotalPlaylistCount()
        {
            if (TotalPlaylistRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Playlist };
                BaseItemDtoQueryResult recordCount;
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: token);
                if (token.IsCancellationRequested) { cancelTokenSource = new(); return -1; }
                TotalPlaylistRecordCount = recordCount.TotalRecordCount;
            }
            return TotalPlaylistRecordCount;
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
        private Album AlbumBuilder(BaseItemDto baseItem)
        {
            return AlbumBuilder(baseItem, false).Result;
        }
        private async Task<Album> AlbumBuilder(BaseItemDto baseItem, bool fetchFullArtists, Song[]? songs = null)
        {
            if(baseItem == null)
            {
                return Album.Empty;
            }

            Album newAlbum = new();
            newAlbum.name = baseItem.Name;
            newAlbum.id = baseItem.Id;
            newAlbum.image = MusicItemImageBuilder(baseItem);

            if(songs == null)
            {
                newAlbum.songs = new Song[0]; // TODO: Implement songs
            }
            else
            {
                newAlbum.songs = songs;
            }

            // Artists
            if (baseItem.Type != BaseItemKind.MusicArtist) 
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
            newArtist.description = baseItem.Overview;
            newArtist.isFavourite = baseItem.UserData.IsFavorite;
            newArtist.image = MusicItemImageBuilder(baseItem);
            newArtist.isPartial = false;

            newArtist.backgroundImage = MusicItemImageBuilder(baseItem, ImageBuilderImageType.Backdrop);
            newArtist.logoImage = MusicItemImageBuilder(baseItem, ImageBuilderImageType.Logo);

            return newArtist;
        }
        private Artist ArtistBuilder(NameGuidPair nameGuidPair)
        {
            if (nameGuidPair == null)
            {
                return Artist.Empty;
            }

            Artist newArtist = new();
            newArtist.name = nameGuidPair.Name;
            newArtist.id = nameGuidPair.Id;
            newArtist.image = MusicItemImageBuilder(nameGuidPair);
            newArtist.isPartial = true;

            return newArtist;
        }
        private Task<Genre> GenreBuilder(BaseItemDto baseItem)
        {
            // TODO: This is just a rehash of the AlbumBuilder to get the page I needed
            // working really quick. Ideally this'd be redone. 
            return Task.Run(() =>
            {
                Genre newGenre = new();
                newGenre.name = baseItem.Name;
                newGenre.id = baseItem.Id;
                newGenre.image = MusicItemImageBuilder(baseItem);
                newGenre.songs = null; // TODO: Implement songs

                if (baseItem.Type != BaseItemKind.MusicAlbum)
                {
                    if (baseItem.AlbumId != null)
                    {
                        newGenre.id = (Guid)baseItem.AlbumId;
                    }
                }

                // TODO: Implement getting album images for each genre 
                return newGenre;
            });

        }
        public enum ImageBuilderImageType
        {
            Primary,
            Backdrop,
            Logo
        }
        private MusicItemImage MusicItemImageBuilder(BaseItemDto baseItem, ImageBuilderImageType? imageType = ImageBuilderImageType.Primary)
        {
            MusicItemImage image = new();
            image.musicItemImageType = MusicItemImage.MusicItemImageType.url;

            string imgType = "Primary";
            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    imgType = "Backdrop";
                    if (baseItem.ImageBlurHashes.Backdrop != null)
                    { // Set backdrop img 
                        string? hash = baseItem.ImageBlurHashes.Backdrop.FirstOrDefault().Value;
                        if (hash != null)
                        {
                            image.blurHash = hash;
                        }
                    }
                    else if (baseItem.ImageBlurHashes.Primary != null)
                    { // if there is no backdrop, fall back to primary image
                        string? hash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
                        imgType = "Primary";
                        if (hash != null)
                        {
                            image.blurHash = hash;
                        }
                    }
                    break;
                case ImageBuilderImageType.Logo:
                    imgType = "Logo";
                    if (baseItem.ImageBlurHashes.Logo != null)
                    {
                        string? hash = baseItem.ImageBlurHashes.Logo.FirstOrDefault().Value;
                        if (hash != null)
                        {
                            image.blurHash = hash;
                        }
                    }
                    else
                    {
                        image.blurHash = String.Empty;
                        return image; // bascially returning nothing if no logo is found
                    }
                    break;
                default:
                    imgType = "Primary";
                    if (baseItem.ImageBlurHashes.Primary != null)
                    {
                        string? hash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
                        if (hash != null)
                        {
                            image.blurHash = hash;
                        }
                    }
                    else
                    {
                        image.blurHash = String.Empty;
                    }
                    break;
            }

            if (baseItem.Type == BaseItemKind.MusicAlbum)
            {
                if(baseItem.ImageBlurHashes.Primary != null)
                {
                    image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                else
                {
                    image.source = "emptyAlbum.png";
                }
            }
            else if(baseItem.Type == BaseItemKind.Playlist)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;

                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemKind.Audio)
            {
                if(baseItem.AlbumId != null)
                {
                    image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                }
                else
                {
                    image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if(baseItem.Type == BaseItemKind.MusicArtist)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
            }
            else if(baseItem.Type == BaseItemKind.MusicGenre && baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.ImageBlurHashes.Primary != null && baseItem.AlbumId != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id.ToString() + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.ArtistItems != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.ArtistItems.First().Id + "/Images/" + imgType;
            }

            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    image.source += "?format=jpg";
                    break;
                case ImageBuilderImageType.Logo:
                    imgType = "Logo";
                    break;
                default:
                    image.source += "?format=jpg";
                    break;
            }
            

            return image;
        }
        private MusicItemImage MusicItemImageBuilder(NameGuidPair nameGuidPair)
        {
            MusicItemImage image = new();

            image.musicItemImageType = MusicItemImage.MusicItemImageType.url;
            image.source = _sdkClientSettings.BaseUrl + "/Items/" + nameGuidPair.Id + "/Images/Primary?format=jpg";

            return image;
        }
        private Playlist PlaylistBuilder(BaseItemDto baseItem)
        {
            return PlaylistBuilder(baseItem, false).Result;
        }
        private Task<Playlist> PlaylistBuilder(BaseItemDto baseItem, bool fetchFullArtists)
        {
            return Task.Run(() =>
            {
                Playlist newPlaylist = new();
                newPlaylist.name = baseItem.Name;
                newPlaylist.id = baseItem.Id;
                newPlaylist.isFavourite = baseItem.UserData.IsFavorite;
                newPlaylist.songs = null; // TODO: Implement songs

                if (baseItem.Type != BaseItemKind.MusicAlbum)
                {
                    if (baseItem.AlbumId != null)
                    {
                        newPlaylist.id = (Guid)baseItem.AlbumId;
                    }
                }

                newPlaylist.image = MusicItemImageBuilder(baseItem);

                return newPlaylist;
            });   
        }
        private Song SongBuilder(BaseItemDto baseItem)
        {
            PlaylistSong newSong = new(
                    setGuid: baseItem.Id,
                    setPlaylistId: baseItem.PlaylistItemId,
                    setName: baseItem.Name,
                    setAlbumID: baseItem.AlbumId,
                    setDiskNum: 0, //TODO: Fix disk num
                    setIsFavourite: baseItem.UserData.IsFavorite);
            newSong.playCount = baseItem.UserData.PlayCount;
            newSong.image = MusicItemImageBuilder(baseItem);
            newSong.streamUrl = _sdkClientSettings.BaseUrl + "/Audio/" + baseItem.Id + "/stream";

            List<Artist> artists = new List<Artist>();
            foreach (var item in baseItem.AlbumArtists)
            {
                artists.Add(ArtistBuilder(item));
            }
            newSong.artists = artists.ToArray();
            return newSong;
        }
    }
}