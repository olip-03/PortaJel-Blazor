using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Portajel.Connections.Database;
using SQLite;

namespace Portajel.Connections
{
    public class ServerReporter
    {
        private readonly JellyfinApiClient _jellyfinApiClient;
        private readonly JellyfinSdkSettings _sdkClientSettings = new();
        private readonly string sessionId;
        private readonly Guid _userId;

        private SQLiteAsyncConnection _database = null;
        private const SQLiteOpenFlags DbFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public ServerReporter(string baseurl, string userid, string sessionid, string accessToken) 
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddHttpClient("Default", c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue(
                        MauiProgram.ApplicationName,
                        MauiProgram.ApplicationClientVersion));
                //c.Timeout = TimeSpan.FromSeconds(2);
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

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            _jellyfinApiClient = serviceProvider.GetRequiredService<JellyfinApiClient>();
            _sdkClientSettings = serviceProvider.GetRequiredService<JellyfinSdkSettings>();
            _sdkClientSettings.SetServerUrl(baseurl);
            _sdkClientSettings.SetAccessToken(accessToken);
            _sdkClientSettings.Initialize(
                MauiProgram.ApplicationName,
                MauiProgram.ApplicationClientVersion,
                Microsoft.Maui.Devices.DeviceInfo.Current.Name,
                Microsoft.Maui.Devices.DeviceInfo.Current.Idiom.ToString());
            sessionId = sessionid;
            _userId = Guid.Parse(userid);

            InitDb(baseurl);
        }

        private async void InitDb(string baseUrl)
        {
            System.Guid? result = null;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(baseUrl));
                result = new System.Guid(hash);
            }
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, $"{result}.db");
            MauiProgram.UpdateDebugMessage($"Db initalized at {filePath}");
            _database = new SQLiteAsyncConnection(filePath, DbFlags);
            await _database.CreateTableAsync<AlbumData>();
            await _database.CreateTableAsync<SongData>();
            await _database.CreateTableAsync<ArtistData>();
            await _database.CreateTableAsync<PlaylistData>();
        }
		
        public void SetVerboseReporting(bool verbose)
        {

        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReportPlayingPing()
        {
            try
            {
                if (_jellyfinApiClient != null)
                {
                    await _jellyfinApiClient.Sessions.Playing.Ping.PostAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: Cannot Ping {_sdkClientSettings.ServerUrl}: {ex.Message}");
            }

            return false;
        }
		
        /// <summary>
        /// Reports playback progress within the current session to the Jellyfin API.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method sends playback progress information to the Jellyfin API if the API client and user DTO are not null. 
        /// It creates a new instance of <see cref="PlaybackStartInfo"/> to provide the required progress details.
        /// </remarks>
        public async Task<bool> ReportPlayingProgress(Guid itemId, long positionTicks)
        {
            try
            {
                if (_jellyfinApiClient != null)
                {
                    // Ensure the PlaybackStartInfo instance is properly populated
                    var playbackProgressInfo = new PlaybackProgressInfo
                    {
                        ItemId = itemId, // Example item ID, replace with actual data
                        SessionId = sessionId, // Example session ID, replace with actual data
                        PositionTicks = positionTicks // Example position in ticks, replace with actual progress
                    };

                    Task dbReport = new Task(async () =>
                    {
                        try
                        {
                            var song = await _database.Table<SongData>().Where(a => a.Id == itemId).FirstOrDefaultAsync();
                            var album = await _database.Table<AlbumData>().Where(a => a.Id == song.AlbumId).FirstOrDefaultAsync();

                            song.DatePlayed = DateTimeOffset.Now;
                            album.DatePlayed = DateTimeOffset.Now;

                            await _database.UpdateAsync(song);
                            await _database.UpdateAsync(album);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"ERROR: Cannot update Playback Progress in {_sdkClientSettings.ServerUrl} db!: {ex.Message}");
                        }
                    });

                    await Task.WhenAll(
                        dbReport,
                        _jellyfinApiClient.Sessions.Playing.Progress.PostAsync(playbackProgressInfo),
                        _jellyfinApiClient.UserPlayedItems[itemId].PostAsync(c =>
                        {
                            c.QueryParameters.UserId = _userId;
                            c.QueryParameters.DatePlayed = DateTime.Now;
                        })).ConfigureAwait(false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: Cannot report Playback Progress to {_sdkClientSettings.ServerUrl}: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReportPlayingStopped(Guid itemId, long positionTicks)
        {
            try
            {
                if (_jellyfinApiClient != null)
                {
                    var playbackStopInfo = new PlaybackStopInfo
                    {
                        Failed = false,  // Set to true if there was a failure
                                         //Item = new BaseItemDto
                                         //{
                                         //    // Populate necessary fields for the Item
                                         //},
                        ItemId = itemId,  // Replace with actual ItemId
                                          // LiveStreamId = "someLiveStreamId",  // Replace with actual LiveStreamId
                                          // MediaSourceId = "someMediaSourceId",  // Replace with actual MediaSourceId
                                          // NextMediaType = "video",  // Replace with actual NextMediaType
                                          // NowPlayingQueue = new List<QueueItem>
                                          // {
                                          //     // Populate the queue with actual items
                                          // },
                                          //PlaylistItemId = "somePlaylistItemId",  // Replace with actual PlaylistItemId
                                          //PlaySessionId = "somePlaySessionId",  // Replace with actual PlaySessionId
                        PositionTicks = positionTicks,  // Replace with actual position in ticks
                        SessionId = sessionId  // Replace with actual SessionId
                    };

                    await _jellyfinApiClient.Sessions.Playing.Stopped.PostAsync(playbackStopInfo).ConfigureAwait(false);
                    return true; // Return true if the API call succeeds
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: Cannot report Playback Stopped to {_sdkClientSettings.ServerUrl}: {ex.Message}");
            }

            return false; // Return false if any conditions fail
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public async Task<bool> ReportViewing(Guid itemId)
        {
            try
            {
                if (_jellyfinApiClient != null)
                {
                    // this is fucked I get why people don't like this langauge
                    var requestConfig = new Action<RequestConfiguration<global::Jellyfin.Sdk.Generated.Sessions.Viewing.ViewingRequestBuilder.ViewingRequestBuilderPostQueryParameters>>(config =>
                    {
                        config.QueryParameters = new global::Jellyfin.Sdk.Generated.Sessions.Viewing.ViewingRequestBuilder.ViewingRequestBuilderPostQueryParameters
                        {
                            ItemId = itemId.ToString(),
                            SessionId = sessionId
                        };
                    });

                    // Post the request to report viewing
                    await _jellyfinApiClient.Sessions.Viewing.PostAsync(requestConfig).ConfigureAwait(false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: Cannot report Viewing to {_sdkClientSettings.ServerUrl}: {ex.Message}");
            }

            return false; // Return false if any conditions fail
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Logout()
        {
            try
            {
                if (_jellyfinApiClient != null)
                {
                    await _jellyfinApiClient.Sessions.Logout.PostAsync();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: Cannot log-out from {_sdkClientSettings.ServerUrl}: {ex.Message}");
            }

            return false;
        }
    }
}
