using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace StswExpress.Tests;
public class StswTranslatorTests
{
    [Fact]
    public void AddOrUpdateTranslation_AddsAndUpdatesTranslation()
    {
        StswTranslator.AddOrUpdateTranslation("TestKey", "en", "TestValue");
        var result = StswTranslator.GetTranslation("TestKey", language: "en");
        Assert.Equal("TestValue", result);

        StswTranslator.AddOrUpdateTranslation("TestKey", "en", "UpdatedValue");
        result = StswTranslator.GetTranslation("TestKey", language: "en");
        Assert.Equal("UpdatedValue", result);
    }

    [Fact]
    public void GetTranslation_ReturnsDefaultValueIfMissing()
    {
        var result = StswTranslator.GetTranslation("MissingKey", defaultValue: "DefaultValue", language: "en");
        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void GetTranslation_ReturnsKeyIfMissingAndNoDefault()
    {
        var result = StswTranslator.GetTranslation("MissingKey", language: "en");
        Assert.Equal("MissingKey", result);
    }

    [Fact]
    public void GetTranslation_AppliesPrefixAndSuffix()
    {
        StswTranslator.AddOrUpdateTranslation("PrefixSuffixKey", "en", "Value");
        var result = StswTranslator.GetTranslation("PrefixSuffixKey", language: "en", prefix: "[", suffix: "]");
        Assert.Equal("[Value]", result);
    }

    [Fact]
    public void AvailableLanguages_ContainsExpectedLanguages()
    {
        Assert.Contains("en", StswTranslator.AvailableLanguages.Keys);
        Assert.Contains("pl", StswTranslator.AvailableLanguages.Keys);
    }

    [Fact]
    public void LoadTranslationsFromJsonString_LoadsTranslations()
    {
        var json = "{\"KeyA\":{\"en\":\"ValueA\",\"pl\":\"WartoœæA\"},\"KeyB\":{\"en\":\"ValueB\"}}";
        StswTranslator.LoadTranslationsFromJsonString(json);

        Assert.Equal("ValueA", StswTranslator.GetTranslation("KeyA", language: "en"));
        Assert.Equal("WartoœæA", StswTranslator.GetTranslation("KeyA", language: "pl"));
        Assert.Equal("ValueB", StswTranslator.GetTranslation("KeyB", language: "en"));
    }

    [Fact]
    public void LoadTranslationsFromJsonString_WithLanguage_LoadsFlatDictionary()
    {
        var json = "{\"FlatKey1\":\"FlatValue1\",\"FlatKey2\":\"FlatValue2\"}";
        StswTranslator.LoadTranslationsFromJsonString(json, language: "fr");

        Assert.Equal("FlatValue1", StswTranslator.GetTranslation("FlatKey1", language: "fr"));
        Assert.Equal("FlatValue2", StswTranslator.GetTranslation("FlatKey2", language: "fr"));
    }

    [Fact]
    public async Task LoadTranslationsFromJsonStringAsync_LoadsTranslations()
    {
        var json = "{\"AsyncKey\":{\"en\":\"AsyncValue\"}}";
        await StswTranslator.LoadTranslationsFromJsonStringAsync(json);

        Assert.Equal("AsyncValue", StswTranslator.GetTranslation("AsyncKey", language: "en"));
    }

    [Fact]
    public async Task LoadTranslationsFromJsonStringAsync_WithLanguage_LoadsFlatDictionary()
    {
        var json = "{\"AsyncFlatKey\":\"AsyncFlatValue\"}";
        await StswTranslator.LoadTranslationsFromJsonStringAsync(json, language: "es");

        Assert.Equal("AsyncFlatValue", StswTranslator.GetTranslation("AsyncFlatKey", language: "es"));
    }

    [Fact]
    public void ExportTranslationsToJson_ExportsToFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            StswTranslator.AddOrUpdateTranslation("ExportKey", "en", "ExportValue");
            StswTranslator.ExportTranslationsToJson(tempFile);

            var json = File.ReadAllText(tempFile);
            Assert.Contains("ExportKey", json);
            Assert.Contains("ExportValue", json);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void PropertyChanged_EventIsRaisedOnCurrentLanguageChange()
    {
        bool eventRaised = false;
        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == "CurrentLanguage")
                eventRaised = true;
        };
        StswTranslator.PropertyChanged += handler;

        var original = StswTranslator.CurrentLanguage;
        StswTranslator.CurrentLanguage = "pl";
        Task.Delay(100).Wait(); // Allow async event

        Assert.True(eventRaised);

        StswTranslator.PropertyChanged -= handler;
        StswTranslator.CurrentLanguage = original;
    }

    [Fact]
    public async Task CustomTranslationLoader_IsInvoked()
    {
        bool loaderInvoked = false;
        StswTranslator.CustomTranslationLoader += async (lang) =>
        {
            loaderInvoked = true;
            return "{\"CustomKey\":{\"" + lang + "\":\"CustomValue\"}}";
        };

        StswTranslator.CurrentLanguage = "en";
        await Task.Delay(100); // Allow async event

        Assert.True(loaderInvoked);
        Assert.Equal("CustomValue", StswTranslator.GetTranslation("CustomKey", language: "en"));

        StswTranslator.CustomTranslationLoader -= null;
    }
}
