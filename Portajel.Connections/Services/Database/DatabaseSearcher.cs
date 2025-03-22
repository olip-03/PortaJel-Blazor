using System.Collections.Concurrent;
using Portajel.Connections.Data;

namespace Portajel.Connections.Services.Database;

public class DatabaseSearcher(int debounceDelay = 300)
{
    private readonly ConcurrentQueue<string> _searchQueue = new(); // Queue to store search queries
    private readonly SemaphoreSlim _semaphore = new(1, 1); // Semaphore to handle a single active search
    private CancellationTokenSource _cts; // Token to cancel debounced searches
    public bool IsSearching { get; private set; } = false;
    // Delay in milliseconds

    public event Action<BaseMusicItem[]> SearchUpdated; // Event to update UI with search results

    public void QueueSearch(string query)
    {
        _searchQueue.Enqueue(query);
        ProcessQueueAsync();
    }
    
    private async void ProcessQueueAsync()
    {
        // Ensure only one task is processing the queue
        await _semaphore.WaitAsync();
        try
        {
            IsSearching = true;
            while (_searchQueue.Count > 0)
            {
                _cts?.Cancel(); // Cancel any ongoing debounce
                _cts = new CancellationTokenSource();

                string currentQuery = string.Empty;

                // Retrieve the last query in the queue, skipping all intermediate ones
                while (_searchQueue.TryDequeue(out var dequeuedQuery))
                {
                    currentQuery = dequeuedQuery;
                }

                try
                {
                    // Debounce before starting the search
                    await Task.Delay(debounceDelay, _cts.Token);

                    // Perform the search for the last query
                    var results = await PerformSearchAsync(currentQuery);

                    // Notify the UI or any listeners with the updated results
                    if (_searchQueue.Count == 0)
                    {
                        IsSearching = false;
                    }
                    SearchUpdated?.Invoke(results);
                }
                catch (TaskCanceledException)
                {
                    // Search was canceled; loop will check for new queries
                }
            }
            IsSearching = false;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private async Task<BaseMusicItem[]> PerformSearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }
        
        var searchResults = await MauiProgram.Database.SearchAsync(query, limit: 50);
        
        return searchResults; // Replace this with actual search results
    }
}