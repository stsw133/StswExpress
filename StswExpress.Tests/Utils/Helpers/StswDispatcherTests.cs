using System.Reflection;
using System.Windows;

namespace StswExpress.Tests;
public class StswDispatcherTests
{
    [Fact]
    public void Run_ExecutesAction_WhenDispatcherIsNull()
    {
        // Arrange
        bool executed = false;
        var originalApp = Application.Current;
        SetApplicationCurrent(null);

        // Act
        StswDispatcher.Run(() => executed = true);

        // Assert
        Assert.True(executed);

        // Cleanup
        SetApplicationCurrent(originalApp);
    }

    [Fact]
    public void Run_ExecutesAction_WhenCheckAccessIsTrue()
    {
        // Arrange
        bool executed = false;
        var dispatcherMock = new DispatcherMock { CheckAccessResult = true };
        var appMock = new ApplicationMock(dispatcherMock);
        var originalApp = Application.Current;
        SetApplicationCurrent(appMock);

        // Act
        StswDispatcher.Run(() => executed = true);

        // Assert
        Assert.True(executed);

        // Cleanup
        SetApplicationCurrent(originalApp);
    }

    [Fact]
    public void Run_InvokesDispatcher_WhenCheckAccessIsFalse()
    {
        // Arrange
        bool invoked = false;
        var dispatcherMock = new DispatcherMock { CheckAccessResult = false, InvokeAction = () => invoked = true };
        var appMock = new ApplicationMock(dispatcherMock);
        var originalApp = Application.Current;
        SetApplicationCurrent(appMock);

        // Act
        StswDispatcher.Run(() => { });

        // Assert
        Assert.True(invoked);

        // Cleanup
        SetApplicationCurrent(originalApp);
    }

    [Fact]
    public async Task RunAsync_ExecutesAction_WhenDispatcherIsNull()
    {
        // Arrange
        bool executed = false;
        var originalApp = Application.Current;
        SetApplicationCurrent(null);

        // Act
        await StswDispatcher.RunAsync(() => executed = true);

        // Assert
        Assert.True(executed);

        // Cleanup
        SetApplicationCurrent(originalApp);
    }

    [Fact]
    public async Task RunAsync_ExecutesAction_WhenCheckAccessIsTrue()
    {
        // Arrange
        bool executed = false;
        var dispatcherMock = new DispatcherMock { CheckAccessResult = true };
        var appMock = new ApplicationMock(dispatcherMock);
        var originalApp = Application.Current;
        SetApplicationCurrent(appMock);

        // Act
        await StswDispatcher.RunAsync(() => executed = true);

        // Assert
        Assert.True(executed);

        // Cleanup
        SetApplicationCurrent(originalApp);
    }

    [Fact]
    public async Task RunAsync_InvokesDispatcher_WhenCheckAccessIsFalse()
    {
        // Arrange
        bool invoked = false;
        var dispatcherMock = new DispatcherMock { CheckAccessResult = false, InvokeAsyncAction = () => { invoked = true; return Task.CompletedTask; } };
        var appMock = new ApplicationMock(dispatcherMock);
        var originalApp = Application.Current;
        SetApplicationCurrent(appMock);

        // Act
        await StswDispatcher.RunAsync(() => { });

        // Assert
        Assert.True(invoked);

        // Cleanup
        SetApplicationCurrent(originalApp);
    }

    // Helper method to set Application.Current using reflection
    private static void SetApplicationCurrent(Application? app)
    {
        var currentProperty = typeof(Application).GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
        if (currentProperty != null)
        {
            var backingField = currentProperty.DeclaringType?.GetField("_current", BindingFlags.Static | BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(null, app);
            }
        }
    }

    // Mocks for Dispatcher and Application
    private class DispatcherMock
    {
        public bool CheckAccessResult { get; set; }
        public Action? InvokeAction { get; set; }
        public Func<Task>? InvokeAsyncAction { get; set; }

        public bool CheckAccess() => CheckAccessResult;
        public void Invoke(Action action) => InvokeAction?.Invoke();
        public Task InvokeAsync(Action action)
        {
            if (InvokeAsyncAction != null)
                return InvokeAsyncAction();
            action();
            return Task.CompletedTask;
        }
    }

    private class ApplicationMock : Application
    {
        public DispatcherMock DispatcherMock { get; }

        public ApplicationMock(DispatcherMock dispatcherMock)
        {
            DispatcherMock = dispatcherMock;
        }

        public DispatcherAdapter CustomDispatcher => new DispatcherAdapter(DispatcherMock);
    }

    private class DispatcherAdapter
    {
        private readonly DispatcherMock _mock;

        public DispatcherAdapter(DispatcherMock mock)
        {
            _mock = mock;
        }

        public bool CheckAccess() => _mock.CheckAccess();

        public void Invoke(Action action) => _mock.Invoke(action);

        public Task InvokeAsync(Action action) => _mock.InvokeAsync(action);
    }
}
