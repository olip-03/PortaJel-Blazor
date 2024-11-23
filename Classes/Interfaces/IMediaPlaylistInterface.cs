namespace PortaJel_Blazor.Classes.Interfaces;

public interface IMediaPlaylistInterface
{
    Task<bool> RemovePlaylistItemAsync(Guid playlistId, Guid songId, string serverUrl = "", CancellationToken cancellationToken = default);
    Task<bool> MovePlaylistItem(Guid playlistId, Guid songId, int newIndex, string serverUrl = "", CancellationToken cancellationToken = default);
}