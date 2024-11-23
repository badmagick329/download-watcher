using System.Diagnostics;
using DownloadWatcher.Core;
using Serilog;

namespace DownloadWatcher.Application;

class Worker
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
            Log.Debug("Worker is shutting down. Task will not be added.");
            return;
        }

        lock (_taskQueue)
        {
            _taskQueue.Enqueue(scheduledTask, scheduledTask.ScheduledTime);
            ReportTaskQueue();
        }

        _queueSignal.Release();
    }

    public void Start()
    {
        Log.Debug("Starting Worker...");
        Task.Run(Work);
        Log.Debug("Worker started.");
    }

    public async Task Shutdown()
    {
        Log.Debug("Shutdown initiated. No more events will be added.");
        _isShuttingDown = true;
        if (_currentTask is not null)
        {
            Log.Information("Waiting for current task to complete.");
            await _currentTask;
        }
    }

    private async Task Work()
    {
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
                            ReportTaskQueue();
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
                        Log.Debug($"Executing task at {DateTime.UtcNow}");
                        Debug.Assert(_currentTask == null, "_currentTask == null");
                        _currentTask = nextTask.TaskAction();
                        await _currentTask;
                        Log.Debug("Task completed.");
                        _currentTask = null;
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"Task failed: {e.Message}");
                        if (nextTask.RetryCount > 0)
                        {
                            nextTask.RetryCount--;
                            nextTask.ScheduledTime = DateTime.UtcNow + nextTask.RetryInterval;
                            lock (_taskQueue)
                            {
                                _taskQueue.Enqueue(nextTask, nextTask.ScheduledTime);
                                ReportTaskQueue();
                            }

                            _queueSignal.Release();
                            Log.Debug($"Task rescheduled for {nextTask.ScheduledTime}");
                        }
                        else
                        {
                            Log.Debug($"Task retries exhausted.");
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
        catch (OperationCanceledException) { }
    }

    private void ReportTaskQueue()
    {
        Log.Debug($"Task queue count: {_taskQueue.Count}");
        if (_taskQueue.Count > 1000)
        {
            Log.Warning("Task queue count is over 1000");
        }
    }
}
