using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
[MarkupExtensionReturnType(typeof(object))]
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="MissingMethodException"></exception>
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
    /// 
    /// </summary>
    /// <param name="value"></param>
    public class ObjectValueProvider(object value) : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public object Value { get; } = value;

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public class InvokeMethodConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public string? MethodName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="MissingMethodException"></exception>
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
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}

/* usage:

<TextBlock>
    <TextBlock.Text>
        <se:StswInvokeMethod MethodName="YourNamespace.Helper.FormatName">
            <se:StswInvokeMethod.Parameters>
                <sys:String>John</sys:String>
                <Binding Path="LastName" />
            </se:StswInvokeMethod.Parameters>
        </se:StswInvokeMethod>
    </TextBlock.Text>
</TextBlock>

*/
