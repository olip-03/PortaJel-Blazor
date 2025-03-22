using System.Text.Json;
using Portajel.Connections.Services;
using Portajel.Connections.Services.Database;
using Portajel.Connections.Services.Discogs;
using Portajel.Connections.Services.FS;
using Portajel.Connections.Services.Jellyfin;
using Portajel.Connections.Services.Spotify;
using Portajel.Connections.Enum;
using Portajel.Connections.Interfaces;
using SystemEnum = System.Enum;

namespace Portajel.Connections.Data;

public class ServerConnectorSettings
{
    public ServerConnector ServerConnector { get; private init; } = new();

    public ServerConnectorSettings(string json)
    {
        // Deserialize the JSON string to populate properties
        var settings = JsonSerializer.Deserialize<List<Dictionary<string,ConnectorProperty>>>(json);
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
            
            if (SystemEnum.TryParse(setting["ConnectorType"].Value.ToString(), out MediaServerConnection parsedType))
            {
                type = parsedType;
            }

            if(type == null) continue; 
            switch (type)
            {
                case MediaServerConnection.ServerConnector:
                    connector = new ServerConnector()
                    {
                        Properties = setting
                    };
                    break;
                case MediaServerConnection.Database:
                    connector = new DatabaseConnector()
                    {
                        Properties = setting
                    };
                    break;
                case MediaServerConnection.Filesystem:
                    connector = new FileSystemConnector()
                    {
                        Properties = setting
                    };
                    break;
                case MediaServerConnection.Jellyfin:
                    connector = new JellyfinServerConnector()
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
            ServerConnector.AddServer(connector);
        }
    }

    public ServerConnectorSettings(ServerConnector serverConnector, IMediaServerConnector[] servers)
    {
        ServerConnector = serverConnector;
        if(ServerConnector.GetServers().Length > 0) return;
        if(servers == null) return;
        foreach (var srv in servers)
        {
            ServerConnector.AddServer(srv);
        }
    }

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        foreach (var server in ServerConnector.GetServers())
        {
            if (!server.Properties.TryAdd("ConnectorType", new ConnectorProperty(
                    label: "ConnectorType",
                    description: "The Connection Type of this server.",
                    value: server.GetConnectionType(),
                    protectValue: false)
                ))
            {
                server.Properties["ConnectorType"].Value = server.GetConnectionType();
            }
        }
        return JsonSerializer.Serialize(ServerConnector.GetServers().Select(s => s.Properties), options);
    }
}