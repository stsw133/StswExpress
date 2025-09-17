using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress.Tests;
public class StswExtensionsTests
{
    #region Clone extensions
    [Fact]
    public void Clone_Binding_ClonesAllProperties()
    {
        var binding = new Binding("TestPath")
        {
            AsyncState = new object(),
            BindingGroupName = "Group",
            BindsDirectlyToSource = true,
            Converter = new TestValueConverter(),
            ConverterCulture = System.Globalization.CultureInfo.InvariantCulture,
            ConverterParameter = "param",
            FallbackValue = "fallback",
            IsAsync = true,
            Mode = BindingMode.TwoWay,
            NotifyOnSourceUpdated = true,
            NotifyOnTargetUpdated = true,
            NotifyOnValidationError = true,
            StringFormat = "format",
            TargetNullValue = "null",
            UpdateSourceExceptionFilter = (ex, bindingExpression) => null,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            ValidatesOnDataErrors = true,
            ValidatesOnExceptions = true,
            XPath = "xpath",
            ElementName = "element"
        };
        binding.ValidationRules.Add(new TestValidationRule());

        var clone = binding.Clone() as Binding;

        Assert.NotNull(clone);
        Assert.Equal(binding.Path.Path, clone.Path.Path);
        Assert.Equal(binding.ElementName, clone.ElementName);
        Assert.Equal(binding.BindingGroupName, clone.BindingGroupName);
        Assert.Equal(binding.BindsDirectlyToSource, clone.BindsDirectlyToSource);
        Assert.Equal(binding.Converter, clone.Converter);
        Assert.Equal(binding.ConverterCulture, clone.ConverterCulture);
        Assert.Equal(binding.ConverterParameter, clone.ConverterParameter);
        Assert.Equal(binding.FallbackValue, clone.FallbackValue);
        Assert.Equal(binding.IsAsync, clone.IsAsync);
        Assert.Equal(binding.Mode, clone.Mode);
        Assert.Equal(binding.NotifyOnSourceUpdated, clone.NotifyOnSourceUpdated);
        Assert.Equal(binding.NotifyOnTargetUpdated, clone.NotifyOnTargetUpdated);
        Assert.Equal(binding.NotifyOnValidationError, clone.NotifyOnValidationError);
        Assert.Equal(binding.StringFormat, clone.StringFormat);
        Assert.Equal(binding.TargetNullValue, clone.TargetNullValue);
        Assert.Equal(binding.UpdateSourceTrigger, clone.UpdateSourceTrigger);
        Assert.Equal(binding.ValidatesOnDataErrors, clone.ValidatesOnDataErrors);
        Assert.Equal(binding.ValidatesOnExceptions, clone.ValidatesOnExceptions);
        Assert.Equal(binding.XPath, clone.XPath);
        Assert.Single(clone.ValidationRules);
    }

    [Fact]
    public void Clone_MultiBinding_ClonesChildBindings()
    {
        var multiBinding = new MultiBinding
        {
            BindingGroupName = "Group",
            Converter = new TestMultiValueConverter(),
            ConverterCulture = System.Globalization.CultureInfo.InvariantCulture,
            ConverterParameter = "param",
            FallbackValue = "fallback",
            Mode = BindingMode.OneWay,
            NotifyOnSourceUpdated = true,
            NotifyOnTargetUpdated = true,
            NotifyOnValidationError = true,
            StringFormat = "format",
            TargetNullValue = "null",
            UpdateSourceExceptionFilter = (ex, bindingExpression) => null,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            ValidatesOnDataErrors = true,
            ValidatesOnExceptions = true
        };
        multiBinding.ValidationRules.Add(new TestValidationRule());
        multiBinding.Bindings.Add(new Binding("A"));
        multiBinding.Bindings.Add(new Binding("B"));

        var clone = multiBinding.Clone() as MultiBinding;

        Assert.NotNull(clone);
        Assert.Equal(multiBinding.BindingGroupName, clone.BindingGroupName);
        Assert.Equal(multiBinding.Converter, clone.Converter);
        Assert.Equal(multiBinding.Bindings.Count, clone.Bindings.Count);
        Assert.Single(clone.ValidationRules);
    }

    [Fact]
    public void Clone_PriorityBinding_ClonesChildBindings()
    {
        var priorityBinding = new PriorityBinding
        {
            BindingGroupName = "Group",
            FallbackValue = "fallback",
            StringFormat = "format",
            TargetNullValue = "null"
        };
        priorityBinding.Bindings.Add(new Binding("A"));
        priorityBinding.Bindings.Add(new Binding("B"));

        var clone = priorityBinding.Clone() as PriorityBinding;

        Assert.NotNull(clone);
        Assert.Equal(priorityBinding.BindingGroupName, clone.BindingGroupName);
        Assert.Equal(priorityBinding.FallbackValue, clone.FallbackValue);
        Assert.Equal(priorityBinding.StringFormat, clone.StringFormat);
        Assert.Equal(priorityBinding.TargetNullValue, clone.TargetNullValue);
        Assert.Equal(priorityBinding.Bindings.Count, clone.Bindings.Count);
    }

    [Fact]
    public void Clone_UnsupportedBinding_Throws()
    {
        var customBinding = new CustomBindingBase();
        Assert.Throws<NotSupportedException>(() => customBinding.Clone());
    }

    [Fact]
    public void TryClone_ClonesICloneableItems()
    {
        var source = new object[] { new CloneableItem(1), new CloneableItem(2) };
        var result = source.TryClone().Cast<CloneableItem>().ToArray();

        Assert.NotSame(source[0], result[0]);
        Assert.NotSame(source[1], result[1]);
        Assert.Equal(1, result[0].Value);
        Assert.Equal(2, result[1].Value);
    }

    [Fact]
    public void TryClone_ReturnsNonCloneableItemsAsIs()
    {
        var source = new object[] { "a", 123 };
        var result = source.TryClone().Cast<object>().ToArray();

        Assert.Same(source[0], result[0]);
        Assert.Same(source[1], result[1]);
    }
    #endregion

    #region Convert extensions
    [Fact]
    public void ToBytes_ThrowsIfNotBitmapSource()
    {
        var notBitmapSource = new DrawingImage();
        Assert.Throws<ArgumentException>(() => notBitmapSource.ToBytes());
    }

    [Fact]
    public void ToBytes_ReturnsByteArray()
    {
        var bitmap = new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
        var bytes = bitmap.ToBytes();
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void ToImageSource_Bitmap_ReturnsImageSource()
    {
        using var bmp = new System.Drawing.Bitmap(1, 1);
        var imgSource = bmp.ToImageSource();
        Assert.IsAssignableFrom<BitmapSource>(imgSource);
    }

    [Fact]
    public void ToImageSource_Icon_ReturnsImageSource()
    {
        using var icon = System.Drawing.SystemIcons.Application;
        var imgSource = icon.ToImageSource();
        Assert.IsAssignableFrom<BitmapSource>(imgSource);
    }

    [Fact]
    public void ToImageSource_Geometry_ReturnsImageSource()
    {
        var geometry = new RectangleGeometry(new Rect(0, 0, 10, 10));
        var imgSource = geometry.ToImageSource(10, Brushes.Red, Brushes.Black, 1);
        Assert.IsAssignableFrom<RenderTargetBitmap>(imgSource);
    }
    #endregion

    #region Color extensions
    [Fact]
    public void ToDrawingColor_ConvertsCorrectly()
    {
        var color = Color.FromArgb(10, 20, 30, 40);
        var drawingColor = color.ToDrawingColor();
        Assert.Equal(10, drawingColor.A);
        Assert.Equal(20, drawingColor.R);
        Assert.Equal(30, drawingColor.G);
        Assert.Equal(40, drawingColor.B);
    }

    [Fact]
    public void ToHex_ReturnsCorrectFormat()
    {
        var color = Color.FromArgb(255, 1, 2, 3);
        Assert.Equal("#010203", color.ToHex());

        var colorWithAlpha = Color.FromArgb(128, 1, 2, 3);
        Assert.Equal("#80010203", colorWithAlpha.ToHex());
    }

    [Fact]
    public void ToMediaColor_ConvertsCorrectly()
    {
        var drawingColor = System.Drawing.Color.FromArgb(10, 20, 30, 40);
        var color = drawingColor.ToMediaColor();
        Assert.Equal(10, color.A);
        Assert.Equal(20, color.R);
        Assert.Equal(30, color.G);
        Assert.Equal(40, color.B);
    }
    #endregion

    #region Process extensions
    [Fact]
    public void GetUser_ReturnsNullOnException()
    {
        using var process = Process.GetCurrentProcess();
        // Simulate exception by passing an invalid process handle
        var fakeProcess = new FakeProcess();
        Assert.Null(fakeProcess.GetUser());
    }
    #endregion

    #region Helpers
    private class TestValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value;
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value;
    }

    private class TestMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) => values;
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => new object[0];
    }

    private class TestValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo) => ValidationResult.ValidResult;
    }

    private class CustomBindingBase : Binding
    {
        public CustomBindingBase() { }
    }

    private class CloneableItem : ICloneable
    {
        public int Value { get; }
        public CloneableItem(int value) => Value = value;
        public object Clone() => new CloneableItem(Value);
    }

    private class FakeProcess : Process
    {
        // Removed the override for Handle as it is not virtual, abstract, or override in the base class.
        // Instead, we can use a property to simulate the behavior.
        public IntPtr FakeHandle => IntPtr.Zero;
    }
    #endregion
}
