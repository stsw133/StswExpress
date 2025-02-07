using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Data;
using System.Reflection;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that allows binding event handlers to <see cref="ICommand"/> in MVVM,
/// with a fallback for events that have the signature (object sender, EventArgs e).
/// Supports optional <see cref="AllowedKey"/> filtering and passing event args as <see cref="CommandParameter"/>.
/// </summary>
[MarkupExtensionReturnType(typeof(Delegate))]
public class StswEventToCommandExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the binding for the command to execute.
    /// </summary>
    public BindingBase? CommandBinding { get; set; }

    /// <summary>
    /// Gets or sets the binding for the command parameter.
    /// </summary>
    public BindingBase? CommandParameterBinding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event args should be passed as the command parameter.
    /// </summary>
    public bool PassEventArgsAsParameter { get; set; }

    /// <summary>
    /// Gets or sets an optional key filter for key events.
    /// </summary>
    public Key? AllowedKey { get; set; }

    /// <summary>
    /// Provides the event handler delegate to bind to an event in XAML.
    /// </summary>
    /// <param name="serviceProvider">A service provider for the markup extension.</param>
    /// <returns>A delegate bound to the specified event.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget pvt)
            return null;

        if (pvt.TargetProperty is EventInfo eventInfo)
        {
            var eventHandlerType = eventInfo.EventHandlerType;
            return CreateHandler(eventHandlerType, pvt.TargetObject as DependencyObject);
        }
        else if (pvt.TargetProperty is MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 2)
            {
                var eventHandlerType = parameters[1].ParameterType;
                return CreateHandler(eventHandlerType, pvt.TargetObject as DependencyObject);
            }
        }

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
                if (AllowedKey == null || e.Key == AllowedKey.Value)
                    ExecuteCommand(targetObject, e);
            });
        }
        else
        {
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
    }

    /// <summary>
    /// Executes the bound command when the event is triggered.
    /// </summary>
    /// <param name="targetObject">The dependency object where the event occurred.</param>
    /// <param name="e">The event arguments.</param>
    private void ExecuteCommand(DependencyObject targetObject, EventArgs e)
    {
        var cmd = EvaluateBinding<ICommand>(targetObject, CommandBinding!);
        var param = PassEventArgsAsParameter ? e : EvaluateBinding<object>(targetObject, CommandParameterBinding!);

        if (cmd?.CanExecute(param) == true)
            cmd.Execute(param);
    }

    /// <summary>
    /// Evaluates a binding expression and retrieves its value.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="target">The dependency object.</param>
    /// <param name="bindingBase">The binding to evaluate.</param>
    /// <returns>The resolved binding value.</returns>
    private T? EvaluateBinding<T>(DependencyObject target, BindingBase bindingBase)
    {
        if (bindingBase == null)
            return default;

        var tempProperty = DependencyProperty.RegisterAttached(
                "Temp" + Guid.NewGuid().ToString("N"),
                typeof(object),
                typeof(StswEventToCommandExtension),
                new PropertyMetadata(null)
            );

        BindingOperations.SetBinding(target, tempProperty, bindingBase);
        var value = target.GetValue(tempProperty);
        BindingOperations.ClearBinding(target, tempProperty);

        return (T?)value;
    }

    /// <summary>
    /// Creates an event handler delegate using an expression tree.
    /// </summary>
    /// <param name="delegateType">The event delegate type.</param>
    /// <param name="targetObj">The dependency object where the event occurs.</param>
    /// <param name="ext">The extension instance.</param>
    /// <returns>An event delegate for handling the event.</returns>
    private static object CreateDefaultHandlerViaExpression(
        Type delegateType,
        DependencyObject targetObj,
        StswEventToCommandExtension ext)
    {
        var senderParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "sender");
        var eventArgsParam = System.Linq.Expressions.Expression.Parameter(typeof(EventArgs), "e");

        var callExecuteCommand = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(ext),
            nameof(ExecuteCommand),
            typeArguments: null,
            System.Linq.Expressions.Expression.Constant(targetObj),
            eventArgsParam
        );

        var lambda = System.Linq.Expressions.Expression.Lambda(
            delegateType,
            callExecuteCommand,
            senderParam,
            eventArgsParam
        );

        return lambda.Compile();
    }
}

/* usage:

<Button Content="Click Me" Click="{se:StswEventToCommand CommandBinding={Binding MyCommand}}"/>

<TextBox KeyDown="{se:StswEventToCommand CommandBinding={Binding EnterCommand}, AllowedKey=Enter}"/>

<Button Content="Details" Click="{se:StswEventToCommand CommandBinding={Binding ShowDetailsCommand}, PassEventArgsAsParameter=True}"/>

<Button Content="Delete" Click="{se:StswEventToCommand CommandBinding={Binding DeleteCommand}, CommandParameterBinding={Binding SelectedItem}}"/>

*/
