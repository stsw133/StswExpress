using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress.Tests;
public class StswEventToCommandExtensionTests
{
    private class TestCommand : ICommand
    {
        public bool CanExecuteResult { get; set; } = true;
        public object? LastParameter { get; private set; }
        private bool _executed;
        public bool Executed
        {
            get => _executed;
            set => _executed = value; // Add a public setter to allow modification
        }
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            LastParameter = parameter;
            return CanExecuteResult;
        }

        public void Execute(object? parameter)
        {
            Executed = true;
            LastParameter = parameter;
        }
    }

    private class DummyControl : Button { }

    [Fact]
    public void ProvideValue_ReturnsNull_WhenServiceProviderIsNull()
    {
        var ext = new StswEventToCommandExtension();
        Assert.Null(ext.ProvideValue(null!));
    }

    [Fact]
    public void ProvideValue_ReturnsNull_WhenTargetPropertyIsNotEventOrMethod()
    {
        var ext = new StswEventToCommandExtension();
        var sp = new FakeServiceProvider(new object(), typeof(string));
        Assert.Null(ext.ProvideValue(sp));
    }

    [Fact]
    public void CreateHandler_ReturnsKeyEventHandler_WhenEventTypeIsKeyEventHandler()
    {
        var ext = new StswEventToCommandExtension();
        var handler = ext.GetType().GetMethod("CreateHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { typeof(KeyEventHandler), new DummyControl() });
        Assert.IsType<KeyEventHandler>(handler);
    }

    [Fact]
    public void ExecuteCommand_ExecutesCommand_WhenCanExecuteTrue()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension
        {
            CommandBinding = new Binding { Source = cmd }
        };
        var control = new DummyControl();
        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { control, EventArgs.Empty });
        Assert.True(cmd.Executed);
    }

    [Fact]
    public void ExecuteCommand_DoesNotExecute_WhenCanExecuteFalse()
    {
        var cmd = new TestCommand { CanExecuteResult = false };
        var ext = new StswEventToCommandExtension
        {
            CommandBinding = new Binding { Source = cmd }
        };
        var control = new DummyControl();
        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { control, EventArgs.Empty });
        Assert.False(cmd.Executed);
    }

    [Fact]
    public void ExecuteCommand_PassesEventArgsAsParameter_WhenConfigured()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension
        {
            CommandBinding = new Binding { Source = cmd },
            PassEventArgsAsParameter = true
        };
        var control = new DummyControl();
        var args = new RoutedEventArgs();
        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { control, args });
        Assert.Same(args, cmd.LastParameter);
    }

    [Fact]
    public void ExecuteCommand_PassesCommandParameterBinding_WhenConfigured()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension
        {
            CommandBinding = new Binding { Source = cmd },
            CommandParameterBinding = new Binding { Source = "param" }
        };
        var control = new DummyControl();
        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { control, EventArgs.Empty });
        Assert.Equal("param", cmd.LastParameter);
    }

    [Fact]
    public void KeyEventHandler_OnlyExecutesForAllowedKey()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension
        {
            CommandBinding = new Binding { Source = cmd },
            AllowedKey = Key.Enter
        };
        var control = new DummyControl();
        var handler = (KeyEventHandler)ext.GetType().GetMethod("CreateHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, new object[] { typeof(KeyEventHandler), control });

        var args = new KeyEventArgs(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, Key.Enter)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
        handler(control, args);
        Assert.True(cmd.Executed);

        cmd.Executed = false;
        var args2 = new KeyEventArgs(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, Key.Space)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
        handler(control, args2);
        Assert.False(cmd.Executed);
    }

    [Fact]
    public void EvaluateBinding_ReturnsDefault_WhenBindingIsNull()
    {
        var ext = new StswEventToCommandExtension();
        var control = new DummyControl();
        var result = ext.GetType().GetMethod("EvaluateBinding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(typeof(object))
            .Invoke(ext, new object[] { control, null });
        Assert.Null(result);
    }

    // Helper classes for test
    private class FakeServiceProvider : IServiceProvider
    {
        private readonly object _targetObject;
        private readonly object _targetProperty;
        public FakeServiceProvider(object targetObject, object targetProperty)
        {
            _targetObject = targetObject;
            _targetProperty = targetProperty;
        }
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
                return new FakeProvideValueTarget(_targetObject, _targetProperty);
            return null;
        }
    }

    private class FakeProvideValueTarget : IProvideValueTarget
    {
        public object TargetObject { get; }
        public object TargetProperty { get; }
        public FakeProvideValueTarget(object obj, object prop)
        {
            TargetObject = obj;
            TargetProperty = prop;
        }
    }

    private class FakePresentationSource : PresentationSource
    {
        public override Visual RootVisual { get => null!; set { } }
        protected override CompositionTarget GetCompositionTargetCore() => null!;
        public override bool IsDisposed => false;
    }
}
