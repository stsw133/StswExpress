using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Data;
using System.Reflection;

namespace StswExpress;

/// <summary>
/// A multi-event MarkupExtension that allows binding event handlers to ICommand in MVVM,
/// with a fallback for events that have the signature (object sender, EventArgs e).
/// Supports optional AllowedKey filtering and passing event args as CommandParameter.
/// </summary>
[MarkupExtensionReturnType(typeof(Delegate))]
public class StswEventToCommandExtension : MarkupExtension
{
    /// <summary>
    /// 
    /// </summary>
    public BindingBase? CommandBinding { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand? Command { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public BindingBase? CommandParameterBinding { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public object? CommandParameter { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool PassEventArgsAsParameter { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Key? AllowedKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="eventHandlerType"></param>
    /// <param name="targetObject"></param>
    /// <returns></returns>
    private Delegate? CreateHandler(Type eventHandlerType, DependencyObject targetObject)
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
    /// 
    /// </summary>
    private void ExecuteCommand(DependencyObject targetObject, EventArgs e)
    {
        var cmd = Command ?? EvaluateBinding<ICommand>(targetObject, CommandBinding);
        var param = PassEventArgsAsParameter ? e : CommandParameter ?? EvaluateBinding<object>(targetObject, CommandParameterBinding);

        if (cmd?.CanExecute(param) == true)
            cmd.Execute(param);
    }

    /// <summary>
    /// 
    /// </summary>
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
    /// 
    /// </summary>
    /// <param name="delegateType"></param>
    /// <param name="targetObj"></param>
    /// <param name="ext"></param>
    /// <returns></returns>
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
