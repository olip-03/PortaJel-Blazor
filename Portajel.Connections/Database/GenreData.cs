using Jellyfin.Sdk.Generated.Models;
using SQLite;
using System.Text.Json;
using Portajel.Connections.Data;

namespace Portajel.Connections.Database;

public class GenreData
{
    [PrimaryKey, NotNull, AutoIncrement]
    public int LocalId { get; set; }
    public Guid Id { get; set; }
    public string ServerAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset DateAdded { get; set; }
    public string AlbumIdsJson { get; set;} = string.Empty;
    public bool IsPartial { get; set; } = true;

    public Guid[] GetAlbumIds()
    {
        return [];
    }
}