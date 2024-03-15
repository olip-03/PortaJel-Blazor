using Jellyfin.Sdk;
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
        private UserDto userDto = null;

        private SdkClientSettings _sdkClientSettings = null;
        private ArtistsClient _artistsClient;
        private ItemsClient _itemsClient;
        private ItemLookupClient _itemLookupClient;
        private ItemUpdateClient _itemUpdateClient;
        private PlaylistsClient _playlistsClient;
        private PlaylistCreationResult _playlistCreationResult;
        private MediaInfoClient _mediaInfoClient;
        private UserLibraryClient _userLibraryClient; 
        private ImageClient _imageClient;
        private MusicGenresClient _genresClient;
        private SearchClient _searchClient;

        private ISystemClient _systemClient;
        private IUserClient _userClient;

        private IUserViewsClient _userViewsClient;

        private string Username = String.Empty;
        private string StoredPassword = String.Empty;

        private int TotalAlbumRecordCount = -1;
        private int TotalPlaylistRecordCount = -1;
        private int TotalArtistRecordCount = -1;
        private int TotalSongRecordCount = -1;
        private int TotalGenreRecordCount = -1;

        public bool isOffline = false;
        public bool isRunningTask = false;
        public CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private CancellationToken token;

        HttpClient _httpClient;
        public ServerConnecter()
        {
            _sdkClientSettings = new();
            _httpClient = new HttpClient();

            _sdkClientSettings.DeviceName = Microsoft.Maui.Devices.DeviceInfo.Current.Name;
            _sdkClientSettings.DeviceId = Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString();
            _sdkClientSettings.ClientName = MauiProgram.applicationName;
            _sdkClientSettings.ClientVersion = MauiProgram.applicationClientVersion;

            token = cancelTokenSource.Token;
        }
        public async Task<bool> AuthenticateAddressAsync(string address)
        {
            _sdkClientSettings.BaseUrl = address;

            _systemClient = new SystemClient(_sdkClientSettings, _httpClient);
            try
            {
                // Get public system info to verify that the url points to a Jellyfin server.
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
                var systemInfo = await _systemClient.GetPublicSystemInfoAsync(token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                {
                    cancelTokenSource = new();
                    return false;
                }
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
                    Pw = password,
                }, token).ConfigureAwait(false);

                _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
                userDto = authenticationResult.User;

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

                _playlistCreationResult = new();
                Username = username;
                StoredPassword = password;
                validUser = true;

                if (token.IsCancellationRequested)
                {
                    cancelTokenSource = new();
                    return false;
                }
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
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true, isFavorite: _isFavourite, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return new Album[0];
            }

            List<Album> albums = new List<Album>();
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
        public async Task<BaseMusicItem[]> FetchFavouritesAddedAsync(int? _startIndex = null, int? _limit = null)
        {
            // Create a list containing only the "Album" item type
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum, BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "PlayCount" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult songResult;
            // Call GetItemsAsync with the specified parameters
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true, isFavorite: true, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return new BaseMusicItem[0];
            }

            List<BaseMusicItem> musicItems = new List<BaseMusicItem>();
            foreach (var item in songResult.Items)
            {
                switch (item.Type)
                {
                    case BaseItemKind.MusicAlbum:
                        musicItems.Add(AlbumBuilder(item));
                        break;
                    case BaseItemKind.Audio:
                        musicItems.Add(SongBuilder(item));
                        break;
                    case BaseItemKind.Playlist:
                        musicItems.Add(PlaylistBuilder(item));
                        break;
                }

            }
            return musicItems.ToArray();
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
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, filters: _itemFilter, recursive: true, enableImages: true, isFavorite: _isFavourite, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return new Album[0];
            }

            List<Album> albums = new List<Album>();
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
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.Audio };
            List<String> _sortTypes = new List<string> { "PlayCount" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };
            List<ItemFilter> _itemFilter = new List<ItemFilter> { ItemFilter.IsPlayed };

            BaseItemDtoQueryResult songResult;
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, filters: _itemFilter, recursive: true, enableImages: true, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return new Album[0];
            }

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
        public async Task<Album[]> FetchRandomAsync(int? _startIndex = null, int? _limit = null)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "Random" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Descending };

            BaseItemDtoQueryResult songResult;
            try
            {
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: _limit, recursive: true, enableImages: true, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (songResult == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return new Album[0];
            }

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
        public async Task<Album> FetchAlbumByIDAsync(Guid albumId, bool? fetchFullArtist = false)
        {
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
                albumResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, ids: _filterIds, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _albumItemTypes, recursive: true, enableImages: true, cancellationToken: token);
                songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _songItemTypes, parentId: albumId, recursive: true, enableImages: true, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Album.Empty;
            }

            // Null checking
            if (albumResult.Items.FirstOrDefault() == null || token.IsCancellationRequested)
            {
                cancelTokenSource = new();
                return Album.Empty;
            }

            Album getAlbum = Album.Empty;
            BaseItemDto albumResultItem = new();
            BaseItemDto? result = albumResult.Items.FirstOrDefault();
            if (result != null)  { albumResultItem = result; }

            getAlbum = await AlbumBuilder(albumResultItem, true);

            // Set Song information
            List<Song> songList = new List<Song>();
            foreach (var song in songResult.Items)
            {
                // Create object
                Song newSong = SongBuilder(song);
                newSong.album = getAlbum;

                List<Artist> partialArtistList = new List<Artist>();
                foreach (NameGuidPair partialArtist in song.ArtistItems)
                {
                    // TOOD: Check if we already have a complete record in the cached data
                    partialArtistList.Add(ArtistBuilder(partialArtist));
                }
                newSong.artists = partialArtistList.ToArray();

                // Add to song dictionary
                if (!MauiProgram.songDictionary.ContainsKey(song.Id))
                {
                    MauiProgram.songDictionary.Add(song.Id, newSong);
                }
                songList.Add(newSong);
            }
            getAlbum.songs = songList.ToArray();

            return getAlbum;
        }
        public async Task<Playlist[]> GetPlaylistsAsycn(int? limit = 50, int? startFromIndex = 0)
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
        public async Task<Album[]> GetAllAlbumsAsync(int? limit = 50, int? startFromIndex = 0, bool? favourites = false, CancellationToken? cancellationToken = null)
        {
            List<BaseItemKind> _includeItemTypes = new List<BaseItemKind> { BaseItemKind.MusicAlbum };
            List<String> _sortTypes = new List<string> { "SortName" };
            List<SortOrder> _sortOrder = new List<SortOrder> { SortOrder.Ascending };

            CancellationToken ctoken = new();
            if(cancellationToken != null) { ctoken = (CancellationToken)cancellationToken; }

            BaseItemDtoQueryResult songResult = new BaseItemDtoQueryResult();
            // Call GetItemsAsync with the specified parameters
            try
            {  
                if(favourites == true)
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, isFavorite: true, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: ctoken);
                }
                else
                {
                    songResult = await _itemsClient.GetItemsAsync(userId: userDto.Id, sortBy: _sortTypes, sortOrder: _sortOrder, includeItemTypes: _includeItemTypes, limit: limit, startIndex: startFromIndex, recursive: true, enableImages: true, enableTotalRecordCount: true, cancellationToken: ctoken);
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

            if (songResult == null || ctoken.IsCancellationRequested) { cancelTokenSource = new(); return new Album[0]; }
            if (songResult.Items == null || ctoken.IsCancellationRequested) { cancelTokenSource = new(); return new Album[0]; }

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
                recordCount = await _itemsClient.GetItemsAsync(userId: userDto.Id, includeItemTypes: _includeItemTypes, recursive: true, enableImages: false, enableTotalRecordCount: true, cancellationToken: token);

                if (recordCount == null || token.IsCancellationRequested)
                {
                    return -1;
                }

                return recordCount.TotalRecordCount;
            }
            return TotalArtistRecordCount;
        }
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
                return Album.Empty;
            }

            Album newAlbum = new();
            newAlbum.name = baseItem.Name;
            newAlbum.id = baseItem.Id;
            newAlbum.songs = null; // TODO: Implement songs
            newAlbum.image = MusicItemImageBuilder(baseItem);

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

            newArtist.backgroundImage = MusicItemImageBuilder(baseItem);
            newArtist.logoImage = MusicItemImageBuilder(baseItem);

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
        private MusicItemImage MusicItemImageBuilder(BaseItemDto baseItem)
        {
            MusicItemImage image = new();
            image.musicItemImageType = MusicItemImage.MusicItemImageType.url;
            
            if(baseItem.Type == BaseItemKind.MusicAlbum)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Primary?format=jpg";
            }
            else if(baseItem.Type == BaseItemKind.Playlist)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Primary?format=jpg";
                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.Type == BaseItemKind.Audio)
            {
                if(baseItem.AlbumId != null)
                {
                    image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.AlbumId + "/Images/Primary?format=jpg";
                }
                else
                {
                    image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Primary?format=jpg";
                }
                // image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if(baseItem.Type == BaseItemKind.MusicGenre && baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id + "/Images/Primary?format=jpg";
                image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.ImageBlurHashes.Primary != null && baseItem.AlbumId != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.AlbumId + "/Images/Primary?format=jpg";
                image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.ImageBlurHashes.Primary != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.Id.ToString() + "/Images/Primary?format=jpg";
                image.blurHash = baseItem.ImageBlurHashes.Primary.FirstOrDefault().Value;
            }
            else if (baseItem.ArtistItems != null)
            {
                image.source = _sdkClientSettings.BaseUrl + "/Items/" + baseItem.ArtistItems.First().Id + "/Images/Primary?format=jpg";
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
            newSong.image = MusicItemImageBuilder(baseItem);
            return newSong;
        }
    }
}