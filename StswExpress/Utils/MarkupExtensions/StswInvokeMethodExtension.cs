using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A XAML markup extension that invokes a static method with parameters provided in XAML.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock&gt;
///     &lt;TextBlock.Text&gt;
///         &lt;se:StswInvokeMethod MethodName="YourNamespace.Helper.FormatName"&gt;
///             &lt;se:StswInvokeMethod.Parameters&gt;
///                 &lt;sys:String&gt;John&lt;/sys:String&gt;
///                 &lt;Binding Path="LastName" /&gt;
///             &lt;/se:StswInvokeMethod.Parameters&gt;
///         &lt;/se:StswInvokeMethod&gt;
///     &lt;/TextBlock.Text&gt;
/// &lt;/TextBlock&gt;
/// </code>
/// </example>
[MarkupExtensionReturnType(typeof(object))]
[StswInfo("0.17.0", IsTested = false)]
public class StswInvokeMethodExtension : MarkupExtension
{
    /// <summary>
    /// Fully qualified name of the static method to invoke, e.g. "MyNamespace.MyClass.MyMethod".
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Parameters to pass into the method. Optional.
    /// </summary>
    public Collection<object> Parameters { get; set; } = [];

    public StswInvokeMethodExtension()
    {
    }

    public StswInvokeMethodExtension(string methodName)
    {
        MethodName = methodName;
    }

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        var multiBinding = new MultiBinding
        {
            Converter = new InvokeMethodConverter { MethodName = MethodName },
            Mode = BindingMode.OneWay
        };

        foreach (var param in Parameters)
        {
            switch (param)
            {
                case BindingBase binding:
                    multiBinding.Bindings.Add(binding);
                    break;
                default:
                    multiBinding.Bindings.Add(new Binding
                    {
                        Source = new ObjectValueProvider(param),
                        Path = new PropertyPath(nameof(ObjectValueProvider.Value))
                    });
                    break;
            }
        }

        return multiBinding.ProvideValue(serviceProvider);
    }

    /// <summary>
    /// A simple observable object that holds a value to be used in the InvokeMethodConverter.
    /// </summary>
    /// <param name="value">The value to hold.</param>
    public class ObjectValueProvider(object value) : StswObservableObject
    {
        /// <summary>
        /// The value to be used in the method invocation.
        /// </summary>
        public object Value { get; } = value;
    }

    /// <summary>
    /// A converter that invokes a static method with parameters provided through bindings.
    /// </summary>
    public class InvokeMethodConverter : IMultiValueConverter
    {
        /// <summary>
        /// The fully qualified name of the static method to invoke.
        /// </summary>
        public string? MethodName { get; set; }

        /// <summary>
        /// Converts an array of values to the result of invoking the specified static method.
        /// </summary>
        /// <param name="values">An array of values to pass as parameters to the method.</param>
        /// <param name="targetType">The type of the target property (not used in this converter).</param>
        /// <param name="parameter">An optional parameter (not used in this converter).</param>
        /// <param name="culture">The culture to use for conversion (not used in this converter).</param>
        /// <returns>The result of the method invocation.</returns>
        /// <exception cref="ArgumentException">Thrown when MethodName is not fully qualified or the method cannot be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the specified type cannot be found or the method does not exist.</exception>
        /// <exception cref="MissingMethodException">Thrown when the method with the specified name and parameter count cannot be found.</exception>
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(MethodName))
                return null;

            var lastDot = MethodName.LastIndexOf('.');
            if (lastDot < 0)
                throw new ArgumentException("MethodName must be fully qualified (Namespace.Class.Method)");

            var typeName = MethodName[..lastDot];
            var methodName = MethodName[(lastDot + 1)..];

            var type = Type.GetType(typeName) ?? throw new InvalidOperationException($"Type '{typeName}' not found.");
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.GetParameters().Length == values.Length);

            return method == null
                ? throw new MissingMethodException($"Method '{methodName}' not found with {values.Length} parameter(s).")
                : method.Invoke(null, values);
        }

        /// <summary>
        /// Converts back the result of the method invocation to an array of values.
        /// </summary>
        /// <param name="value">The result of the method invocation.</param>
        /// <param name="targetTypes">An array of target types (not used in this converter).</param>
        /// <param name="parameter">An optional parameter (not used in this converter).</param>
        /// <param name="culture">The culture to use for conversion (not used in this converter).</param>
        /// <returns>Throws <see cref="NotSupportedException"/> since this converter is one-way only.</returns>
        /// <exception cref="NotSupportedException">Always thrown since this converter is one-way only.</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
