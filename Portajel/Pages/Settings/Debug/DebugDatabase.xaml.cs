using NetTopologySuite.Index.HPRtree;
using Portajel.Connections;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using System;
using System.Text.Json;
using System.Threading;

namespace Portajel.Pages.Settings.Debug;

public partial class DebugDatabase : ContentPage, IDisposable
{
    private PeriodicTimer _timer;
    private CancellationTokenSource _cancellationTokenSource;
    private ServerConnector _server = default!;
    private DatabaseConnector _database = default!;
    public DebugDatabase(IMediaServerConnector serverConnector, IDbConnector dbConnector)
    {
        _server = (ServerConnector)serverConnector;
        _database = (DatabaseConnector)dbConnector;

        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        // Start the timer when page appears
        this.Unloaded += OnUnloaded;
        await StartTimerAsync();
    }

    private void OnUnloaded(object sender, EventArgs e)
    {
        StopTimer();
    }

    private async Task StartTimerAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
            {
                await Tick();
            }
        }
        catch (OperationCanceledException)
        {
            // Timer was canceled, this is expected
        }
    }

    private void StopTimer()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task Tick()
    {
        // Implement your periodic logic here
        System.Diagnostics.Debug.WriteLine("Tick at: " + DateTime.Now);

        // Example: Update UI on main thread if needed
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            string combination = "";

            foreach (var server in _server.GetServers())
            {
                
                string srvjson = JsonSerializer.Serialize(server.Properties, new JsonSerializerOptions { WriteIndented = true });
                combination += srvjson + "\n";

                foreach (var item in server.GetDataConnectors())
                {
                    string json = JsonSerializer.Serialize(item.Value.SyncStatusInfo, new JsonSerializerOptions { WriteIndented = true });
                    System.Diagnostics.Debug.WriteLine(json);
                    combination += item.Key + "\n";
                    combination += json + "\n";
                }
            }

            ticker.Text = combination;
        });
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _timer?.Dispose();
    }
}
