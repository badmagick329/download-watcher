using System.Collections.Concurrent;

namespace DownloadWatcher.Application;

public class Worker
{
    private readonly CancellationToken _cancelToken;
    private readonly ConcurrentQueue<string> _eventQueue = new();
    private readonly SemaphoreSlim _queueSignal = new(0);
    private bool _isShuttingDown;
    private Task? _currentTask;

    public Worker(CancellationToken cancelToken)
    {
        _cancelToken = cancelToken;
    }

    public void AddEvent(string eventString)
    {
        if (_isShuttingDown)
        {
            Console.WriteLine("Shutdown in progress. No more events will be added.");
            return;
        }

        _eventQueue.Enqueue(eventString);
        Console.WriteLine($"Added: {eventString} to EventQueue");
        _queueSignal.Release();
    }

    public void Start()
    {
        Console.WriteLine("Starting Worker...");
        _currentTask = Work();
        Console.WriteLine($"Got current task: {_currentTask}");
        Console.WriteLine("Worker started.");
    }

    public async Task Shutdown()
    {
        Console.WriteLine("Shutdown initiated. No more events will be added.");
        _isShuttingDown = true;
        _queueSignal.Release();
        if (_currentTask is not null)
        {
            await _currentTask;
        }
    }


    private async Task Work()
    {
        Console.WriteLine("Inside work");
        try
        {
            Console.WriteLine("Entering work loop");
            while (!_cancelToken.IsCancellationRequested || !_eventQueue.IsEmpty || !_isShuttingDown)
            {
                await _queueSignal.WaitAsync(_cancelToken);
                while (_eventQueue.TryDequeue(out var eventItem))
                {
                    Console.WriteLine($"Processing: {eventItem}");
                    // Simulate processing
                    await Task.Delay(1000, _cancelToken);
                    Console.WriteLine($"Done Processing: {eventItem}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Worker cancelled.");
        }
        finally
        {
            Console.WriteLine("Worker exiting.");
        }

        Console.WriteLine("Worker has exited work loop");
        _currentTask = null;
    }
}