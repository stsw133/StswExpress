using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress.Tests.Utils.MarkupExtensions;
public class StswEventToCommandExtensionTests
{
    private class TestCommand : ICommand
    {
        public bool CanExecuteResult { get; set; } = true;
        public object? LastParameter { get; private set; }
        public bool Executed { get; set; }
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

    /// <summary>
    /// ICommand, który preferuje RoutedEventArgs:
    /// - ma publiczne Execute(RoutedEventArgs) (to widzi nasza refleksja),
    /// - a ICommand.Execute(object) i CanExecute(object) s¹ jawnie zaimplementowane.
    /// </summary>
    private class RoutedArgsPreferringCommand : ICommand
    {
        public bool ExecutedWithRoutedArgs { get; private set; }
        public object? LastParameterObjectExec { get; private set; }

        public event EventHandler? CanExecuteChanged;

        // to jest "preferowana" sygnatura dla naszych testów
        public bool CanExecute(RoutedEventArgs e) => true;
        public void Execute(RoutedEventArgs e) => ExecutedWithRoutedArgs = true;

        bool ICommand.CanExecute(object? parameter)
        {
            return true;
        }
        void ICommand.Execute(object? parameter)
        {
            LastParameterObjectExec = parameter;
        }
    }

    private class DummyControl : Button { }

    [StaFact]
    public void ProvideValue_ReturnsNull_WhenServiceProviderIsNull()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        Assert.Null(ext.ProvideValue(null!));
    }

    [StaFact]
    public void ProvideValue_ReturnsNull_WhenTargetPropertyIsNotEventOrMethod()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var sp = new FakeServiceProvider(new DummyControl(), typeof(string));
        Assert.Null(ext.ProvideValue(sp));
    }

    [StaFact]
    public void CreateHandler_ReturnsKeyEventHandler_WhenEventTypeIsKeyEventHandler()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var handler = ext.GetType()
            .GetMethod("CreateHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, [typeof(KeyEventHandler), new DummyControl()]);
        Assert.IsType<KeyEventHandler>(handler);
    }

    [StaFact]
    public void ExecuteCommand_ExecutesCommand_WhenCanExecuteTrue()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var control = new DummyControl();

        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
           .Invoke(ext, [control, EventArgs.Empty]);

        Assert.True(cmd.Executed);
        Assert.Null(cmd.LastParameter);
    }

    [StaFact]
    public void ExecuteCommand_DoesNotExecute_WhenCanExecuteFalse()
    {
        var cmd = new TestCommand { CanExecuteResult = false };
        var ext = new StswEventToCommandExtension(cmd);
        var control = new DummyControl();

        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
           .Invoke(ext, [control, EventArgs.Empty]);

        Assert.False(cmd.Executed);
    }

    [StaFact]
    public void ExecuteCommand_PassesCommandParameter_WhenLiteralProvided()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            CommandParameter = "param"
        };
        var control = new DummyControl();

        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
           .Invoke(ext, [control, EventArgs.Empty]);

        Assert.True(cmd.Executed);
        Assert.Equal("param", cmd.LastParameter);
    }

    [StaFact]
    public void ExecuteCommand_PassesCommandParameter_WhenBindingProvided()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            CommandParameter = new Binding { Source = "param" }
        };
        var control = new DummyControl();

        ext.GetType().GetMethod("ExecuteCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
           .Invoke(ext, [control, EventArgs.Empty]);

        Assert.True(cmd.Executed);
        Assert.Equal("param", cmd.LastParameter);
    }

    [StaFact]
    public void ResolveParameter_ReturnsBindingValue_WhenCommandParameterIsBinding()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            CommandParameter = new Binding { Source = "testValue" }
        };
        var control = new DummyControl();
        var method = ext.GetType().GetMethod("ResolveParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var result = method.Invoke(ext, [control, EventArgs.Empty]);
        Assert.Equal("testValue", result);
    }

    [StaFact]
    public void ResolveParameter_ReturnsLiteral_WhenCommandParameterIsLiteral()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            CommandParameter = 42
        };
        var control = new DummyControl();
        var method = ext.GetType().GetMethod("ResolveParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var result = method.Invoke(ext, [control, EventArgs.Empty]);
        Assert.Equal(42, result);
    }

    [StaFact]
    public void ResolveParameter_ReturnsEventArgs_WhenPassEventArgsIsTrue_AndNoParameter()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            PassEventArgs = true
        };
        var control = new DummyControl();
        var args = new RoutedEventArgs(Button.ClickEvent, control);
        var method = ext.GetType().GetMethod("ResolveParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var result = method.Invoke(ext, [control, args]);
        Assert.Equal(args, result);
    }

    [StaFact]
    public void ResolveParameter_ReturnsNull_WhenNoParameterAndPassEventArgsFalse()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            PassEventArgs = false
        };
        var control = new DummyControl();
        var method = ext.GetType().GetMethod("ResolveParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var result = method.Invoke(ext, [control, EventArgs.Empty]);
        Assert.Null(result);
    }

    [StaFact]
    public void EvaluateBinding_ReturnsDefault_WhenBindingBaseIsNull()
    {
        var control = new DummyControl();
        var method = typeof(StswExpress.StswEventToCommandExtension)
            .GetMethod("EvaluateBinding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(typeof(object));
        var result = method.Invoke(null, [control, null]);
        Assert.Null(result);
    }

    [StaFact]
    public void KeyEventHandler_OnlyExecutesForAllowedKey()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd)
        {
            AllowedKey = Key.Enter
        };
        var control = new DummyControl();
        var handler = (KeyEventHandler)ext.GetType()
            .GetMethod("CreateHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, [typeof(KeyEventHandler), control])!;

        if (Keyboard.PrimaryDevice is null)
            return;

        var argsEnter = new KeyEventArgs(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, Key.Enter)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
        handler(control, argsEnter);
        Assert.True(cmd.Executed);

        cmd.Executed = false;

        var argsSpace = new KeyEventArgs(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, Key.Space)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
        handler(control, argsSpace);
        Assert.False(cmd.Executed);
    }

    [StaFact]
    public void ProvideValue_ReturnsHandler_ForEventInfo()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var control = new DummyControl();
        var eventInfo = typeof(Button).GetEvent(nameof(Button.Click));
        var sp = new FakeServiceProvider(control, eventInfo!);

        var result = ext.ProvideValue(sp);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<Delegate>(result);
    }

    [StaFact]
    public void ProvideValue_ReturnsHandler_ForMethodInfo()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var control = new DummyControl();
        var methodInfo = typeof(Button).GetEvent(nameof(Button.Click))!.GetAddMethod();

        var sp = new FakeServiceProvider(control, methodInfo!);
        var result = ext.ProvideValue(sp);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<Delegate>(result);
    }

    [StaFact]
    public void CreateHandler_ReturnsNull_ForUnknownEventType()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var handler = ext.GetType()
            .GetMethod("CreateHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, [typeof(EventHandlerList), new DummyControl()]);

        Assert.Null(handler);
    }

    [StaFact]
    public void CreateHandlerFromMethodInfo_ReturnsNull_ForNonDelegateParameter()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var methodInfo = typeof(object).GetMethod("ToString")!;
        var control = new DummyControl();

        var handler = ext.GetType()
            .GetMethod("CreateHandlerFromMethodInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(ext, [methodInfo, control]);

        Assert.Null(handler);
    }

    [StaFact]
    public void CreateDefaultHandlerViaExpression_InvokesExecuteCommand()
    {
        var cmd = new TestCommand();
        var ext = new StswEventToCommandExtension(cmd);
        var control = new DummyControl();
        var delegateType = typeof(EventHandler);

        var handler = (Delegate)ext.GetType()
            .GetMethod("CreateDefaultHandlerViaExpression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [delegateType, control, ext])!;

        Assert.NotNull(handler);

        ((EventHandler)handler)(control, EventArgs.Empty);

        Assert.True(cmd.Executed);
    }

    [StaFact]
    public void TempValueProperty_IsRegisteredAttachedProperty()
    {
        var extType = typeof(StswExpress.StswEventToCommandExtension);
        var dpField = extType.GetField("TempValueProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(dpField);
        var dp = dpField!.GetValue(null);
        Assert.IsType<DependencyProperty>(dp);
    }

    // ==== Helpers ====

    private class FakeServiceProvider(object targetObject, object targetProperty) : IServiceProvider
    {
        private readonly object _targetObject = targetObject;
        private readonly object _targetProperty = targetProperty;

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
                return new FakeProvideValueTarget(_targetObject, _targetProperty);
            return null;
        }
    }

    private class FakeProvideValueTarget(object obj, object prop) : IProvideValueTarget
    {
        public object TargetObject { get; } = obj;
        public object TargetProperty { get; } = prop;
    }

    private class FakePresentationSource : PresentationSource
    {
        private Visual _root = new Canvas();
        public override Visual RootVisual
        {
            get => _root;
            set => _root = value;
        }
        protected override CompositionTarget? GetCompositionTargetCore() => null;
        public override bool IsDisposed => false;
    }
}
