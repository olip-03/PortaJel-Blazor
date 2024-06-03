using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;
using Newtonsoft.Json.Linq;
using PortaJel_Blazor.Data;
using PortaJel_Blazor.Classes.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace PortaJel_Blazor.Classes
{
    // https://media.olisshittyserver.xyz/api-docs/swagger/index.html
    // {69c72555-b29b-443d-9a17-01d735bd6f9f} UID
    // /Artists to get all artists 
    // +
    // /Users/{userId}/Items/Latest endpoint to fetch MOST RECENT media added to the server
    // https://github.com/crobibero/jellyfin-client-avalonia/blob/master/src/Jellyfin.Mvvm/Services/LibraryService.cs
    public class ServerConnecter : IMediaServerConnector //TODO implement all data return classes into this interface
    {
        private UserDto? userDto = null;

        private readonly JellyfinSdkSettings _sdkClientSettings = new();
        private readonly JellyfinApiClient? _jellyfinApiClient = null;

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

        // https://stackoverflow.com/questions/26020/what-is-the-best-way-to-connect-and-use-a-sqlite-database-from-c-sharp
        public ServerConnecter(string baseUrl, string? username = null, string? password = null)
        {
            var serviceProvider = ConfigureServices();

            _jellyfinApiClient = serviceProvider.GetRequiredService<JellyfinApiClient>();
            _sdkClientSettings = serviceProvider.GetRequiredService<JellyfinSdkSettings>();
            _sdkClientSettings.SetServerUrl(baseUrl);
            _sdkClientSettings.Initialize(
                MauiProgram.applicationName,
                MauiProgram.applicationClientVersion,
                Microsoft.Maui.Devices.DeviceInfo.Current.Name,
                Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString());
            token = cancelTokenSource.Token;

            if(username != null)
            {
                Username = username;
            }
            if(password != null)
            {
                StoredPassword = password;
            }

        }

        private ServiceProvider ConfigureServices()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            
            // Add Http Client
            serviceCollection.AddHttpClient("Default", c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue(
                        MauiProgram.applicationName,
                        MauiProgram.applicationClientVersion));

                c.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json, 1.0));
                c.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            })
                .ConfigurePrimaryHttpMessageHandler(_ => new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8
                });

            // Add Jellyfin SDK services.
            serviceCollection.AddSingleton<JellyfinSdkSettings>();
            serviceCollection.AddSingleton<IAuthenticationProvider, JellyfinAuthenticationProvider>();
            serviceCollection.AddScoped<IRequestAdapter, JellyfinRequestAdapter>(s => new JellyfinRequestAdapter(
                s.GetRequiredService<IAuthenticationProvider>(),
                s.GetRequiredService<JellyfinSdkSettings>(),
                s.GetRequiredService<IHttpClientFactory>().CreateClient("Default")));
            serviceCollection.AddScoped<JellyfinApiClient>();

            // Add sample service
            return serviceCollection.BuildServiceProvider();
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
        public async Task<bool> AuthenticateServerAsync(string? username = null, string? password = null)
        {
            if (_jellyfinApiClient == null)
            {
                throw new InvalidOperationException("API Client is Null!");
            }

            if(username != null)
            {
                Username = username;
            }
            else if(Username == null)
            {
                throw new InvalidOperationException("Cannot authenticate because Username is null!");
            }

            if (password != null)
            {
                StoredPassword = password;
            }
            else if(StoredPassword == null)
            {
                throw new InvalidOperationException("Cannot authenticate because Password is null!");
            }

            try
            {
                var authenticationResult = await _jellyfinApiClient.Users.AuthenticateByName.PostAsync(new AuthenticateUserByName
                {
                    Username = this.Username,
                    Pw = this.StoredPassword
                }).ConfigureAwait(false);

                if(authenticationResult != null)
                {
                    _sdkClientSettings.SetAccessToken(authenticationResult.AccessToken);
                    userDto = authenticationResult.User;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public async Task<bool> AuthenticateAddressAsync(string serverUrl)
        {
            return true;
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
        public async Task<Album[]> GetAllAlbumsAsync(int? setLimit = null, int? setStartIndex = 0, bool? setFavourites = false, ItemSortBy[]? setSortTypes = null, SortOrder setSortOrder = SortOrder.Ascending)
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Album> toReturn = new();
            // Function to run that returns data from the cache
            void ReturnFromCache()
            {
                // TODO: REPLACE WITH SQLITE QUERY TO PULL FROM LOCAL STORAGE :3 

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
                            case ItemSortBy.Name:
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
                setSortTypes = new ItemSortBy[] { ItemSortBy.Name };
            }

            if (isOffline)
            { // If offline, return all from the cache
                ReturnFromCache();
            }
            else
            { // Call server
                try
                {
                    // Call server and return items
                    BaseItemDtoQueryResult? serverResults = await _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IsFavorite = setFavourites;
                        c.QueryParameters.SortBy = setSortTypes;
                        c.QueryParameters.SortOrder = [setSortOrder];
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                        c.QueryParameters.Limit = setLimit;
                        c.QueryParameters.StartIndex = setStartIndex;
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                        c.QueryParameters.EnableTotalRecordCount = true;
                    }).ConfigureAwait(false);
                    if(serverResults != null && serverResults.Items != null)
                    {
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
            if(_jellyfinApiClient == null || userDto == null)
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
                    Task<BaseItemDtoQueryResult?> albumResult = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.Ids = [setId];
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);
                    Task<BaseItemDtoQueryResult?> songResult = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                        c.QueryParameters.SortBy = [ItemSortBy.Album, ItemSortBy.SortName];
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.SortOrder = [SortOrder.Ascending];
                        c.QueryParameters.ParentId = setId;
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);

                    await Task.WhenAll(albumResult, songResult);

                    if(albumResult.Result != null)
                    {
                        BaseItemDto? album = albumResult.Result.Items.FirstOrDefault();
                        if (album == null)
                        {
                            return Album.Empty;
                        }
                        toReturn = await AlbumBuilder(album, true);
                    }

                    // Set Song information
                    List<Song> songList = new List<Song>();
                    foreach (var song in songResult.Result.Items)
                    {
                        // Create object
                        Song newSong = Song.Builder(song, _sdkClientSettings.ServerUrl);
                        newSong.album = toReturn;

                        // Create Artists
                        List<Artist> artistList = new List<Artist>();
                        foreach (NameGuidPair partialArtist in song.ArtistItems)
                        {
                            if (artistCache.ContainsKey((Guid)partialArtist.Id))
                            {
                                artistList.Add(artistCache[(Guid)partialArtist.Id]);
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

        /// <summary>
        /// Retrieves similar albums asynchronously based on the provided album ID.
        /// </summary>
        /// <param name="setId">The ID of the album set to find similar albums for.</param>
        /// <returns>An array of albums that are similar to the provided album set.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the server connector has not been initialized.</exception>
        public async Task<Album[]> GetSimilarAlbumsAsync(Guid setId, int limit = 30)
        {
            if (_jellyfinApiClient == null || userDto == null) throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            List<Album> toReturn = new();
            BaseItemDtoQueryResult? result = await _jellyfinApiClient.Albums[setId].Similar.GetAsync(c =>
            {
                c.QueryParameters.UserId = userDto.Id;
                c.QueryParameters.Limit = limit;
            });
            if(result != null && result.Items != null)
            {
                foreach (BaseItemDto albumResult in result.Items)
                {
                    toReturn.Add(AlbumBuilder(albumResult));
                }
            }
            return toReturn.ToArray();
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
        public async Task<Song[]> GetAllSongsAsync(int? setLimit = null, int? setStartIndex = 0, bool? setFavourites = false, ItemSortBy[]? setSortTypes = null, SortOrder[]? setSortOrder = null)
        {
            if (_jellyfinApiClient == null || userDto == null || _sdkClientSettings.ServerUrl == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Song> toReturn = new();
            // Function to run that returns data from the cache
            void ReturnFromCache()
            { 
                // TODO: Return information from SQLite 
                // Filter the cache based on the provided parameters
                List<Song> filteredCache = songCache.Values.ToList();

                // Apply filters based on parameters
                if (setFavourites == true)
                {
                    filteredCache = filteredCache.Where(song => song.isFavourite == setFavourites.Value).ToList();
                }

                // Apply sorting if sort types are provided
                if (setSortOrder != null && setSortTypes != null && setSortTypes.Any())
                {
                    foreach (var sortType in setSortTypes)
                    {
                        switch (sortType)
                        {
                            // Implement sorting logic based on sort types
                            // For example, sorting by album name
                            case ItemSortBy.Name:
                                filteredCache = setSortOrder.Contains(SortOrder.Ascending) ?
                                    filteredCache.OrderBy(song => song.name).ToList() :
                                    filteredCache.OrderByDescending(song => song.name).ToList();
                                break;
                            case ItemSortBy.PlayCount:
                                filteredCache = setSortOrder.Contains(SortOrder.Ascending) ?
                                    filteredCache.OrderBy(song => song.playCount).ToList() :
                                    filteredCache.OrderByDescending(song => song.playCount).ToList();
                                break;

                            case ItemSortBy.Random:
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
                    BaseItemDtoQueryResult? serverResults = await _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IsFavorite = setFavourites;
                        c.QueryParameters.SortBy = setSortTypes;
                        c.QueryParameters.SortOrder = setSortOrder;
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                        c.QueryParameters.Limit = setLimit;
                        c.QueryParameters.StartIndex = setStartIndex;
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                        c.QueryParameters.EnableTotalRecordCount = true;
                    }).ConfigureAwait(false);
                    if (serverResults != null && serverResults.Items != null)
                    {
                        for (int i = 0; i < serverResults.Items.Count(); i++)
                        {
                            Song newSong = Song.Builder(serverResults.Items[i], _sdkClientSettings.ServerUrl);
                            toReturn.Add(newSong);
                            if (!songCache.TryAdd(newSong.id, newSong))
                            {
                                songCache[newSong.id] = newSong;
                            }
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
            if (_jellyfinApiClient == null || userDto == null)
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
                    Task<BaseItemDtoQueryResult?> songResult = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                        c.QueryParameters.Ids = [setId];
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);
                    // TODO: Include artistResults, and call API synchronously
                    await Task.WhenAll(songResult);

                    BaseItemDto? song = songResult.Result.Items.FirstOrDefault();
                    if (song == null)
                    {
                        return Song.Empty;
                    }
                    toReturn = Song.Builder(song, _sdkClientSettings.ServerUrl);
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
        public async Task<Artist[]> GetAllArtistsAsync(int? limit = 50, int? startFromIndex = 0, bool? favourites = false)
        {
            if(_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            // TODO: Add return cache which pulls from SQLite

            BaseItemDtoQueryResult? artistResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                artistResult = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.IsFavorite = favourites;
                    c.QueryParameters.SortBy = [ItemSortBy.Name];
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                    c.QueryParameters.Limit = limit;
                    c.QueryParameters.StartIndex = startFromIndex;
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true; 
                });
                TotalArtistRecordCount = (int)artistResult.TotalRecordCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

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
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            BaseItemDtoQueryResult? artistInfo = new BaseItemDtoQueryResult();
            BaseItemDtoQueryResult? albumResult = new BaseItemDtoQueryResult();

            // Call GetItemsAsync with the specified parameters
            try
            {
                Task<BaseItemDtoQueryResult?> runArtistInfo = _jellyfinApiClient.Items.GetAsync(c => {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.Ids = [artistId];
                    c.QueryParameters.SortBy = [ItemSortBy.Name];
                    c.QueryParameters.SortOrder = [SortOrder.Ascending];
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.Fields = [ItemFields.Overview];
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                });
                Task<BaseItemDtoQueryResult?> runAlbumInfo = _jellyfinApiClient.Items.GetAsync(c => {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.ArtistIds = [artistId];
                    c.QueryParameters.SortBy = [ItemSortBy.Name];
                    c.QueryParameters.SortOrder = [SortOrder.Ascending];
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.Fields = [ItemFields.Overview];
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                });
                await Task.WhenAll( runArtistInfo, runAlbumInfo);

                artistInfo = runArtistInfo.Result;
                albumResult = runAlbumInfo.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            // Catch blocks
            if (artistInfo == null || artistInfo.Items == null || albumResult == null || token.IsCancellationRequested) { return Artist.Empty; }
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

            return returnArtist;
        }
        
        public async Task<Artist[]> GetSimilarArtistsAsync(Guid setId, int limit = 30)
        {
            if (_jellyfinApiClient == null || userDto == null) throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            List<Artist> toReturn = new();
            BaseItemDtoQueryResult? result = await _jellyfinApiClient.Artists[setId.ToString()].Similar.GetAsync(c =>
            {
                c.QueryParameters.UserId = userDto.Id;
                c.QueryParameters.Limit = limit;
            });
            if (result != null && result.Items != null)
            {
                foreach (BaseItemDto albumResult in result.Items)
                {
                    toReturn.Add(ArtistBuilder(albumResult));
                }
            }
            return toReturn.ToArray();
        }
        #endregion

        #region Playlists
        public async Task<Playlist[]> GetPlaylistsAsync(int? limit = 50, int? startFromIndex = 0)
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            BaseItemDtoQueryResult? playlistResult = new BaseItemDtoQueryResult();
            try
            {
                playlistResult = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.SortBy = [ItemSortBy.Name];
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.Playlist];
                    c.QueryParameters.Limit = limit;
                    c.QueryParameters.StartIndex = startFromIndex;
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                    c.QueryParameters.Fields = [ItemFields.Path];
                }, cancellationToken: token);

                TotalPlaylistRecordCount = (int)playlistResult.TotalRecordCount;
            }
            catch (HttpRequestException netEx)
            { // NETWORK EXCEPTION, NETWORK FAILURE

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (playlistResult == null || token.IsCancellationRequested) { return new Playlist[0]; }
            if (playlistResult.Items == null || token.IsCancellationRequested) { return new Playlist[0]; }

            List<Playlist> playlists = new List<Playlist>();
            foreach (BaseItemDto item in playlistResult.Items)
            {
                Playlist toAdd = PlaylistBuilder(item);

                if (MauiProgram.hideM3u)
                {
                    if (!toAdd.path.EndsWith(".m3u") && !toAdd.path.EndsWith(".m3u8"))
                    {
                        playlists.Add(toAdd);
                    }
                }
                else
                {
                    playlists.Add(toAdd);
                }
            }
            return playlists.ToArray();
        }
        public async Task<Playlist?> FetchPlaylistByIDAsync(Guid playlistId)
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Guid> _filterIds = new List<Guid> { playlistId };

            Task<BaseItemDtoQueryResult?> playlistSongResult;
            Task<BaseItemDtoQueryResult?> playlistResult;
            try
            {
                playlistResult = _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.Ids = [playlistId];
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                }, cancellationToken: token);
                playlistSongResult = _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.ParentId = playlistId;
                    c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                    c.QueryParameters.EnableImages = true;
                });
                await Task.WhenAll(playlistResult, playlistSongResult);
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
            foreach (BaseItemDto item in playlistResult.Result.Items)
            {
                if(item.Id == playlistId)
                {
                    newPlaylist.id = playlistId;
                    newPlaylist.name = item.Name;
                    newPlaylist.isFavourite = (bool)item.UserData.IsFavorite;
                    newPlaylist.image = MusicItemImageBuilder(item);
                }
            }

            List<Song> songList = new();
            foreach (BaseItemDto songItem in playlistSongResult.Result.Items)
            {
 
                List<Guid> artistIds = new();
                foreach (NameGuidPair artist in songItem.AlbumArtists)
                {
                    artistIds.Add((Guid)artist.Id);
                }

                Song newSong = Song.Builder(songItem, _sdkClientSettings.ServerUrl);
                
                songList.Add(newSong);
            }
            newPlaylist.songs = songList.ToArray();

            return newPlaylist;
        }
        public async Task<bool> MovePlaylistItem(Guid playlistId, string itemServerId, int newIndex)
        {
            try
            {
                //await _jellyfinApiClient.Playlists.PostAsync(new(), c => {
                //    c.QueryParameters.
                //});
                //await _playlistsClient.MoveItemAsync(apiPlaylistId, apiitemId, newIndex);
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

            //await _playlistsClient.RemoveFromPlaylistAsync(apiPlaylistId, toRemove);
            return true;
        }
        public async Task<bool> RemovePlaylistItem(Guid playlistId, string[] itemPlaylistId)
        {
            List<string> toRemove = itemPlaylistId.ToList();

            string apiPlaylistId = playlistId.ToString().Replace("-", string.Empty);

            //await _playlistsClient.RemoveFromPlaylistAsync(apiPlaylistId, toRemove);
            return true;
        }
        #endregion

        #region PlaybackReporting
        // POST
        // ​/Sessions​/Playing
        // Reports playback has started within a session.
        public async Task<bool> SessionReportPlaying()
        {
            //if(_playstateClient != null)
            //{
            //    PlaybackStartInfo playbackStartInfo = new PlaybackStartInfo();
            //    await _jellyfinApiClient.
            //    await _playstateClient.ReportPlaybackStartAsync(body: playbackStartInfo);
            //    return true;
            //}
            if (_jellyfinApiClient != null)
            {
                //await _jellyfinApiClient.Sessions.Playing.Progress.PostAsync(c =>
                //{
                //    c.
                //})
                return true;
            }
            return false;
        }

        // POST
        // ​/Sessions​/Playing​/Ping
        // Pings a playback session.
        public async Task<bool> SessionReportPing(string playSessionId)
        {
            if(_jellyfinApiClient != null)
            {
                await _jellyfinApiClient.Sessions.Playing.Ping.PostAsync(c =>
                {
                    c.QueryParameters.PlaySessionId = playSessionId;
                });
                return true;
            }
            return false;
        }

        // POST
        // ​/Sessions​/Playing​/Progress
        // Reports playback progress within a session.
        public async Task<bool> SessionReportProgress()
        {
            //if (_playstateClient != null)
            //{
            //    PlaybackProgressInfo playbackProgressInfo = new();
            //    await _playstateClient.ReportPlaybackProgressAsync(body: playbackProgressInfo);
            //    return true;
            //}
            return false;
        }

        // POST
        // ​/Sessions​/Playing​/Stopped
        // Reports playback has stopped within a session.
        public async Task<bool> SessionReportStopped()
        {
            //if (_playstateClient != null)
            //{
            //    PlaybackStopInfo playbackStopInfo = new();
            //    await _playstateClient.ReportPlaybackStoppedAsync(playbackStopInfo);
            //    return true;
            //}
            return false;
        }

        // POST
        // ​/Users​/{userId}​/PlayedItems​/{itemId}
        // Marks an item as played for user.
        public async Task<bool> SessionReportUpdatePlayedItem(Guid itemId)
        {
            //if (_playstateClient != null && userDto != null)
            //{
            //    PlaybackStopInfo playbackStopInfo = new();
            //    await _playstateClient.MarkPlayedItemAsync(userDto.Id, itemId);
            //    return true;
            //}
            return false;
        }

        // DELETE
        // ​/Users​/{userId}​/PlayedItems​/{itemId}
        // Marks an item as unplayed for user.

        // POST
        // ​/Users​/{userId}​/PlayingItems​/{itemId}
        // Reports that a user has begun playing an item.
        public async Task<bool> SessionReportBeginPlayingItem(Guid itemId)
        {
            //if (_playstateClient != null && userDto != null)
            //{
            //    PlaybackStopInfo playbackStopInfo = new();
            //    await _playstateClient.MarkUnplayedItemAsync(userDto.Id, itemId);
            //    return true;
            //}
            return false;
        }

        // DELETE
        // ​/Users​/{userId}​/PlayingItems​/{itemId}
        // Reports that a user has stopped playing an item.

        // POST
        // ​/Users​/{userId}​/PlayingItems​/{itemId}​/Progress
        // Reports a user's playback progress.
        public async Task<bool> SessionReportPlaybackProgress(Guid itemId)
        {
            if (_jellyfinApiClient != null && userDto != null)
            {
                PlaybackProgressInfo playbackprogressinfo = new();
                await _jellyfinApiClient.Sessions.Playing.Progress.PostAsync(playbackprogressinfo);
                return true;
            }
            return false;
        }
        #endregion

        public async Task<BaseMusicItem[]> SearchAsync(string _searchTerm, bool? sorted = false, int? searchLimit = 50)
        {
            // TODO: Return from SQLite Cache if offline or null or something idek
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            if (String.IsNullOrWhiteSpace(_searchTerm))
            {
                return new BaseMusicItem[0];
            }

            List<BaseItemKind> _albumItemTypes = new List<BaseItemKind> {  };

            SearchHintResult? searchResult = await _jellyfinApiClient.Search.Hints.GetAsync(c =>
            {
                c.QueryParameters.UserId = userDto.Id;
                c.QueryParameters.SearchTerm = _searchTerm;
                c.QueryParameters.Limit = searchLimit;
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum, BaseItemKind.Audio, BaseItemKind.MusicArtist, BaseItemKind.Playlist, BaseItemKind.MusicGenre];
                c.QueryParameters.IncludeArtists = true;
                c.QueryParameters.IncludeGenres = true;
            }).ConfigureAwait(false);
            if (token.IsCancellationRequested) { cancelTokenSource = new(); return new Album[0]; }

            if (searchResult.SearchHints.Count() == 0)
            {
                return new BaseMusicItem[0];
            }

            List<Guid?> searchedIds = new List<Guid?>();
            foreach (var item in searchResult.SearchHints)
            {
               searchedIds.Add(item.Id);
            }

            BaseItemDtoQueryResult? itemsResult;
            try
            {
                itemsResult = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.Ids = searchedIds.ToArray();
                    c.QueryParameters.SortBy = [ItemSortBy.Name];
                    c.QueryParameters.SortOrder = [SortOrder.Descending];
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum, BaseItemKind.Audio, BaseItemKind.MusicArtist, BaseItemKind.Playlist, BaseItemKind.MusicGenre];
                    c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                    c.QueryParameters.Limit = searchLimit;
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                }).ConfigureAwait(false);
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
                        case BaseItemDto_Type.Audio:
                            searchResults.Add(Song.Builder(item, _sdkClientSettings.ServerUrl));
                            break;
                        case BaseItemDto_Type.MusicAlbum:
                            searchResults.Add(AlbumBuilder(item));
                            break;
                        case BaseItemDto_Type.MusicArtist:
                            searchResults.Add(ArtistBuilder(item));
                            break;
                        //case BaseItemKind.MusicGenre:
                        //    searchResults.Add(GenreBuilder(item));
                        //    break;
                        case BaseItemDto_Type.Playlist:
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
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            if (TotalAlbumRecordCount == -1)
            {
                BaseItemDtoQueryResult? recordCount = new BaseItemDtoQueryResult();
                recordCount = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                });

                if (recordCount == null || token.IsCancellationRequested)
                {
                    cancelTokenSource = new();
                    return -1;
                }
                return (int)recordCount.TotalRecordCount;
            }
            return TotalAlbumRecordCount;
        }
        public async Task<int> GetTotalArtistCount()
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            if (TotalArtistRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicArtist };
                BaseItemDtoQueryResult? recordCount;
                recordCount = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                });

                if (recordCount == null || token.IsCancellationRequested)
                {
                    return -1;
                }

                return (int)recordCount.TotalRecordCount;
            }
            return TotalArtistRecordCount;
        }
        public async Task<int> GetTotalSongCount()
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            if (TotalSongRecordCount == -1)
            {
                List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
                BaseItemDtoQueryResult? recordCount;
                recordCount = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                }); if (token.IsCancellationRequested)
                {
                    return -1;
                }
                return (int)recordCount.TotalRecordCount;
            }
            return TotalSongRecordCount;
        }
        public async Task<Song[]> GetAllSongsAsync(int? limit= 50, int? startFromIndex = 0, bool? favourites = false, CancellationToken? cancellationToken = null)
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            CancellationToken ctoken = new();
            if (cancellationToken != null) { ctoken = (CancellationToken)cancellationToken; }

            BaseItemDtoQueryResult? songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.SortBy = [ItemSortBy.Name];
                    c.QueryParameters.IsFavorite = favourites;
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                    c.QueryParameters.Limit = limit;
                    c.QueryParameters.StartIndex = startFromIndex;
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                    c.QueryParameters.EnableTotalRecordCount = true;
                }, cancellationToken: ctoken);
                TotalSongRecordCount = (int)songResult.TotalRecordCount;
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
                Song toAdd = Song.Builder(item, _sdkClientSettings.ServerUrl);
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
                BaseItemDtoQueryResult? recordCount = await _jellyfinApiClient.MusicGenres.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicGenre];
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                });
                if (recordCount == null || token.IsCancellationRequested)
                {
                    return -1;
                }
                return (int)recordCount.TotalRecordCount;
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

            BaseItemDtoQueryResult songResult = await _jellyfinApiClient.MusicGenres.GetAsync(c =>
            {
                c.QueryParameters.UserId = userDto.Id;
                c.QueryParameters.Limit = limit;
                c.QueryParameters.StartIndex = startFromIndex;
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicGenre];
                c.QueryParameters.EnableImages = true;
                c.QueryParameters.EnableTotalRecordCount = true;
            });

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
        public async Task<bool> FavouriteItem(Guid id, bool setState)
        {
            if(userDto == null || _jellyfinApiClient == null)
            {
                return false;
            }

            if (setState)
            {
                await _jellyfinApiClient.UserFavoriteItems[id].PostAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id; 
                });
            }
            else
            {
                await _jellyfinApiClient.UserFavoriteItems[id].DeleteAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                });
            }
            return setState;
        }
        public async Task<int> GetTotalPlaylistCount()
        {
            if (TotalPlaylistRecordCount == -1)
            {
                BaseItemDtoQueryResult? recordCount = await _jellyfinApiClient.Items.GetAsync(c =>
                {
                    c.QueryParameters.UserId = userDto.Id;
                    c.QueryParameters.IncludeItemTypes = [BaseItemKind.Playlist];
                    c.QueryParameters.EnableImages = true;
                    c.QueryParameters.EnableTotalRecordCount = true;
                });
                if (token.IsCancellationRequested) { cancelTokenSource = new(); return -1; }
                TotalPlaylistRecordCount = (int)recordCount.TotalRecordCount;
            }
            return TotalPlaylistRecordCount;
        }
        public void SetBaseAddress(string url)
        {
            _sdkClientSettings.SetServerUrl(url);
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
            return _sdkClientSettings.ServerUrl;
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
            newAlbum.id = (Guid)baseItem.Id;
            newAlbum.image = MusicItemImageBuilder(baseItem);
            newAlbum.serverAddress = _sdkClientSettings.ServerUrl;

            if (songs == null)
            {
                newAlbum.songs = new Song[0]; // TODO: Implement songs
            }
            else
            {
                newAlbum.songs = songs;
            }

            // Artists
            if (baseItem.Type != BaseItemDto_Type.MusicArtist) 
            {
                if (fetchFullArtists == true && baseItem.AlbumArtists.Count > 0)
                {
                    List<BaseItemKind> _includeItemTypesArtist = new List<BaseItemKind> {  };
                    List<Guid?> _searchIds = new List<Guid?>();
                    BaseItemDtoQueryResult? artistFetchRequest = new BaseItemDtoQueryResult();

                    foreach (var artist in baseItem.AlbumArtists)
                    {
                        _searchIds.Add((Guid)artist.Id);
                    }

                    artistFetchRequest = await _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.Ids = _searchIds.ToArray();
                        c.QueryParameters.SortBy = [ItemSortBy.Name];
                        c.QueryParameters.SortOrder = [SortOrder.Descending];
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                        c.QueryParameters.EnableTotalRecordCount = true;
                    });

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
                        newArist.id = (Guid)artist.Id;
                        newArist.name = artist.Name;

                        artists.Add(newArist);
                    }
                    newAlbum.artists = artists.ToArray();
                }
                // Fetch Artists
                // Do need to do another request here
            }

            // Favourite Info
            if(baseItem.UserData.IsFavorite == true)
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
            newArtist.serverAddress = _sdkClientSettings.ServerUrl;
            newArtist.name = baseItem.Name;
            newArtist.id = (Guid)baseItem.Id;
            newArtist.description = baseItem.Overview;
            newArtist.isFavourite = (bool)baseItem.UserData.IsFavorite;
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
            newArtist.id = (Guid)nameGuidPair.Id;
            newArtist.serverAddress = _sdkClientSettings.ServerUrl;
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
                newGenre.id = (Guid)baseItem.Id;
                newGenre.image = MusicItemImageBuilder(baseItem);
                newGenre.serverAddress = _sdkClientSettings.ServerUrl;
                newGenre.songs = null; // TODO: Implement songs

                if (baseItem.Type != BaseItemDto_Type.MusicAlbum)
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
            image.musicItemImageType = MusicItemImageType.url;

            string imgType = "Primary";
            switch (imageType)
            {
                case ImageBuilderImageType.Backdrop:
                    imgType = "Backdrop";
                    if (baseItem.ImageBlurHashes.Backdrop != null)
                    { // Set backdrop img 
                        string? hash = baseItem.ImageBlurHashes.Backdrop.AdditionalData.First().Value.ToString();
                        if (hash != null)
                        {
                            image.blurHash = hash;
                        }
                    }
                    else if (baseItem.ImageBlurHashes.Primary != null)
                    { // if there is no backdrop, fall back to primary image
                        string? hash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
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
                        string? hash = baseItem.ImageBlurHashes.Logo.AdditionalData.First().Value.ToString();
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
                        string? hash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
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

            if (baseItem.Type == BaseItemDto_Type.MusicAlbum)
            {
                if(baseItem.ImageBlurHashes.Primary != null)
                {
                    image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                else
                {
                    image.source = "emptyAlbum.png";
                }
            }
            else if(baseItem.Type == BaseItemDto_Type.Playlist)
            {
                image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;

                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemDto_Type.Audio)
            {
                if(baseItem.AlbumId != null)
                {
                    image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                }
                else
                {
                    image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
                }
                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if(baseItem.Type == BaseItemDto_Type.MusicArtist)
            {
                image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
            }
            else if(baseItem.Type == BaseItemDto_Type.MusicGenre && baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.Id + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ImageBlurHashes.Primary != null && baseItem.AlbumId != null)
            {
                image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.AlbumId + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.Id.ToString() + "/Images/" + imgType;
                image.blurHash = baseItem.ImageBlurHashes.Primary.AdditionalData.First().Value.ToString();
            }
            else if (baseItem.ArtistItems != null)
            {
                image.source = _sdkClientSettings.ServerUrl + "/Items/" + baseItem.ArtistItems.First().Id + "/Images/" + imgType;
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

            image.musicItemImageType = MusicItemImageType.url;
            image.source = _sdkClientSettings.ServerUrl + "/Items/" + nameGuidPair.Id + "/Images/Primary?format=jpg";

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
                newPlaylist.id = (Guid)baseItem.Id;
                newPlaylist.isFavourite = (bool)baseItem.UserData.IsFavorite;
                newPlaylist.songs = new Song[0]; // TODO: Implement songs
                newPlaylist.path = baseItem.Path;
                newPlaylist.serverAddress = _sdkClientSettings.ServerUrl;

                if (baseItem.Type != BaseItemDto_Type.MusicAlbum)
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
    }
}