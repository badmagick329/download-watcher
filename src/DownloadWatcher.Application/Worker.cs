using System.Diagnostics;
using DownloadWatcher.Core;

namespace DownloadWatcher.Application;

public class Worker
{
    private readonly CancellationToken _cancelToken;
    private readonly PriorityQueue<ScheduledTask, DateTime> _taskQueue = new();
    private readonly SemaphoreSlim _queueSignal = new(0);
    private Task? _currentTask;
    private bool _isShuttingDown;


    public Worker(CancellationToken cancelToken)
    {
        _cancelToken = cancelToken;
    }

    public void AddTask(ScheduledTask scheduledTask)
    {
        if (_isShuttingDown)
        {
            Console.WriteLine("Worker is shutting down. Task will not be added.");
            ReportThread("Add task");
            return;
        }

        lock (_taskQueue)
        {
            _taskQueue.Enqueue(scheduledTask, scheduledTask.ScheduledTime);
        }

        _queueSignal.Release();
    }

    public void Start()
    {
        Console.WriteLine("Starting Worker...");
        Task.Run(Work);
        ReportThread("Worker start method");
        Console.WriteLine("Worker started.");
    }

    public async Task Shutdown()
    {
        Console.WriteLine("Shutdown initiated. No more events will be added.");
        _isShuttingDown = true;
        if (_currentTask is not null)
        {
            await _currentTask;
        }
    }


    private async Task Work()
    {
        ReportThread("Worker loop method");
        try
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                ScheduledTask? nextTask = null;
                TimeSpan? delay = null;

                lock (_taskQueue)
                {
                    if (_taskQueue.Count > 0)
                    {
                        var nextScheduledTime = _taskQueue.Peek().ScheduledTime;
                        if (nextScheduledTime <= DateTime.UtcNow)
                        {
                            nextTask = _taskQueue.Dequeue();
                        }
                        else
                        {
                            delay = nextScheduledTime - DateTime.UtcNow;
                        }
                    }
                }

                if (nextTask != null)
                {
                    try
                    {
                        Console.WriteLine($"Executing task at {DateTime.UtcNow}");
                        ReportThread("Task execution");
                        Debug.Assert(_currentTask == null, "_currentTask == null");
                        _currentTask = nextTask.TaskAction();
                        await _currentTask;
                        Console.WriteLine("Task completed.");
                        _currentTask = null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Task failed: {e.Message}");
                        ReportThread("Task failure");
                        if (nextTask.RetryCount > 0)
                        {
                            nextTask.RetryCount--;
                            nextTask.ScheduledTime = DateTime.UtcNow + nextTask.RetryInterval;
                            lock (_taskQueue)
                            {
                                _taskQueue.Enqueue(nextTask, nextTask.ScheduledTime);
                            }

                            _queueSignal.Release();
                            Console.WriteLine($"Task rescheduled for {nextTask.ScheduledTime}");
                        }
                        else
                        {
                            Console.WriteLine("Task retries exhausted.");
                        }
                    }
                }
                else if (delay.HasValue)
                {
                    await Task.Delay(delay.Value, _cancelToken);
                }
                else
                {
                    await _queueSignal.WaitAsync(_cancelToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            ReportThread("Worker cancelled");
        }
        finally
        {
            ReportThread("Worker exit");
        }

        ReportThread("After worker exit");
    }

    public static void ReportThread(string source)
    {
        Console.WriteLine($"{source} running in thread: {Environment.CurrentManagedThreadId}");
    }
}