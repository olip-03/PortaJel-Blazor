using System.Text.Json;
using PortaJel_Blazor.Classes.Connectors;
using PortaJel_Blazor.Classes.Interfaces;

namespace PortaJel_Blazor.Classes.Data;

public class ServerConnectorSettings
{
    public ServerConnector ServerConnector { get; private init; }
    public IMediaServerConnector[] Servers { get; private init; } 

    public ServerConnectorSettings(string json)
    {
        // Deserialize the JSON string to populate properties
        var settings = JsonSerializer.Deserialize<ServerConnectorSettings>(json);
        if (settings == null) return;
        ServerConnector = settings.ServerConnector;
        Servers = settings.Servers;
    }

    public ServerConnectorSettings(ServerConnector serverConnector, IMediaServerConnector[] servers)
    {
        ServerConnector = serverConnector;
        Servers = servers;
    }
    
    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        return JsonSerializer.Serialize(this, options);
    }
}