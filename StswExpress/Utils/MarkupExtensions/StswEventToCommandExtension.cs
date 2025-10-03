using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// XAML markup extension that binds an event to an <see cref="ICommand"/> in MVVM.
/// 
/// Parameter priority:
/// 1) <see cref="CommandParameter"/> if provided
/// 2) if not provided and <see cref="PassEventArgs"/> is <see langword="true"/>, passes event args
/// 3) otherwise passes <see langword="null"/>
///
/// Additionally, when a literal XAML value is used as <see cref="CommandParameter"/> (e.g. <c>CommandParameter=2</c>),
/// the extension attempts to convert it to the command's preferred parameter type (int, enum, bool, etc.).
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;Button Content="Click Me" Click="{se:StswEventToCommand MyCommand}"/&gt;
/// &lt;TextBox KeyDown="{se:StswEventToCommand EnterCommand, AllowedKey=Enter}"/&gt;
/// &lt;Button Content="Details" Click="{se:StswEventToCommand ShowDetailsCommand, PassEventArgsAsParameter=True}"/&gt;
/// &lt;Button Content="Delete" Click="{se:StswEventToCommand DeleteCommand, CommandParameter={Binding SelectedItem}}"/&gt;
/// </code>
/// </example>
[MarkupExtensionReturnType(typeof(object))]
public class StswEventToCommandExtension : MarkupExtension
{
    private readonly BindingBase _command;

    /// <summary>
    /// Gets or sets the parameter to pass to the command. If this is a <see cref="BindingBase"/>, it will be evaluated at runtime.
    /// </summary>
    public object? CommandParameter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to pass the event arguments to the command if no <see cref="CommandParameter"/> is provided.
    /// </summary>
    public bool PassEventArgs { get; set; }

    /// <summary>
    /// Gets or sets an optional key filter for key events.
    /// </summary>
    public Key? AllowedKey { get; set; }

    public StswEventToCommandExtension(string path) => _command = new Binding(path);
    public StswEventToCommandExtension(ICommand command) => _command = new Binding { Source = command };

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider?.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget pvt)
            return null;

        if (pvt.TargetObject is not DependencyObject d)
            return null;

        if (pvt.TargetProperty is EventInfo eventInfo)
            return CreateHandler(eventInfo.EventHandlerType, d);

        if (pvt.TargetProperty is MethodInfo methodInfo)
            return CreateHandlerFromMethodInfo(methodInfo, d);

        return null;
    }

    /// <summary>
    /// Creates a delegate handler for the specified event type.
    /// </summary>
    /// <param name="eventHandlerType">The event handler type.</param>
    /// <param name="targetObject">The target dependency object.</param>
    /// <returns>A delegate to handle the event.</returns>
    private Delegate? CreateHandler(Type? eventHandlerType, DependencyObject? targetObject)
    {
        if (targetObject == null || eventHandlerType == null)
            return null;

        if (eventHandlerType == typeof(KeyEventHandler))
        {
            return new KeyEventHandler((sender, e) =>
            {
                if (AllowedKey == null || e.Key == AllowedKey.Value || (e.Key == Key.System && e.SystemKey == AllowedKey.Value))
                    ExecuteCommand(targetObject, e);
            });
        }

        var invokeMethod = eventHandlerType.GetMethod("Invoke");
        if (invokeMethod == null)
            return null;

        var parameters = invokeMethod.GetParameters();
        if (parameters.Length == 2
            && parameters[0].ParameterType == typeof(object)
            && typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType))
        {
            return (Delegate)CreateDefaultHandlerViaExpression(
                eventHandlerType,
                targetObject,
                this
            );
        }

        return null;
    }

    /// <summary>
    /// Creates a delegate handler from a method info, handling both standard and attachable event adders.
    /// </summary>
    /// <param name="methodInfo">The method info representing the event adder.</param>
    /// <param name="targetObject">The target dependency object.</param>
    /// <returns>A delegate to handle the event, or <see langword="null"/> if it cannot be determined.</returns>
    private Delegate? CreateHandlerFromMethodInfo(MethodInfo methodInfo, DependencyObject targetObject)
    {
        if (methodInfo.IsSpecialName && methodInfo.Name.StartsWith("add_", StringComparison.Ordinal))
        {
            var ps = methodInfo.GetParameters();
            if (ps.Length == 1 && typeof(Delegate).IsAssignableFrom(ps[0].ParameterType))
                return CreateHandler(ps[0].ParameterType, targetObject);
        }

        var delParam = methodInfo.GetParameters().FirstOrDefault(p => typeof(Delegate).IsAssignableFrom(p.ParameterType));
        if (delParam is not null)
            return CreateHandler(delParam.ParameterType, targetObject);

        return null;
    }

    /// <summary>
    /// Executes the bound command when the event is triggered.
    /// </summary>
    /// <param name="targetObject">The dependency object where the event occurred.</param>
    /// <param name="e">The event arguments.</param>
    private void ExecuteCommand(DependencyObject targetObject, EventArgs e)
    {
        var cmd = EvaluateBinding<ICommand>(targetObject, _command);
        if (cmd is null)
            return;

        var param = ResolveParameter(targetObject, e);
        if (cmd.CanExecute(param))
            cmd.Execute(param);
    }

    /// <summary>
    /// Gets the preferred parameter type of the command's Execute method, if available.
    /// </summary>
    /// <param name="target">The command instance.</param>
    /// <param name="e">The event arguments.</param>
    /// <param name="cmd">The command to evaluate.</param>
    /// <returns>The preferred parameter type, or <see langword="null"/> if it cannot be determined.</returns>
    private object? ResolveParameter(DependencyObject target, EventArgs e)
    {
        if (CommandParameter is BindingBase bb)
            return EvaluateBinding<object>(target, bb);

        if (CommandParameter is not null)
            return CommandParameter;

        return PassEventArgs ? e : null;
    }

    /// <summary>
    /// Evaluates a binding expression and retrieves its value.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="target">The dependency object.</param>
    /// <param name="bindingBase">The binding to evaluate.</param>
    /// <returns>The resolved binding value.</returns>
    private static T? EvaluateBinding<T>(DependencyObject target, BindingBase bindingBase)
    {
        if (bindingBase == null)
            return default;

        BindingOperations.SetBinding(target, TempValueProperty, bindingBase);
        var value = target.GetValue(TempValueProperty);
        BindingOperations.ClearBinding(target, TempValueProperty);
        target.ClearValue(TempValueProperty);

        return (T?)value;
    }

    /// <summary>
    /// Creates an event handler delegate using an expression tree.
    /// </summary>
    /// <param name="delegateType">The event delegate type.</param>
    /// <param name="targetObj">The dependency object where the event occurs.</param>
    /// <param name="ext">The extension instance.</param>
    /// <returns>An event delegate for handling the event.</returns>
    private static object CreateDefaultHandlerViaExpression(Type delegateType, DependencyObject targetObj, StswEventToCommandExtension ext)
    {
        var invoke = delegateType.GetMethod("Invoke")!;
        var delegateParams = invoke.GetParameters();

        var senderParam = System.Linq.Expressions.Expression.Parameter(delegateParams[0].ParameterType, "sender");
        var eventArgsParam = System.Linq.Expressions.Expression.Parameter(delegateParams[1].ParameterType, "e");

        var callExecuteCommand = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(ext),
            nameof(ExecuteCommand),
            typeArguments: null,
            System.Linq.Expressions.Expression.Constant(targetObj),
            System.Linq.Expressions.Expression.Convert(eventArgsParam, typeof(EventArgs))
        );

        var lambda = System.Linq.Expressions.Expression.Lambda(
            delegateType,
            callExecuteCommand,
            senderParam,
            eventArgsParam
        );

        return lambda.Compile();
    }

    /// <summary>
    /// A temporary attached property used for evaluating bindings.
    /// </summary>
    private static readonly DependencyProperty TempValueProperty
        = DependencyProperty.RegisterAttached(
            nameof(TempValueProperty)[..^8],
            typeof(object),
            typeof(StswEventToCommandExtension),
            new PropertyMetadata(null)
        );
}
