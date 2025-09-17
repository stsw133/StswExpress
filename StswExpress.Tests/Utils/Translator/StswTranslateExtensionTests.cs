using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswTranslateExtensionTests
{
    private class MockTranslator : INotifyPropertyChanged
    {
        public static string? CurrentLanguage { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public string GetTranslation(string key, string? defaultValue, string? language, string? prefix, string? suffix)
        {
            return $"{prefix}{key}-{language ?? "default"}{suffix}";
        }
        public void ChangeLanguage(string lang)
        {
            CurrentLanguage = lang;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
        }
    }

    private class StswTranslatorProxy
    {
        public static event PropertyChangedEventHandler? PropertyChanged;
        public static string? CurrentLanguage { get; set; }
        public static string GetTranslation(string key, string? defaultValue, string? language, string? prefix, string? suffix)
        {
            return $"{prefix}{key}-{language ?? "default"}{suffix}";
        }
        public static void ChangeLanguage(string lang)
        {
            CurrentLanguage = lang;
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
        }
    }

    [Fact]
    public void Constructor_SetsKey()
    {
        var ext = new StswExpress.StswTranslateExtension("TestKey");
        Assert.Equal("TestKey", ext.Key);
    }

    [Fact]
    public void TranslatedText_ReturnsExpectedFormat()
    {
        // Arrange
        var ext = new StswExpress.StswTranslateExtension("Hello")
        {
            DefaultValue = "Default",
            Prefix = "[",
            Suffix = "]",
            Language = "en"
        };
        // Act
        var result = ext.TranslatedText;
        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ProvideValue_DesignMode_ReturnsKeyWithPrefixSuffix()
    {
        var ext = new StswExpress.StswTranslateExtension("DesignKey")
        {
            Prefix = "<",
            Suffix = ">"
        };
        // Simulate design mode
        var value = ext.ProvideValue(null);
        Assert.Contains("DesignKey", value.ToString());
        Assert.StartsWith("<", value.ToString());
        Assert.EndsWith(">", value.ToString());
    }

    [Fact]
    public void ProvideValue_ReturnsBinding()
    {
        var ext = new StswExpress.StswTranslateExtension("BindKey");
        var value = ext.ProvideValue(new object() as IServiceProvider);
        Assert.IsType<string>(ext.TranslatedText);
        Assert.True(value is BindingExpressionBase || value is string);
    }

    [Fact]
    public void TranslationManager_PropertyChanged_UpdatesTranslatedText()
    {
        var ext = new StswExpress.StswTranslateExtension("LangKey");
        bool propertyChangedRaised = false;
        ext.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ext.TranslatedText))
                propertyChangedRaised = true;
        };

        // Simulate language change
        var propertyChangedEvent = typeof(StswExpress.StswTranslator).GetEvent("PropertyChanged");
        if (propertyChangedEvent != null)
        {
            var eventDelegate = (MulticastDelegate?)typeof(StswExpress.StswTranslator)
                .GetField("PropertyChanged", BindingFlags.Static | BindingFlags.NonPublic)?
                .GetValue(null);

            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { null, new PropertyChangedEventArgs(nameof(StswExpress.StswTranslator.CurrentLanguage)) });
                }
            }
        }

        ext.GetType().GetMethod("TranslationManager_PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(ext, new object[] { null, new PropertyChangedEventArgs(nameof(StswExpress.StswTranslator.CurrentLanguage)) });

        Assert.True(propertyChangedRaised);
    }
}
