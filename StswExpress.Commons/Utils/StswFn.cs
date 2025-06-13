using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace StswExpress.Commons;
/// <summary>
/// Utility class providing various helper functions for general use.
/// </summary>
public static class StswFn
{
    #region Assembly functions
    /// <summary>
    /// Gets the name of the currently executing application.
    /// </summary>
    /// <returns>The name of the currently executing application, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;

    /// <summary>
    /// Gets the version number of the currently executing application.
    /// </summary>
    /// <returns>The version number of the currently executing application as a string, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? AppVersion()
    {
        if (Assembly.GetEntryAssembly()?.GetName().Version is Version version)
        {
            int?[] versionParts = [version.Major, version.Minor, version.Build, version.Revision];
            for (var i = versionParts.Length - 1; i >= 0; i--)
            {
                if (versionParts[i] == 0)
                    versionParts[i] = null;
                else
                    break;
            }

            return string.Join('.', versionParts.Where(part => part != null));
        }

        return null;
    }

    /// <summary>
    /// Gets the copyright information for the currently executing application.
    /// </summary>
    /// <returns>The copyright information, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? AppCopyright => Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;

    /// <summary>
    /// Determines whether the currently executing assembly was built in debug mode.
    /// This method checks for the presence of the <see cref="DebuggableAttribute"/> 
    /// with JIT tracking enabled.
    /// </summary>
    /// <returns><see langword="true"/> if the assembly was compiled in debug mode, <see langword="false"/> otherwise.</returns>
    public static bool IsInDebug() => Assembly.GetEntryAssembly()?.GetCustomAttributes<DebuggableAttribute>().FirstOrDefault()?.IsJITTrackingEnabled == true;

    /// <summary>
    /// Checks if a UI thread is available using SynchronizationContext.
    /// </summary>
    /// <returns><see langword="true"/> if a UI thread (e.g., WPF, WinForms) is available, <see langword="false"/> otherwise.</returns>
    public static bool IsUiThreadAvailable() => SynchronizationContext.Current is not null;
    #endregion

    #region Convert functions
    /// <summary>
    /// Merges properties of multiple objects into a single dynamic object. 
    /// In case of property name conflicts, properties from later objects will overwrite those from earlier ones.
    /// </summary>
    /// <param name="mergePriority"></param>
    /// <param name="parameters">An array of objects to be merged.</param>
    /// <returns>
    /// A single dynamic object (<see cref="ExpandoObject"/>) if the first parameter is not a collection;
    /// otherwise a <see cref="List{ExpandoObject}"/> containing merged elements.
    /// </returns>
    public static dynamic MergeObjects(StswMergePriority mergePriority, params object?[] parameters)
    {
        if (parameters.Length == 0)
            return new ExpandoObject();

        if (parameters[0] is IEnumerable firstEnumerable && parameters[0] is not string)
        {
            var baseList = firstEnumerable.Cast<object?>().Select(ToExpando).ToList();

            for (var i = 1; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param == null)
                    continue;

                if (param is IEnumerable list && param is not string)
                {
                    var items = list.Cast<object?>().ToList();
                    for (var j = 0; j < baseList.Count && j < items.Count; j++)
                        MergeInto(baseList[j], items[j], mergePriority);
                }
                else
                    foreach (var item in baseList)
                        MergeInto(item, param, mergePriority);
            }

            return baseList;
        }
        else
        {
            var merged = new ExpandoObject();
            foreach (var param in parameters)
                MergeInto(merged, param, mergePriority);
            return merged;
        }
    }

    /// <summary>
    /// Merges properties of multiple objects into a single dynamic object. 
    /// In case of property name conflicts, properties from later objects will overwrite those from earlier ones.
    /// </summary>
    /// <param name="parameters">An array of objects to be merged.</param>
    /// <returns>
    /// A single dynamic object (<see cref="ExpandoObject"/>) if the first parameter is not a collection;
    /// otherwise a <see cref="List{ExpandoObject}"/> containing merged elements.
    /// </returns>
    public static dynamic MergeObjects(params object?[] parameters) => MergeObjects(StswMergePriority.Last, parameters);

    /// <summary>
    /// Merges properties of a source object into a target dictionary.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private static void MergeInto(IDictionary<string, object?> target, object? source, StswMergePriority mergePriority)
    {
        if (source == null)
            return;

        foreach (var prop in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            var value = prop.GetValue(source);

            switch (mergePriority)
            {
                case StswMergePriority.First:
                    if (!target.ContainsKey(prop.Name))
                        target[prop.Name] = value;
                    break;
                case StswMergePriority.FirstExceptNull:
                    if (!target.TryGetValue(prop.Name, out var targetValue) || targetValue == null)
                        target[prop.Name] = value;
                    break;
                case StswMergePriority.Last:
                    target[prop.Name] = value;
                    break;
                case StswMergePriority.LastExceptNull:
                    if (!target.ContainsKey(prop.Name) || value != null)
                        target[prop.Name] = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Converts an object to an <see cref="ExpandoObject"/> by copying its properties.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static ExpandoObject ToExpando(object? source)
    {
        var expando = new ExpandoObject() as IDictionary<string, object?>;
        if (source == null)
            return (ExpandoObject)expando;

        foreach (var prop in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            expando[prop.Name] = prop.GetValue(source);
        }

        return (ExpandoObject)expando;
    }
    #endregion

    #region DateTime functions
    /// <summary>
    /// Generates a list of unique year and month tuples within a specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of tuples, where each tuple contains the year and month within the specified date range.</returns>
    public static List<(int Year, int Month)> GetUniqueMonthsFromRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            return [];

        var monthsDifference = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
        var months = new List<(int Year, int Month)>(monthsDifference);

        var current = new DateTime(startDate.Year, startDate.Month, 1);

        while (current <= endDate)
        {
            months.Add((current.Year, current.Month));
            current = current.AddMonths(1);
        }

        return months;
    }
    #endregion

    #region File functions
    /// <summary>
    /// Checks if a file is currently in use.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><see langword="true"/> if the file is in use; otherwise, <see langword="false"/>.</returns>
    public static bool IsFileInUse(string path)
    {
        if (!File.Exists(path))
            return false;

        SafeFileHandle? handle = null;

        try
        {
            handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            handle?.Dispose();
        }
    }

    /// <summary>
    /// Moves a file or directory to the recycle bin.
    /// </summary>
    /// <param name="path"> The path to the file or directory to be moved to the recycle bin.</param>
    public static bool MoveToRecycleBin(string path)
    {
        if (!Path.Exists(path))
            return false;

        var shf = new SHFILEOPSTRUCT
        {
            wFunc = FO_DELETE,
            pFrom = path + '\0' + '\0',
            fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION
        };
        return SHFileOperation(ref shf) == 0;
    }

    /// <summary>
    /// Opens a file in its associated application, a hyperlink in the default web browser, or a folder in Windows Explorer.
    /// </summary>
    /// <param name="path">The path to the file, hyperlink, or folder to be opened.</param>
    public static void OpenPath(string path)
    {
        if (!Path.Exists(path))
            throw new FileNotFoundException($"Path '{path}' not found.", path);

        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            }
        }.Start();
    }

    /// <summary>
    /// Prints a file in its associated application.
    /// </summary>
    /// <param name="path">The path to the file to be printed.</param>
    public static void PrintFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File '{path}' not found.", path);

        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "print"
            }
        }.Start();
    }

    /// <summary>
    /// Opens Windows Explorer and selects the specified file.
    /// </summary>
    /// <param name="path">The path to the file to be selected in Windows Explorer.</param>
    public static void SelectPathInExplorer(string path)
    {
        if (!Path.Exists(path))
            throw new FileNotFoundException($"Path '{path}' not found.", path);

        Process.Start("explorer.exe", $"/select,\"{path}\"");
    }
    #endregion

    #region List functions
    /// <summary>
    /// Generates a collection of random items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of items to generate.</typeparam>
    /// <param name="count">The number of random items to generate.</param>
    /// <returns>An enumerable collection of randomly generated items.</returns>
    public static IEnumerable<T> CreateRandomItems<T>(int count) => StswRandomGenerator.CreateRandomItems<T>(count);
    #endregion

    #region Text functions
    /// <summary>
    /// Splits a string by a specified separator into chunks of size n.
    /// </summary>
    /// <param name="input"> The input string to be split.</param>
    /// <param name="separator"> The separator used to split the string.</param>
    /// <param name="n"> The maximum number of parts in each chunk.</param>
    /// <returns>A list of strings, each containing up to n parts from the original string.</returns>
    /// <exception cref="ArgumentException">Thrown when n is less than or equal to 0.</exception>
    public static List<string> ChunkBySeparator(string input, string separator, int n)
    {
        if (n <= 0)
            throw new ArgumentException($"Parameter {nameof(n)} must be greater than 0.", nameof(n));
        if (string.IsNullOrEmpty(separator))
            throw new ArgumentException("Separator cannot be null or empty.", nameof(separator));

        var result = new List<string>();
        var count = 0;
        var lastCutPosition = 0;
        var index = 0;

        while (index < input.Length)
        {
            var sepIndex = input.IndexOf(separator, index);
            if (sepIndex == -1)
                break;

            count++;
            index = sepIndex + separator.Length;

            if (count == n)
            {
                var cutPosition = index;
                var chunk = input[lastCutPosition..cutPosition];
                result.Add(chunk);
                lastCutPosition = cutPosition;
                count = 0;
            }
        }

        if (lastCutPosition < input.Length)
            result.Add(input[lastCutPosition..]);

        return result;
    }

    /// <summary>
    /// Replaces diacritical marks in a string with their ASCII equivalents.
    /// Uses Unicode normalization to decompose characters and remove 
    /// non-spacing marks.
    /// Useful for text search and standardization.
    /// </summary>
    /// <param name="text">The input string containing diacritics.</param>
    /// <returns>The normalized string with diacritics replaced by ASCII characters.</returns>
    public static string NormalizeDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(text.Length);

        foreach (var c in normalizedString)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Removes consecutive occurrences of a specified string from the original text.
    /// </summary>
    /// <param name="originalText">The original text to process.</param>
    /// <param name="textToRemove">The string to remove when consecutive occurrences are found.</param>
    /// <returns>The processed string with consecutive occurrences removed.</returns>
    public static string RemoveConsecutiveText(string originalText, string textToRemove)
    {
        if (string.IsNullOrEmpty(textToRemove))
            return originalText;

        var pattern = $"({Regex.Escape(textToRemove)})+";
        return Regex.Replace(originalText, pattern, textToRemove);
    }
    #endregion

    #region Universal functions
    /// <summary>
    /// Attempts to execute the specified action multiple times until it succeeds or reaches the maximum number of attempts.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxTries">The maximum number of attempts before failing. Defaults to 5.</param>
    /// <param name="msInterval">The delay in milliseconds between attempts. Defaults to 200ms.</param>
    /// <remarks>
    /// If the action throws an exception, it will be retried up to <paramref name="maxTries"/> times.
    /// </remarks>
    public static void TryMultipleTimes(Action action, int maxTries = 5, int msInterval = 200)
    {
        for (var attempt = 1; attempt <= maxTries; attempt++)
        {
            try
            {
                action.Invoke();
                break;
            }
            catch (Exception) when (attempt < maxTries)
            {
                Thread.Sleep(msInterval);
            }
            catch
            {
                throw;
            }
        }
    }
    #endregion

    #region Validation functions
    /// <summary>
    /// Validates whether the specified string contains only valid email addresses.
    /// Multiple email addresses can be separated by a specified character.
    /// </summary>
    /// <param name="emails">A string containing one or more email addresses to validate.</param>
    /// <param name="separator">The character used to separate multiple email addresses. Defaults to ','.</param>
    /// <returns><see langword="true"/> if all email addresses are valid; otherwise, <see langword="false"/>.</returns>
    public static bool AreValidEmails(string emails, char separator = ',')
    {
        if (string.IsNullOrWhiteSpace(emails))
            return false;

        var emailList = emails.Split(separator, StringSplitOptions.TrimEntries);
        var emailValidator = new EmailAddressAttribute();
        return emailList.All(emailValidator.IsValid);
    }

    /// <summary>
    /// Validates whether the specified email address is in a valid format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns><see langword="true"/> if the email address is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidEmail(string email) => new EmailAddressAttribute().IsValid(email);

    /// <summary>
    /// Validates whether a phone number matches the expected format for a specified country.
    /// </summary>
    /// <param name="number">The phone number to validate.</param>
    /// <param name="countryCode">The country code (e.g., "PL" for Poland, "US" for United States).</param>
    /// <returns><see langword="true"/> if the phone number is valid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Supports PL (Poland), UK (United Kingdom), US (United States), and generic validation for other countries.
    /// </remarks>
    public static bool IsValidPhoneNumber(string number, string countryCode)
    {
        var digits = new string([.. number.Where(char.IsDigit)]);

        switch (countryCode.ToUpper())
        {
            case "PL":
                if (digits.Length == 9)
                    return true;

                if (digits.Length == 11 && digits.StartsWith("48"))
                    return true;

                return false;
            case "UK":
                return digits.Length == 10 || digits.Length == 11;
            case "US":
                return digits.Length == 10;
            default:
                return digits.Length >= 7 && digits.Length <= 15;
        }
    }

    /// <summary>
    /// Validates whether the specified URL is in a valid format and uses HTTP or HTTPS.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns><see langword="true"/> if the URL is valid, uses HTTP or HTTPS, and has a valid domain; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var result)
         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
        {
            var host = result.Host;
            return !string.IsNullOrWhiteSpace(host) && host.Contains('.') && !host.Any(char.IsWhiteSpace);
        }
        return false;
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string lpszProgressTitle;
    }

    private const uint FO_DELETE = 3;
    private const ushort FOF_ALLOWUNDO = 0x0040;
    private const ushort FOF_NOCONFIRMATION = 0x0010;

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
}
