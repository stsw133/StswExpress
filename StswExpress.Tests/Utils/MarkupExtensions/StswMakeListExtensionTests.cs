using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace StswExpress.Tests.Utils.MarkupExtensions;
public class StswMakeListExtensionTests
{
    private class DummyTargetProvider : IProvideValueTarget
    {
        public object TargetObject { get; set; } = new object();
        public object TargetProperty { get; set; }
    }

    private class DummyServiceProvider : IServiceProvider
    {
        private readonly IProvideValueTarget _target;
        public DummyServiceProvider(IProvideValueTarget target) => _target = target;
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
                return _target;
            return null;
        }
    }

    public static IEnumerable<object[]> ListTypes()
    {
        yield return new object[] { typeof(List<int>), typeof(int), "1,2,3", new[] { 1, 2, 3 } };
        yield return new object[] { typeof(List<string>), typeof(string), "a,b,c", new[] { "a", "b", "c" } };
        yield return new object[] { typeof(List<double>), typeof(double), "1.1,2.2,3.3", new[] { 1.1, 2.2, 3.3 } };
    }

    [Theory]
    [MemberData(nameof(ListTypes))]
    public void ProvideValue_ReturnsCorrectList(Type listType, Type elementType, string values, object[] expected)
    {
        var dp = DependencyProperty.Register("Test", listType, typeof(object));
        var targetProvider = new DummyTargetProvider { TargetProperty = dp };
        var serviceProvider = new DummyServiceProvider(targetProvider);

        var ext = new StswMakeListExtension(values);
        var result = ext.ProvideValue(serviceProvider);

        Assert.NotNull(result);
        var list = Assert.IsAssignableFrom<IList>(result);
        Assert.Equal(expected.Length, list.Count);
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], list[i]);
    }

    [Fact]
    public void Constructor_NullValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswMakeListExtension(null!));
    }

    [Fact]
    public void ProvideValue_EmptyValues_ThrowsInvalidOperationException()
    {
        var dp = DependencyProperty.Register("Test", typeof(List<int>), typeof(object));
        var targetProvider = new DummyTargetProvider { TargetProperty = dp };
        var serviceProvider = new DummyServiceProvider(targetProvider);

        var ext = new StswMakeListExtension("");
        Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(serviceProvider));
    }

    [Fact]
    public void ProvideValue_NonListType_ThrowsInvalidOperationException()
    {
        var dp = DependencyProperty.Register("Test", typeof(int), typeof(object));
        var targetProvider = new DummyTargetProvider { TargetProperty = dp };
        var serviceProvider = new DummyServiceProvider(targetProvider);

        var ext = new StswMakeListExtension("1,2,3");
        Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(serviceProvider));
    }

    [Fact]
    public void ProvideValue_NoServiceProvider_ThrowsInvalidOperationException()
    {
        var ext = new StswMakeListExtension("1,2,3");
        Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(null!));
    }

    [Fact]
    public void ProvideValue_CannotConvertType_ThrowsInvalidOperationException()
    {
        var dp = DependencyProperty.Register("Test", typeof(List<StswMakeListExtensionTests>), typeof(object));
        var targetProvider = new DummyTargetProvider { TargetProperty = dp };
        var serviceProvider = new DummyServiceProvider(targetProvider);

        var ext = new StswMakeListExtension("a,b,c");
        Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(serviceProvider));
    }
}
