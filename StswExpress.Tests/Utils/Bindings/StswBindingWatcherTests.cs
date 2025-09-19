using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Bindings;
public class StswBindingWatcherTests
{
    private class TestFrameworkElement : FrameworkElement
    {
        public object TestValue
        {
            get => GetValue(TestProperty);
            set => SetValue(TestProperty, value);
        }
        public static readonly DependencyProperty TestProperty
            = DependencyProperty.Register(
                nameof(TestValue),
                typeof(object),
                typeof(TestFrameworkElement),
                new PropertyMetadata(null)
            );
    }

    [Fact]
    public void WatchBindingAssignment_ThrowsOnNullArguments()
    {
        var element = new TestFrameworkElement();
        var property = TestFrameworkElement.TestProperty;
        Action callback = () => { };

        Assert.Throws<ArgumentNullException>(() =>
            StswBindingWatcher.WatchBindingAssignment(null, property, callback));
        Assert.Throws<ArgumentNullException>(() =>
            StswBindingWatcher.WatchBindingAssignment(element, null, callback));
        Assert.Throws<ArgumentNullException>(() =>
            StswBindingWatcher.WatchBindingAssignment(element, property, null));
    }

    [Fact]
    public void WatchBindingAssignment_InvokesCallbackOnBindingAssignment()
    {
        var element = new TestFrameworkElement();
        bool callbackInvoked = false;

        StswBindingWatcher.WatchBindingAssignment(element, TestFrameworkElement.TestProperty, () => callbackInvoked = true);

        var binding = new Binding("SomeProperty");
        element.SetBinding(TestFrameworkElement.TestProperty, binding);

        // Simulate property change to trigger the ValueChanged event
        element.TestValue = "new value";

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void WatchBindingAssignment_DoesNotInvokeCallbackIfNoBinding()
    {
        var element = new TestFrameworkElement();
        bool callbackInvoked = false;

        StswBindingWatcher.WatchBindingAssignment(element, TestFrameworkElement.TestProperty, () => callbackInvoked = true);

        // Set value directly, not via binding
        element.TestValue = "direct value";

        Assert.False(callbackInvoked);
    }
}
