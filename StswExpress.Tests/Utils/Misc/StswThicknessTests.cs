using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace StswExpress.Tests.Utils.Misc;
public class StswThicknessTests
{
    [Fact]
    public void Constructors_SetProperties_Correctly()
    {
        var a = new StswThickness(1);
        Assert.Equal(1d, a.Left);
        Assert.Equal(1d, a.Top);
        Assert.Equal(1d, a.Right);
        Assert.Equal(1d, a.Bottom);
        Assert.Equal(1d, a.Inside);

        var b = new StswThickness(1, 2);
        Assert.Equal(1d, b.Left);
        Assert.Equal(1d, b.Top);
        Assert.Equal(1d, b.Right);
        Assert.Equal(1d, b.Bottom);
        Assert.Equal(2d, b.Inside);

        var c = new StswThickness(1, 2, 3);
        Assert.Equal(1d, c.Left);
        Assert.Equal(2d, c.Top);
        Assert.Equal(1d, c.Right);
        Assert.Equal(2d, c.Bottom);
        Assert.Equal(3d, c.Inside);

        var d = new StswThickness(1, 2, 3, 4);
        Assert.Equal(1d, d.Left);
        Assert.Equal(2d, d.Top);
        Assert.Equal(3d, d.Right);
        Assert.Equal(4d, d.Bottom);
        Assert.Equal(0d, d.Inside);

        var e = new StswThickness(1, 2, 3, 4, 5);
        Assert.Equal(1d, e.Left);
        Assert.Equal(2d, e.Top);
        Assert.Equal(3d, e.Right);
        Assert.Equal(4d, e.Bottom);
        Assert.Equal(5d, e.Inside);
    }

    [Fact]
    public void ToString_WithInvariantCulture_ReturnsExpectedFormat()
    {
        var s = new StswThickness(1, 2, 3, 4, 5);
        var str = s.ToString(CultureInfo.InvariantCulture);

        // InvariantCulture list separator is ","
        Assert.Equal("1, 2, 3, 4, 5", str);
    }

    [Fact]
    public void TypeConverter_CanConvertFromTo()
    {
        var conv = new StswThicknessConverter();
        Assert.True(conv.CanConvertFrom(null, typeof(string)));
        Assert.True(conv.CanConvertFrom(null, typeof(Thickness)));
        Assert.True(conv.CanConvertTo(null, typeof(string)));
        Assert.True(conv.CanConvertTo(null, typeof(Thickness)));
    }

    [Fact]
    public void TypeConverter_ConvertFrom_String_VariousTokenCounts()
    {
        var conv = new StswThicknessConverter();

        var one = (StswThickness)conv.ConvertFrom(null, CultureInfo.InvariantCulture, "1")!;
        Assert.Equal(new StswThickness(1d, 1d, 1d, 1d, 1d), one);

        var two = (StswThickness)conv.ConvertFrom(null, CultureInfo.InvariantCulture, "1 2")!;
        Assert.Equal(new StswThickness(1d, 1d, 1d, 1d, 2d), two);

        var three = (StswThickness)conv.ConvertFrom(null, CultureInfo.InvariantCulture, "1,2,3")!;
        Assert.Equal(new StswThickness(1d, 2d, 1d, 2d, 3d), three);

        var four = (StswThickness)conv.ConvertFrom(null, CultureInfo.InvariantCulture, "1; 2;3;4")!;
        Assert.Equal(new StswThickness(1d, 2d, 3d, 4d, 0d), four);

        var five = (StswThickness)conv.ConvertFrom(null, CultureInfo.InvariantCulture, "1, 2, 3, 4, 5")!;
        Assert.Equal(new StswThickness(1d, 2d, 3d, 4d, 5d), five);
    }

    [Fact]
    public void TypeConverter_ConvertFrom_Invalid_ThrowsFormatException()
    {
        var conv = new StswThicknessConverter();
        Assert.Throws<FormatException>(() => conv.ConvertFrom(null, CultureInfo.InvariantCulture, "1, notanumber"));
    }

    [Fact]
    public void TypeConverter_ConvertFrom_Thickness_ReturnsExpected()
    {
        var conv = new StswThicknessConverter();
        var thickness = new Thickness(1, 2, 3, 4);
        var result = conv.ConvertFrom(null, CultureInfo.InvariantCulture, thickness);
        Assert.IsType<StswThickness>(result);
        var st = (StswThickness)result!;
        Assert.Equal(new StswThickness(1d, 2d, 3d, 4d, 0d), st);
    }

    [Fact]
    public void TypeConverter_ConvertTo_String_And_Thickness()
    {
        var conv = new StswThicknessConverter();
        var st = new StswThickness(1, 2, 3, 4, 5);

        var s = conv.ConvertTo(null, CultureInfo.InvariantCulture, st, typeof(string));
        Assert.Equal("1, 2, 3, 4, 5", s);

        var t = conv.ConvertTo(null, CultureInfo.InvariantCulture, st, typeof(Thickness));
        Assert.IsType<Thickness>(t);
        var thickness = (Thickness)t!;
        Assert.Equal(1d, thickness.Left);
        Assert.Equal(2d, thickness.Top);
        Assert.Equal(3d, thickness.Right);
        Assert.Equal(4d, thickness.Bottom);
    }

    [Fact]
    public void Equality_And_HashCode_Behavior()
    {
        var a = new StswThickness(1, 2, 3, 4, 5);
        var b = new StswThickness(1, 2, 3, 4, 5);
        var c = new StswThickness(1, 2, 3, 4, 6);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());

        Assert.False(a.Equals(c));
        Assert.False(a == c);
        Assert.True(a != c);
    }

    [Fact]
    public void Deconstruct_Works()
    {
        var s = new StswThickness(1, 2, 3, 4, 5);
        s.Deconstruct(out var l, out var t, out var r, out var b, out var inside);

        Assert.Equal(1d, l);
        Assert.Equal(2d, t);
        Assert.Equal(3d, r);
        Assert.Equal(4d, b);
        Assert.Equal(5d, inside);
    }

    [Fact]
    public void ExplicitConversions_ToAndFrom_Thickness()
    {
        var st = new StswThickness(1, 2, 3, 4, 5);
        var th = (Thickness)st;
        Assert.Equal(1d, th.Left);
        Assert.Equal(2d, th.Top);
        Assert.Equal(3d, th.Right);
        Assert.Equal(4d, th.Bottom);

        var st2 = (StswThickness)th;
        Assert.Equal(1d, st2.Left);
        Assert.Equal(2d, st2.Top);
        Assert.Equal(3d, st2.Right);
        Assert.Equal(4d, st2.Bottom);
        Assert.Equal(0d, st2.Inside);
    }
}
