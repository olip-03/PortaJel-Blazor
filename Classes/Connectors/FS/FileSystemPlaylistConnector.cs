using PortaJel_Blazor.Classes.Data;
using PortaJel_Blazor.Classes.Interfaces;
using PortaJel_Blazor.Classes.Enum;

namespace PortaJel_Blazor.Classes.Connectors.FS;

public class FileSystemPlaylistConnector : IMediaServerPlaylistConnector
{
    public Task<Playlist[]> GetAllPlaylistsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Playlist> GetPlaylistAsync(int playlistId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalPlaylistCountAsync()
    {
        throw new NotImplementedException();
    }
}