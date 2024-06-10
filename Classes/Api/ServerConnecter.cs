﻿using Jellyfin.Sdk;
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
using PortaJel_Blazor.Classes.Database;
using SQLite;
using System.Security.Cryptography;
using System.Collections.Generic;

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

        private SQLiteAsyncConnection? Database = null;
        private SQLite.SQLiteOpenFlags DBFlags =
        SQLite.SQLiteOpenFlags.ReadWrite |        // open the database in read/write mode       
        SQLite.SQLiteOpenFlags.Create | // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.SharedCache; // enable multi-threaded database access

        private List<Guid> storedAlbums = new();
        private List<Guid> storedSongs = new();
        private List<Guid> storedArtists = new();
        private List<Guid> storedPlaylists = new();
        private List<Guid> storedGenres = new();

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

            if (username != null)
            {
                Username = username;
            }
            if (password != null)
            {
                StoredPassword = password;
            }

            System.Guid? result = null;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(baseUrl));
                result = new System.Guid(hash);
            }

            string filePath = Path.Combine(FileSystem.AppDataDirectory, $"{result}.db");
            Database = new SQLiteAsyncConnection(filePath, DBFlags);
            InitDb();
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

        private async void InitDb()
        {
            if (Database == null) return;
            await Database.CreateTableAsync<AlbumData>();
            await Database.CreateTableAsync<SongData>();
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

            if (username != null)
            {
                Username = username;
            }
            else if (Username == null)
            {
                throw new InvalidOperationException("Cannot authenticate because Username is null!");
            }

            if (password != null)
            {
                StoredPassword = password;
            }
            else if (StoredPassword == null)
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

                if (authenticationResult != null)
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
        /// Retrieves partial albums based on the specified parameters asynchronously.
        /// </summary>
        /// <param name="setLimit">The maximum number of albums to retrieve.</param>
        /// <param name="setStartIndex">The index from which to start retrieving albums.</param>
        /// <param name="setFavourites">A boolean value indicating whether to retrieve only favorite albums.</param>
        /// <param name="setSortTypes">Optional. Specify one or more sort orders, comma delimited. Options: Album, AlbumArtist, Artist, Budget, CommunityRating, CriticRating, DateCreated, DatePlayed, PlayCount, PremiereDate, ProductionYear, SortName, Random, Revenue, Runtime.</param>
        /// <param name="setSortOrder">The sort order for the albums.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is an array of partial albums 
        /// matching the specified criteria.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the server connector has not been initialized. Ensure that 
        /// AuthenticateUserAsync method has been called.
        /// </exception>
        public async Task<Data.Album[]> GetAllAlbumsAsync(int? setLimit = null, int? setStartIndex = 0, bool? setFavourites = false, ItemSortBy setSortTypes = ItemSortBy.Default, SortOrder setSortOrder = SortOrder.Ascending)
        {
            if (_jellyfinApiClient == null || userDto == null || Database == null || _sdkClientSettings.ServerUrl == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Album> toReturn = new();
            // Function to run that returns data from the cache
            async void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                if (setLimit == null)
                {
                    setLimit = 50;
                }
                List<AlbumData> filteredCache = new();
                switch (setSortTypes)
                {
                    case ItemSortBy.DateCreated:
                        filteredCache.AddRange(await Database.Table<AlbumData>()
                            .OrderByDescending(album => album.DateAdded)
                            .Take((int)setLimit).ToListAsync());
                        break;
                    case ItemSortBy.Name:
                        filteredCache.AddRange(await Database.Table<AlbumData>()
                            .OrderByDescending(album => album.Name)
                            .Take((int)setLimit).ToListAsync());
                        break;
                    case ItemSortBy.Random:
                        filteredCache = await Database.Table<AlbumData>()
                            .OrderBy(album => Guid.NewGuid())
                            .Take((int)setLimit)
                            .ToListAsync();
                        break;
                    case ItemSortBy.PlayCount:
                        filteredCache.AddRange(await Database.Table<AlbumData>()
                            .OrderByDescending(album => album.PlayCount)
                            .Take((int)setLimit).ToListAsync());
                        break;
                    default:
                        filteredCache.AddRange(await Database.Table<AlbumData>()
                            .OrderByDescending(album => album.Name)
                            .Take((int)setLimit).ToListAsync());
                        break;
                }

                foreach (AlbumData dbItem in filteredCache)
                {
                    //SongData[] songData = [];
                    //ArtistData[] artistData = [];
                    // likely don't need to reconstruct entire thing if we're just chasing partials online
                    toReturn.Add(new Album(dbItem));
                }
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
                    MauiProgram.UpdateDebugMessage("Calling Items Endpoint for All Albums...");
                    BaseItemDtoQueryResult? serverResults = await _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IsFavorite = setFavourites;
                        c.QueryParameters.SortBy = [setSortTypes];
                        c.QueryParameters.SortOrder = [setSortOrder];
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                        c.QueryParameters.Limit = setLimit;
                        c.QueryParameters.StartIndex = setStartIndex;
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                        c.QueryParameters.EnableTotalRecordCount = true;
                    }).ConfigureAwait(false);
                    if (serverResults != null && serverResults.Items != null)
                    {
                        MauiProgram.UpdateDebugMessage("Retrieved! Updating tables.");
                        for (int i = 0; i < serverResults.Items.Count(); i++)
                        {
                            // Add to list to be returned 
                            Album newAlbum = Album.Builder(serverResults.Items[i], _sdkClientSettings.ServerUrl);
                            toReturn.Add(newAlbum);
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    MauiProgram.UpdateDebugMessage("Failed from HTTP Excetion! Returning from Cache.");
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
            if (_jellyfinApiClient == null ||  userDto == null || Database == null || _sdkClientSettings.ServerUrl == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            async void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                AlbumData albumFromDb = await Database.Table<AlbumData>().Where(album => album.Id == setId).FirstOrDefaultAsync();
                SongData[] songFromDb = [];
                ArtistData[] artistsFromDb = [];

                Guid[]? songIds = albumFromDb.GetSongIds();
                Guid[]? artistIds = albumFromDb.GetArtistIds();

                if (songIds != null && artistIds != null)
                {
                    Task<SongData[]> songDbQuery = Database.Table<SongData>().Where(song => songIds.Contains(song.Id)).ToArrayAsync();
                    Task<ArtistData[]> artistDbQuery = Database.Table<ArtistData>().Where(artist => artistIds.Contains(artist.Id)).ToArrayAsync();
                    
                    Task.WaitAll(songDbQuery, artistDbQuery);
                   
                    songFromDb = songDbQuery.Result;
                    artistsFromDb = artistDbQuery.Result;
                }

                toReturn = new Album(albumFromDb, songFromDb, artistsFromDb);
            }

            if (isOffline || storedAlbums.Contains(setId))
            {
                ReturnFromCache();
            }
            else
            {
                try
                {
                    Task<BaseItemDtoQueryResult?> albumQueryResult = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.Ids = [setId];
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);
                    Task<BaseItemDtoQueryResult?> songQueryResult = _jellyfinApiClient.Items.GetAsync(c =>
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
                    Task<BaseItemDtoQueryResult?> artistQueryResults = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicArtist];
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.SortOrder = [SortOrder.Ascending];
                        c.QueryParameters.ParentId = setId;
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);

                    await Task.WhenAll(albumQueryResult, songQueryResult, artistQueryResults);

                    if (albumQueryResult.Result == null || albumQueryResult.Result.Items == null) return Album.Empty;
                    if (songQueryResult.Result == null || songQueryResult.Result.Items == null) return Album.Empty;
                    if (artistQueryResults.Result == null || artistQueryResults.Result.Items == null) return Album.Empty; // TODO: Check if results are valid on next build
                    BaseItemDto? albumBaseItem = albumQueryResult.Result.Items.FirstOrDefault();
                    if (albumBaseItem == null) return Album.Empty;
                    BaseItemDto[] songBaseItemArray = songQueryResult.Result.Items.ToArray();
                    BaseItemDto[] artistBaseItemArray = artistQueryResults.Result.Items.ToArray();

                    AlbumData albumData = AlbumData.Builder(albumBaseItem, _sdkClientSettings.ServerUrl);
                    SongData[] songData = songBaseItemArray.Select(item => SongData.Builder(item, _sdkClientSettings.ServerUrl)).ToArray();
                    ArtistData[] artistData = artistBaseItemArray.Select(item => ArtistData.Builder(item, _sdkClientSettings.ServerUrl)).ToArray();

                    // ChatGPT code, fuck I dunno. If it works it works i guess idk 
                    var albumTask = Database.InsertOrReplaceAsync(albumData);
                    var songTasks = songData.Select(item => Database.InsertOrReplaceAsync(item));
                    var artistTasks = artistData.Select(item => Database.InsertOrReplaceAsync(item));
                    await Task.WhenAll(new[] { albumTask }.Concat(songTasks).Concat(artistTasks));

                    toReturn = new Album(albumData, songData, artistData);
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
        public async Task<Data.Album[]> GetSimilarAlbumsAsync(System.Guid setId, int limit = 30)
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Album> toReturn = new();
            BaseItemDtoQueryResult? result = await _jellyfinApiClient.Albums[setId].Similar.GetAsync(c =>
            {
                c.QueryParameters.UserId = userDto.Id;
                c.QueryParameters.Limit = limit;
            });
            if (result != null && result.Items != null)
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
        public async Task<Song[]> GetAllSongsAsync(int? setLimit = null, int? setStartIndex = 0, bool? setFavourites = false, ItemSortBy setSortTypes = ItemSortBy.Default, SortOrder[]? setSortOrder = null)
        {
            if (_jellyfinApiClient == null || userDto == null || _sdkClientSettings.ServerUrl == null || Database == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Song> toReturn = new();
            // Function to run that returns data from the cache
            async void ReturnFromCache()
            {
                // Filter the cache based on the provided parameters
                if (setLimit == null)
                {
                    setLimit = 50;
                }
                List<SongData> filteredCache = new();
                switch (setSortTypes)
                {
                    case ItemSortBy.DateCreated:
                        filteredCache.AddRange(await Database.Table<SongData>()
                            .OrderByDescending(song => song.DateAdded)
                            .Take((int)setLimit).ToListAsync());
                        break;
                    case ItemSortBy.Name:
                        filteredCache.AddRange(await Database.Table<SongData>()
                            .OrderByDescending(song => song.Name)
                            .Take((int)setLimit).ToListAsync());
                        break;
                    case ItemSortBy.Random:
                        filteredCache = await Database.Table<SongData>()
                            .OrderBy(song => Guid.NewGuid())
                            .Take((int)setLimit)
                            .ToListAsync();
                        break;
                    case ItemSortBy.PlayCount:
                        filteredCache.AddRange(await Database.Table<SongData>()
                            .OrderByDescending(song => song.PlayCount)
                            .Take((int)setLimit).ToListAsync());
                        break;
                    default:
                        filteredCache.AddRange(await Database.Table<SongData>()
                            .OrderByDescending(song => song.Name)
                            .Take((int)setLimit).ToListAsync());
                        break;
                }

                foreach (SongData dbItem in filteredCache)
                {
                    AlbumData albumData = new();
                    ArtistData[] artistData = [];

                    Guid albumId = dbItem.AlbumId;
                    Guid[] artistIds = dbItem.GetArtistIds();

                    Task<AlbumData> albumDbQuery = Database.Table<AlbumData>().Where(album => album.Id == albumId).FirstOrDefaultAsync();
                    Task<ArtistData[]> artistDbQuery = Database.Table<ArtistData>().Where(artist => artistIds.Contains(artist.Id)).ToArrayAsync();
                    Task.WaitAll(albumDbQuery, artistDbQuery);

                    albumData = albumDbQuery.Result;
                    artistData = artistDbQuery.Result;

                    toReturn.Add(new Song(dbItem, albumData, artistData));
                }
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
                        c.QueryParameters.SortBy = [setSortTypes];
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
        public async Task<Song> GetSongAsync(System.Guid setId)
        {
            Song toReturn = Song.Empty;
            if (_jellyfinApiClient == null || userDto == null || Database == null || _sdkClientSettings.ServerUrl == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            async void ReturnFromCache()
            {
                SongData songDbItem = await Database.Table<SongData>().Where(song => song.Id == setId).FirstOrDefaultAsync();

                AlbumData albumData = new();
                ArtistData[] artistData = [];

                Guid albumId = songDbItem.AlbumId;
                Guid[] artistIds = songDbItem.GetArtistIds();

                Task<AlbumData> albumDbQuery = Database.Table<AlbumData>().Where(album => album.Id == albumId).FirstOrDefaultAsync();
                Task<ArtistData[]> artistDbQuery = Database.Table<ArtistData>().Where(artist => artistIds.Contains(artist.Id)).ToArrayAsync();
                Task.WaitAll(albumDbQuery, artistDbQuery);

                albumData = albumDbQuery.Result;
                artistData = artistDbQuery.Result;

                toReturn = new Song(songDbItem, albumData, artistData);
            }

            if (isOffline)
            {
                ReturnFromCache();
            }
            else
            {
                try
                {
                    Task<BaseItemDtoQueryResult?> songQueryResult = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.Audio];
                        c.QueryParameters.Ids = [setId];
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);
                    Task<BaseItemDtoQueryResult?> albumQueryResults = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                        c.QueryParameters.ParentId = setId;
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);
                    Task<BaseItemDtoQueryResult?> artistResults = _jellyfinApiClient.Items.GetAsync(c =>
                    {
                        c.QueryParameters.UserId = userDto.Id;
                        c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum];
                        c.QueryParameters.ParentId = setId;
                        c.QueryParameters.Fields = [ItemFields.ParentId, ItemFields.Path, ItemFields.MediaStreams, ItemFields.CumulativeRunTimeTicks];
                        c.QueryParameters.Recursive = true;
                        c.QueryParameters.EnableImages = true;
                    }, cancellationToken: token);
                    // TODO: Check if ArtistQueryResults is returning valid data
                    await Task.WhenAll(songQueryResult, albumQueryResults, artistResults);
                    if (songQueryResult.Result == null || songQueryResult.Result.Items == null) return Song.Empty;
                    if (albumQueryResults.Result == null || albumQueryResults.Result.Items == null) return Song.Empty;
                    if (artistResults.Result == null || artistResults.Result.Items == null) return Song.Empty;
                    BaseItemDto? songResult = songQueryResult.Result.Items.FirstOrDefault();
                    BaseItemDto? albumResult = albumQueryResults.Result.Items.FirstOrDefault();
                    if (songResult == null) return Song.Empty;
                    if (albumResult == null) return Song.Empty;
                    SongData songData = SongData.Builder(songResult, _sdkClientSettings.ServerUrl);
                    AlbumData? albumData = AlbumData.Builder(albumResult, _sdkClientSettings.ServerUrl);
                    ArtistData[]? artistData = artistResults.Result.Items.Select(artist => ArtistData.Builder(artist, _sdkClientSettings.ServerUrl)).ToArray();

                    toReturn = new Song(songData, albumData, artistData);
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
        public async Task<Artist[]> GetAllArtistsAsync(int limit = 50, int? startFromIndex = 0, bool? favourites = false, ItemSortBy setSortTypes = ItemSortBy.Default)
        {
            if (_jellyfinApiClient == null || userDto == null || _sdkClientSettings.ServerUrl == null || Database == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<Artist> toReturn = new List<Artist>();

            async void ReturnFromCache()
            {
                List<ArtistData> filteredCache = new();
                switch (setSortTypes)
                {
                    case ItemSortBy.DateCreated:
                        filteredCache.AddRange(await Database.Table<ArtistData>()
                            .OrderByDescending(artist => artist.DateAdded)
                            .Take((int)limit).ToListAsync());
                        break;
                    case ItemSortBy.Name:
                        filteredCache.AddRange(await Database.Table<ArtistData>()
                            .OrderByDescending(artist => artist.Name)
                            .Take((int)limit).ToListAsync());
                        break;
                    case ItemSortBy.Random:
                        filteredCache = await Database.Table<ArtistData>()
                            .OrderBy(artist => Guid.NewGuid())
                            .Take((int)limit)
                            .ToListAsync();
                        break;
                    default:
                        filteredCache.AddRange(await Database.Table<ArtistData>()
                            .OrderByDescending(artist => artist.Name)
                            .Take((int)limit).ToListAsync());
                        break;
                }

                foreach (ArtistData dbItem in filteredCache)
                {
                    toReturn.Add(new Artist(dbItem));
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
                    BaseItemDtoQueryResult? artistResult = await _jellyfinApiClient.Items.GetAsync(c =>
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

                    foreach (var item in artistResult.Items)
                    {
                        Artist toAdd = ArtistBuilder(item);
                        toAdd.image = MusicItemImageBuilder(item);
                        toReturn.Add(toAdd);
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

        // TODO: Update to include caching
        public async Task<Artist> GetArtistAsync(System.Guid artistId)
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
                Task<BaseItemDtoQueryResult?> runArtistInfo = _jellyfinApiClient.Items.GetAsync(c =>
                {
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
                Task<BaseItemDtoQueryResult?> runAlbumInfo = _jellyfinApiClient.Items.GetAsync(c =>
                {
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
                await Task.WhenAll(runArtistInfo, runAlbumInfo);

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
                List<Data.Album> albums = new List<Data.Album>();
                foreach (var album in albumResult.Items)
                {
                    albums.Add(AlbumBuilder(album));
                }
                returnArtist.Albums = albums.ToArray();
            }

            return returnArtist;
        }

        public async Task<Artist[]> GetSimilarArtistsAsync(System.Guid setId, int limit = 30)
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
        public async Task<Playlist?> FetchPlaylistByIDAsync(System.Guid playlistId)
        {
            if (_jellyfinApiClient == null || userDto == null)
            {
                throw new InvalidOperationException("Server Connector has not been initialized! Have you called AuthenticateUserAsync?");
            }

            List<System.Guid> _filterIds = new List<System.Guid> { playlistId };

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
                if (item.Id == playlistId)
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

                List<System.Guid> artistIds = new();
                foreach (NameGuidPair artist in songItem.AlbumArtists)
                {
                    artistIds.Add((System.Guid)artist.Id);
                }

                Song newSong = Song.Builder(songItem, _sdkClientSettings.ServerUrl);

                songList.Add(newSong);
            }
            newPlaylist.songs = songList.ToArray();

            return newPlaylist;
        }
        public async Task<bool> MovePlaylistItem(System.Guid playlistId, string itemServerId, int newIndex)
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
        public async Task<bool> RemovePlaylistItem(System.Guid playlistId, string itemPlaylistId)
        {
            List<string> toRemove = new List<String> { itemPlaylistId };

            string apiPlaylistId = playlistId.ToString().Replace("-", string.Empty);

            //await _playlistsClient.RemoveFromPlaylistAsync(apiPlaylistId, toRemove);
            return true;
        }
        public async Task<bool> RemovePlaylistItem(System.Guid playlistId, string[] itemPlaylistId)
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
            if (_jellyfinApiClient != null)
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
        public async Task<bool> SessionReportUpdatePlayedItem(System.Guid itemId)
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
        public async Task<bool> SessionReportBeginPlayingItem(System.Guid itemId)
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
        public async Task<bool> SessionReportPlaybackProgress(System.Guid itemId)
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

            List<BaseItemKind> _albumItemTypes = new List<BaseItemKind> { };

            SearchHintResult? searchResult = await _jellyfinApiClient.Search.Hints.GetAsync(c =>
            {
                c.QueryParameters.UserId = userDto.Id;
                c.QueryParameters.SearchTerm = _searchTerm;
                c.QueryParameters.Limit = searchLimit;
                c.QueryParameters.IncludeItemTypes = [BaseItemKind.MusicAlbum, BaseItemKind.Audio, BaseItemKind.MusicArtist, BaseItemKind.Playlist, BaseItemKind.MusicGenre];
                c.QueryParameters.IncludeArtists = true;
                c.QueryParameters.IncludeGenres = true;
            }).ConfigureAwait(false);
            if (token.IsCancellationRequested) { cancelTokenSource = new(); return new Data.Album[0]; }

            if (searchResult.SearchHints.Count() == 0)
            {
                return new BaseMusicItem[0];
            }

            List<System.Guid?> searchedIds = new List<System.Guid?>();
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
                return new Data.Album[0];
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
        public async Task<Song[]> GetAllSongsAsync(int? limit = 50, int? startFromIndex = 0, bool? favourites = false, CancellationToken? cancellationToken = null)
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
        public async Task<bool> FavouriteItem(System.Guid id, bool setState)
        {
            if (userDto == null || _jellyfinApiClient == null)
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
        private Task<Genre> GenreBuilder(BaseItemDto baseItem)
        {
            // TODO: This is just a rehash of the AlbumBuilder to get the page I needed
            // working really quick. Ideally this'd be redone. 
            return Task.Run(() =>
            {
                Genre newGenre = new();
                newGenre.name = baseItem.Name;
                newGenre.id = (System.Guid)baseItem.Id;
                newGenre.image = MusicItemImageBuilder(baseItem);
                newGenre.serverAddress = _sdkClientSettings.ServerUrl;
                newGenre.songs = null; // TODO: Implement songs

                if (baseItem.Type != BaseItemDto_Type.MusicAlbum)
                {
                    if (baseItem.AlbumId != null)
                    {
                        newGenre.id = (System.Guid)baseItem.AlbumId;
                    }
                }

                // TODO: Implement getting album images for each genre 
                return newGenre;
            });

        }
        //private Playlist PlaylistBuilder(BaseItemDto baseItem)
        //{
        //    return PlaylistBuilder(baseItem, false).Result;
        //}
        //private Task<Playlist> PlaylistBuilder(BaseItemDto baseItem, bool fetchFullArtists)
        //{
        //    return Task.Run(() =>
        //    {
        //        Playlist newPlaylist = new();
        //        newPlaylist.name = baseItem.Name;
        //        newPlaylist.id = (System.Guid)baseItem.Id;
        //        newPlaylist.isFavourite = (bool)baseItem.UserData.IsFavorite;
        //        newPlaylist.songs = new Song[0]; // TODO: Implement songs
        //        newPlaylist.path = baseItem.Path;
        //        newPlaylist.serverAddress = _sdkClientSettings.ServerUrl;

        //        if (baseItem.Type != BaseItemDto_Type.MusicAlbum)
        //        {
        //            if (baseItem.AlbumId != null)
        //            {
        //                newPlaylist.id = (System.Guid)baseItem.AlbumId;
        //            }
        //        }

        //        newPlaylist.image = MusicItemImageBuilder(baseItem);

        //        return newPlaylist;
        //    });
        //}
    }
}