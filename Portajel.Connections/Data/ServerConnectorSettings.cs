using System.Text.Json;
using Portajel.Connections.Services;
using Portajel.Connections.Services.Discogs;
using Portajel.Connections.Services.FS;
using Portajel.Connections.Services.Jellyfin;
using Portajel.Connections.Services.Spotify;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Connections;

namespace Portajel.Services;

public class ServerConnectorSettings
{
    public ServerConnector ServerConnector { get; private init; } = new();

    public ServerConnectorSettings(string json, IDbConnector database, string appDataDirectory)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            // Return empty list
            return;
        }

        var options = new JsonSerializerOptions
        {
            IncludeFields = true
        };
        var settings = JsonSerializer.Deserialize<List<Dictionary<string, ConnectorProperty>>>(json, options);
        if (settings == null) return;
        foreach (var setting in settings)
        {
            MediaServerConnection? type = null;
            IMediaServerConnector connector = null;

            foreach (var variable in setting)
            {
                if (variable.Value.Value is JsonElement jsonElement)
                {
                    switch (jsonElement.ValueKind)
                    {
                        case JsonValueKind.String:
                            variable.Value.Value = jsonElement.GetString();
                            break;
                        case JsonValueKind.Number:
                            variable.Value.Value = jsonElement.GetInt32();
                            break;
                        case JsonValueKind.True:
                            variable.Value.Value = true;
                            break;
                        case JsonValueKind.False:
                            variable.Value.Value = false;
                            break;
                        default:
                            variable.Value.Value = null;
                            break;
                    }
                }
            }
            
            if (Enum.TryParse(setting["ConnectorType"].Value.ToString(), out MediaServerConnection parsedType))
            {
                type = parsedType;
            }

            if (type == null) continue; 
            switch (type)
            {
                case MediaServerConnection.Filesystem:
                    connector = new FileSystemConnector()
                    {
                        Properties = setting
                    };
                    break;
                case MediaServerConnection.Jellyfin:
                    connector = new JellyfinServerConnector(database)
                    {
                        Properties = setting
                    };
                    break;
                case MediaServerConnection.Spotify:
                    connector = new SpotifyServerConnector()
                    {
                        Properties = setting
                    };
                    break;
                case MediaServerConnection.Discogs:
                    connector = new DiscogsConnector()
                    {
                        Properties = setting
                    };
                    break; 
            }
            if(connector == null) continue; 
            ServerConnector.Servers.Add(connector);
        }
    }

    public ServerConnectorSettings(ServerConnector serverConnector, IMediaServerConnector[] servers)
    {
        ServerConnector = serverConnector;
        if(ServerConnector.Servers.Count > 0) return;
        if(servers == null) return;
        foreach (var srv in servers)
        {
            ServerConnector.Servers.Add(srv);
        }
    }

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        foreach (var server in ServerConnector.Servers)
        {
            if (!server.Properties.TryAdd("ConnectorType", new ConnectorProperty(
                    label: "ConnectorType",
                    description: "The Connection Type of this server.",
                    value: server.GetConnectionType(),
                    protectValue: false,
                    userVisible: false)
                ))
            {
                server.Properties["ConnectorType"].Value = server.GetConnectionType();
            }
        }
        return JsonSerializer.Serialize(ServerConnector.Servers.Select(s => s.Properties), options);
    }
}