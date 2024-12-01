using PortaJel_Blazor.Classes.Data;

namespace PortaJel_Blazor.Classes.Interfaces;

public interface IMediaPlaylistInterface
{
    Task<bool> AddAsync(Guid playlistId, BaseMusicItem musicItem, CancellationToken cancellationToken = default);
    Task<bool> AddRangeAsync(Guid playlistId, BaseMusicItem[] musicItems, CancellationToken cancellationToken = default);
    Task<bool> RemovePlaylistItemAsync(Guid playlistId, Guid songId, string serverUrl = "", CancellationToken cancellationToken = default);
    Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex, string serverUrl = "", CancellationToken cancellationToken = default);
}