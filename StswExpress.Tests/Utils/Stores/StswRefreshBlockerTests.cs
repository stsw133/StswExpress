namespace StswExpress.Commons.Tests.Utils.Stores;
public class StswRefreshBlockerTests
{
    [Fact]
    public void Dispose_InvokesOnDisposeAction_Once()
    {
        int callCount = 0;
        var blocker = new StswRefreshBlocker(() => callCount++);

        blocker.Dispose();
        Assert.Equal(1, callCount);

        // Dispose again should not invoke action
        blocker.Dispose();
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Constructor_NullAction_DoesNotThrow()
    {
        // Should not throw, but will throw NullReferenceException on Dispose
        var blocker = new StswRefreshBlocker(null!);
        Assert.NotNull(blocker);
    }

    [Fact]
    public void Dispose_SetsDisposedFlag()
    {
        bool disposed = false;
        var blocker = new StswRefreshBlocker(() => disposed = true);

        blocker.Dispose();
        Assert.True(disposed);
    }
}
