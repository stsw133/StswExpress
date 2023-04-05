using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

public static class StswFn
{
    /// App: name & version & name + version & copyright
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;
    public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0").TrimEnd(".0").TrimEnd(".0");
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";
    public static string? AppCopyright => Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;

    /// App: database connection & mail config
    public static StswDatabase? AppDB { get; set; } = new();
    public static StswMailConfig? AppMC { get; set; } = new();

    /// Starting functions that should be placed in constructor of App class (if you want to have light and dark theme)
    public static void AppStart(Application app, string saltKey, string hashKey)
    {
        /// hash keys
        StswSecurity.SaltKey = saltKey;
        StswSecurity.HashKey = hashKey;

        /// merged dictionaries
        if (!app.Resources.MergedDictionaries.Any(x => x is Theme))
            app.Resources.MergedDictionaries.Add(new Theme());
        ((Theme)app.Resources.MergedDictionaries.First(x => x is Theme)).Color = Settings.Default.Theme < 0 ? (ThemeColor)GetWindowsTheme() : (ThemeColor)Settings.Default.Theme;

        if (!app.Resources.MergedDictionaries.Any(x => x.Source == new Uri("pack://application:,,,/StswExpress;component/Themes/Generic.xaml")))
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/StswExpress;component/Themes/Generic.xaml") });

        /// global commands
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(StswGlobalCommands.Fullscreen, (s, e) => {
            if (s is StswWindow stsw)
                stsw.Fullscreen = !stsw.Fullscreen;
        }));

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        /// on exit
        app.Exit += (sender, e) => Settings.Default.Save();
    }

    /// Calculate string into double
    public static bool TryCalculateString(string expression, out double result, CultureInfo? culture = null)
    {
        try
        {
            /// get culture
            culture ??= CultureInfo.CurrentCulture;
            var decSign = culture.NumberFormat.NumberDecimalSeparator[0];
            var negSign = culture.NumberFormat.NegativeSign[0];

            /// remove unnecessary characters
            expression = expression
                .Replace(Environment.NewLine, string.Empty)
                .Replace("\t", string.Empty)
                .Replace(" ", string.Empty);

            /// find first and last number
            int i1, i2;
            int findFirstAndLastIndex(char[] signs)
            {
                var iSign = (expression[0] == negSign ? "_" + expression[1..] : expression).IndexOfAny(signs);
                i1 = iSign;
                while (i1 > 0
                        && (char.IsDigit(expression[i1 - 1])
                         || expression[i1 - 1] == decSign
                         || (expression[i1 - 1] == negSign && (i1 < 2 || !char.IsDigit(expression[i1 - 2]))))
                      )
                    i1--;

                i2 = iSign;
                do { i2++; } while (i2 == expression.Length - 1
                                || (i2 < expression.Length && (char.IsDigit(expression[i2]) || expression[i2] == decSign || (!char.IsDigit(expression[i2 - 1]) && expression[i2] == negSign))));

                return iSign;
            }
            /// replace
            double value;
            void expressionReplace()
            {
                var addPlusSign = i1 > 0 && char.IsDigit(expression[i1 - 1]);
                expression = expression.Remove(i1, i2 - i1);
                expression = expression.Insert(i1, $"{(value > 0 && addPlusSign ? "+" : string.Empty)}{value}");
            }

            /// first ( )
            while (expression.Any(x => x.In('(', ')')))
            {
                /// indexes
                i2 = expression.IndexOf(')') + 1;
                i1 = expression[..i2].LastIndexOf('(');

                /// result
                var part0 = expression[i1..i2];
                TryCalculateString(part0[1..^1], out value);
                expressionReplace();
            }
            /// next ^
            while (expression[1..].Any(x => x.In('^')))
            {
                var iSign = findFirstAndLastIndex(new char[] { '^' });
                var number1 = Convert.ToDouble(expression[i1..iSign], culture);
                var number2 = Convert.ToDouble(expression[(iSign + 1)..i2], culture);
                value = Math.Pow(number1, number2);
                expressionReplace();
            }
            /// next * /
            while (expression[1..].Any(x => x.In('*', '/')))
            {
                var iSign = findFirstAndLastIndex(new char[] { '*', '/' });
                var number1 = Convert.ToDouble(expression[i1..iSign], culture);
                var number2 = Convert.ToDouble(expression[(iSign + 1)..i2], culture);
                value = expression[iSign] == '*' ? number1 * number2 : number1 / number2;
                expressionReplace();
            }
            /// next + -
            while (expression[1..].Any(x => x.In('+', '-')))
            {
                var iSign = findFirstAndLastIndex(new char[] { '+', '-' });
                var number1 = Convert.ToDouble(expression[i1..iSign], culture);
                var number2 = Convert.ToDouble(expression[(iSign + 1)..i2], culture);
                value = expression[iSign] == '+' ? number1 + number2 : number1 - number2;
                expressionReplace();
            }

            result = Convert.ToDouble(expression, culture);
            return true;
        }
        catch
        {
            result = 0;
            return false;
        }
    }

    /// Gets next value of T type enum
    public static T GetNextEnumValue<T>(T value, int count = 1) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        var index = Array.IndexOf(values, value);
        var nextIndex = (index + count) % values.Length;
        return values[nextIndex];
    }

    /// Gets system theme
    public static int GetWindowsTheme()
    {
        var theme = 0;

        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var registryValueObject = key?.GetValue("AppsUseLightTheme");
                if (registryValueObject == null)
                    return 0;

                var registryValue = (int)registryValueObject;

                //if (SystemParameters.HighContrast)
                //    theme = 2;

                theme = registryValue > 0 ? 0 : 1;
            }

            return theme;
        }
        catch
        {
            return theme;
        }
    }

    /// Opens context menu of a framework element.
    public static void OpenContextMenu(object sender)
    {
        if (sender is FrameworkElement f)
        {
            f.ContextMenu.PlacementTarget = f;
            f.ContextMenu.IsOpen = true;
        }
    }

    /// Opens file from path.
    public static void OpenFile(string path)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "open";
        process.Start();
    }

    /// Remove multiple instances of string in input when they are next to each other.
    public static string RemoveMultipleInstances(string input, string remove)
    {
        var result = input;

        while (result.Contains(remove + remove))
            result = result.Replace(remove + remove, remove);

        return result;
    }
}
