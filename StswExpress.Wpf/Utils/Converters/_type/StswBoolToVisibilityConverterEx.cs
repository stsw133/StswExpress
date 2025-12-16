using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace StswExpress.Wpf;

/// <summary>
/// Converts a boolean value to a Visibility enumeration value.
/// </summary>
internal class StswBoolToVisibilityTypeConverter : TypeConverter
{
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(bool) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(bool) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc/>
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is bool b)
            return b ? Visibility.Visible : Visibility.Collapsed;

        return Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is Visibility v && destinationType == typeof(bool))
            return v == Visibility.Visible;

        return false;
    }
}
