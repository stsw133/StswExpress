using StswExpress.Commons;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress.Commons.Tests.Utils;
public class StswFnTests
{
    [Fact]
    public void Do_ExecutesIfTrueOrIfFalse()
    {
        bool trueCalled = false, falseCalled = false;
        true.Do(() => trueCalled = true, () => falseCalled = true);
        Assert.True(trueCalled);
        Assert.False(falseCalled);

        trueCalled = false; falseCalled = false;
        false.Do(() => trueCalled = true, () => falseCalled = true);
        Assert.False(trueCalled);
        Assert.True(falseCalled);
    }

    [Fact]
    public void TryMultipleTimes_SucceedsOnFirstTry()
    {
        int count = 0;
        StswFn.TryMultipleTimes(() => count++);
        Assert.Equal(1, count);
    }

    [Fact]
    public void TryMultipleTimes_RetriesOnException()
    {
        int count = 0;
        StswFn.TryMultipleTimes(() =>
        {
            count++;
            if (count < 3) throw new InvalidOperationException();
        }, maxTries: 5, msInterval: 1);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task TryMultipleTimesAsync_SucceedsOnFirstTry()
    {
        int count = 0;
        await StswFn.TryMultipleTimesAsync(async () => { count++; await Task.CompletedTask; });
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task TryMultipleTimesAsync_RetriesOnException()
    {
        int count = 0;
        await StswFn.TryMultipleTimesAsync(async () =>
        {
            count++;
            if (count < 3) throw new InvalidOperationException();
            await Task.CompletedTask;
        }, maxTries: 5, msInterval: 1);
        Assert.Equal(3, count);
    }

    [Fact]
    public void AppName_ReturnsNonNull()
    {
        var name = StswFn.AppName();
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void AppVersion_ReturnsNonNull()
    {
        var version = StswFn.AppVersion();
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void AppCopyright_ReturnsStringOrNull()
    {
        var copyright = StswFn.AppCopyright;
        Assert.True(copyright == null || copyright.Length > 0);
    }

    [Fact]
    public void IsInDebug_ReturnsBool()
    {
        Assert.IsType<bool>(StswFn.IsInDebug);
    }

    [Fact]
    public void IsUiThreadAvailable_ReturnsBool()
    {
        Assert.IsType<bool>(StswFn.IsUiThreadAvailable());
    }

    [Fact]
    public void MergeObjects_MergesProperties()
    {
        var obj1 = new { A = 1, B = 2 };
        var obj2 = new { B = 3, C = 4 };
        dynamic merged = StswFn.MergeObjects(obj1, obj2);
        Assert.Equal(1, merged.A);
        Assert.Equal(3, merged.B);
        Assert.Equal(4, merged.C);
    }

    [Fact]
    public void MergeObjects_ListMergesProperties()
    {
        var list1 = new[] { new { A = 1 }, new { A = 2 } };
        var list2 = new[] { new { B = 3 }, new { B = 4 } };
        var result = StswFn.MergeObjects(list1, list2);
        Assert.IsType<List<ExpandoObject>>(result);
        var expandoList = (List<ExpandoObject>)result;
        Assert.Equal(2, expandoList.Count);
        Assert.Equal(1, ((IDictionary<string, object?>)expandoList[0])["A"]);
        Assert.Equal(3, ((IDictionary<string, object?>)expandoList[0])["B"]);
    }

    [Fact]
    public void GetUniqueMonthsFromRange_ReturnsCorrectMonths()
    {
        var months = StswFn.GetUniqueMonthsFromRange(new DateTime(2023, 1, 1), new DateTime(2023, 3, 1));
        Assert.Equal(3, months.Count);
        Assert.Contains((2023, 1), months);
        Assert.Contains((2023, 2), months);
        Assert.Contains((2023, 3), months);
    }

    [Fact]
    public void IsFileInUse_ReturnsFalseForNonexistentFile()
    {
        Assert.False(StswFn.IsFileInUse("nonexistent.file"));
    }

    [Fact]
    public void ChunkBySeparator_SplitsCorrectly()
    {
        var input = "a,b,c,d,e";
        var chunks = StswFn.ChunkBySeparator(input, ",", 2);
        Assert.Equal(3, chunks.Count);
        Assert.Equal("a,b,", chunks[0]);
        Assert.Equal("c,d,", chunks[1]);
        Assert.Equal("e", chunks[2]);
    }

    [Fact]
    public void NormalizeDiacritics_RemovesDiacritics()
    {
        var input = "¹êæœñó³¿Ÿ";
        var normalized = StswFn.NormalizeDiacritics(input);
        Assert.DoesNotContain('¹', normalized);
        Assert.DoesNotContain('ê', normalized);
    }

    [Fact]
    public void RemoveConsecutiveText_RemovesConsecutive()
    {
        var input = "abcabcabc";
        var result = StswFn.RemoveConsecutiveText(input, "abc");
        Assert.Equal("abc", result);
    }

    [Fact]
    public void AreValidEmails_ValidatesMultipleEmails()
    {
        var emails = "test@example.com;other@example.com";
        Assert.True(StswFn.AreValidEmails(emails, new[] { ';' }));
    }

    [Fact]
    public void IsValidEmail_ValidatesSingleEmail()
    {
        Assert.True(StswFn.IsValidEmail("test@example.com"));
        Assert.False(StswFn.IsValidEmail("not-an-email"));
    }

    [Theory]
    [InlineData("123456789", "PL", true)]
    [InlineData("+48123456789", "PL", true)]
    [InlineData("1234567890", "US", true)]
    [InlineData("12345", "US", false)]
    public void IsValidPhoneNumber_ValidatesNumbers(string number, string country, bool expected)
    {
        Assert.Equal(expected, StswFn.IsValidPhoneNumber(number, country));
    }

    [Fact]
    public void IsValidUrl_ValidatesUrl()
    {
        Assert.True(StswFn.IsValidUrl("http://example.com"));
        Assert.True(StswFn.IsValidUrl("https://example.com"));
        Assert.False(StswFn.IsValidUrl("ftp://example.com"));
        Assert.False(StswFn.IsValidUrl("not-a-url"));
    }
}
