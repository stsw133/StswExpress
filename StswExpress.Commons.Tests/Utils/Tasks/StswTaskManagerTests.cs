namespace StswExpress.Commons.Tests.Utils.Tasks;
public class StswTaskManagerTests
{
    [Fact]
    public void Add_AddsTaskFactoryWithoutToken()
    {
        var manager = new StswTaskManager<int>();
        manager.Add(async () => await Task.FromResult(1));
        Assert.Single(manager.Tasks);
        Assert.IsType<StswTask<int>>(manager.Tasks.First());
    }

    [Fact]
    public void Add_AddsTaskFactoryWithToken()
    {
        var manager = new StswTaskManager<int>();
        manager.Add(async ct => await Task.FromResult(2));
        Assert.Single(manager.Tasks);
        Assert.IsType<StswTask<int>>(manager.Tasks.First());
    }

    [Fact]
    public void AddRange_AddsMultipleTaskFactoriesWithoutToken()
    {
        var manager = new StswTaskManager<int>();
        var factories = new List<Func<Task<int>>>
        {
            async () => await Task.FromResult(1),
            async () => await Task.FromResult(2)
        };

        foreach (var factory in factories)
            manager.Add(factory);

        Assert.Equal(2, manager.Tasks.Count);
    }

    [Fact]
    public void AddRange_AddsMultipleTaskFactoriesWithToken()
    {
        var manager = new StswTaskManager<int>();
        var factories = new List<Func<CancellationToken, Task<int>>>
        {
            async ct => await Task.FromResult(3),
            async ct => await Task.FromResult(4)
        };

        foreach (var factory in factories)
            manager.Add(factory);

        Assert.Equal(2, manager.Tasks.Count);
    }

    [Fact]
    public async Task RunAllQueuedAsync_ExecutesAllPendingTasks_Sequentially()
    {
        var manager = new StswTaskManager<int>();
        int sum = 0;
        manager.Add(async () => { await Task.Delay(10); sum += 1; return 1; });
        manager.Add(async () => { await Task.Delay(10); sum += 2; return 2; });

        await manager.RunAllQueuedAsync(continueOnError: false);

        Assert.Equal(3, sum);
        Assert.All(manager.Tasks, t => Assert.Equal(StswTaskStatus.Completed, t.Status));
    }

    [Fact]
    public async Task RunAllQueuedAsync_ContinuesOnError_WhenContinueOnErrorTrue()
    {
        var manager = new StswTaskManager<int>();
        manager.Add(async () => { await Task.Delay(10); throw new Exception("fail"); });
        manager.Add(async () => { await Task.Delay(10); return 5; });

        await manager.RunAllQueuedAsync(continueOnError: true);

        Assert.Equal(StswTaskStatus.Faulted, manager.Tasks[0].Status);
        Assert.Equal(StswTaskStatus.Completed, manager.Tasks[1].Status);
    }

    [Fact]
    public async Task RunAllParallelAsync_ExecutesAllPendingTasks_InParallel()
    {
        var manager = new StswTaskManager<int>();
        var started = new List<int>();
        manager.Add(async () => { started.Add(1); await Task.Delay(20); return 1; });
        manager.Add(async () => { started.Add(2); await Task.Delay(20); return 2; });

        await manager.RunAllParallelAsync(continueOnError: false);

        Assert.Contains(1, started);
        Assert.Contains(2, started);
        Assert.All(manager.Tasks, t => Assert.Equal(StswTaskStatus.Completed, t.Status));
    }

    [Fact]
    public async Task RunAllParallelAsync_ContinuesOnError_WhenContinueOnErrorTrue()
    {
        var manager = new StswTaskManager<int>();
        manager.Add(async () => { await Task.Delay(10); throw new Exception("fail"); });
        manager.Add(async () => { await Task.Delay(10); return 7; });

        await manager.RunAllParallelAsync(continueOnError: true);

        Assert.Equal(StswTaskStatus.Faulted, manager.Tasks[0].Status);
        Assert.Equal(StswTaskStatus.Completed, manager.Tasks[1].Status);
    }

    [Fact]
    public async Task OnAllTasksCompleted_IsInvoked_WhenAllTasksFinish()
    {
        var manager = new StswTaskManager<int>();
        bool completed = false;
        manager.OnAllTasksCompleted = () => completed = true;
        manager.Add(async () => await Task.FromResult(1));
        manager.Add(async () => await Task.FromResult(2));

        await manager.RunAllQueuedAsync(false);

        Assert.True(completed);
    }

    [Fact]
    public async Task Stop_PreventsFurtherTasksFromStarting_Sequential()
    {
        var manager = new StswTaskManager<int>();
        var started = new List<int>();
        manager.Add(async () => { started.Add(1); await Task.Delay(10); return 1; });
        manager.Add(async () => { started.Add(2); await Task.Delay(10); return 2; });

        // Stop after first task
        manager.Tasks[0].OnCompleted = _ => manager.Stop();

        await manager.RunAllQueuedAsync(false);

        Assert.Contains(1, started);
        Assert.DoesNotContain(2, started);
    }
}
