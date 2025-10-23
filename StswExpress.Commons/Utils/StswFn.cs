using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.ComponentModel;
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
public static partial class StswFn
{
    [GeneratedRegex(@"^(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-zA-Z0-9-]*[a-zA-Z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)])$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    #region Action functions
    /// <summary>
    /// Attempts to execute the specified action multiple times until it succeeds or reaches the maximum number of attempts.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxTries">The maximum number of attempts before failing.</param>
    /// <param name="msInterval">The delay in milliseconds between attempts.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxTries"/> is less than or equal to 0, or <paramref name="msInterval"/> is negative.</exception>
    public static void TryMultipleTimes(Action action, int maxTries = 5, int msInterval = 200)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTries);
        ArgumentOutOfRangeException.ThrowIfNegative(msInterval);

        for (var attempt = 1; attempt <= maxTries; attempt++)
        {
            try
            {
                action.Invoke();
                return;
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

    /// <summary>
    /// Attempts to execute the specified asynchronous action multiple times until it succeeds or reaches the maximum number of attempts.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="maxTries">The maximum number of attempts before failing.</param>
    /// <param name="msInterval">The delay in milliseconds between attempts.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxTries"/> is less than or equal to 0, or <paramref name="msInterval"/> is negative.</exception>
    public static async Task TryMultipleTimesAsync(Func<Task> action, int maxTries = 5, int msInterval = 200, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTries);
        ArgumentOutOfRangeException.ThrowIfNegative(msInterval);

        for (var attempt = 1; attempt <= maxTries; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await action().ConfigureAwait(false);
                return;
            }
            catch (Exception) when (attempt < maxTries)
            {
                await Task.Delay(msInterval, ct).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
    }
    #endregion

    #region Assembly functions
    /// <summary>
    /// Gets the name of the currently executing application.
    /// </summary>
    public static string? AppName { get; } = Assembly.GetEntryAssembly()?.GetName().Name;

    /// <summary>
    /// Gets the version of the currently executing application.
    /// </summary>
    public static string? AppVersion { get; } = ComputeAppVersion();
    private static string? ComputeAppVersion()
    {
        var ver = Assembly.GetEntryAssembly()?.GetName().Version;
        if (ver is null)
            return null;
        
        var fields = ver.Revision != 0 ? 4
                   : ver.Build != 0 ? 3
                   : ver.Minor != 0 ? 2
                   : 1;
        
        return ver.ToString(fields);
    }

    /// <summary>
    /// Gets the copyright information of the currently executing application.
    /// </summary>
    public static string? AppCopyright { get; } = Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;

    /// <summary>
    /// Determines whether the application is running in debug mode.
    /// </summary>
    public static bool IsInDebugMode { get; } = ComputeIsInDebugMode();
    private static bool ComputeIsInDebugMode()
    {
        var asm = Assembly.GetEntryAssembly();
        if (asm is null)
            return false;

        var da = asm.GetCustomAttribute<DebuggableAttribute>();
        return da is not null && (da.IsJITTrackingEnabled || da.IsJITOptimizerDisabled);
    }

    /// <summary>
    /// Determines whether the application is running in design mode (e.g., within Visual Studio or Blend).
    /// </summary>
    public static bool IsInDesignMode { get; } = ComputeIsInDesignMode();
    private static bool ComputeIsInDesignMode()
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return true;

        var processName = Process.GetCurrentProcess().ProcessName;
        if (processName.Contains("devenv") || processName.Contains("Blend"))
            return true;

        if (SynchronizationContext.Current is not null)
        {
            var isInDesignMode = false;
            SynchronizationContext.Current?.Send(_ => isInDesignMode = processName.Contains("xdesproc", StringComparison.CurrentCultureIgnoreCase), null);
            return isInDesignMode;
        }

        return false;
    }
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

                if (param is IEnumerable enumerable && param is not string)
                {
                    using var enumerableBase = baseList.GetEnumerator();
                    using var enumerableNew = enumerable.Cast<object?>().GetEnumerator();

                    while (enumerableBase.MoveNext())
                    {
                        if (!enumerableNew.MoveNext())
                            break;

                        MergeInto(enumerableBase.Current, enumerableNew.Current, mergePriority);
                    }
                }
                else
                {
                    foreach (var item in baseList)
                        MergeInto(item, param, mergePriority);
                }
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
    /// <param name="target">The target dictionary to merge properties into.</param>
    /// <param name="source">The source object whose properties will be merged into the target dictionary.</param>
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
    /// <param name="source">The source object to convert.</param>
    /// <returns>An <see cref="ExpandoObject"/> containing the properties of the source object.</returns>
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

    #region File functions
    /// <summary>
    /// Checks if a file is currently in use.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><see langword="true"/> if the file is in use; otherwise, <see langword="false"/>.</returns>
    public static bool IsFileInUse(string path)
    {
        if (!File.Exists(path)) return false;

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
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        if (!Path.Exists(path)) return false;

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
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true, Verb = "open" });
            return;
        }

        if (!Path.Exists(path))
            throw new FileNotFoundException($"Path '{path}' not found.", path);

        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true, Verb = "open" });
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

    #region Text functions
    /// <summary>
    /// Splits a string by a specified separator into chunks of size n.
    /// </summary>
    /// <param name="input"> The input string to be split.</param>
    /// <param name="separator"> The separator used to split the string.</param>
    /// <param name="n"> The maximum number of parts in each chunk.</param>
    /// <returns>A list of strings, each containing up to n parts from the original string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is less than or equal to 0.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="separator"/> is <see langword="null"/> or empty.</exception>
    public static List<string> ChunkBySeparator(string input, string separator, int n, StringComparison cmp = StringComparison.Ordinal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        if (string.IsNullOrEmpty(separator))
            throw new ArgumentNullException(nameof(separator));

        var result = new List<string>();
        int count = 0,  lastCut = 0,  index = 0;

        while (index < input.Length)
        {
            var sepIndex = input.IndexOf(separator, index, cmp);
            if (sepIndex < 0) break;

            count++;
            index = sepIndex + separator.Length;

            if (count == n)
            {
                result.Add(input[lastCut..index]);
                lastCut = index;
                count = 0;
            }
        }
        if (lastCut < input.Length)
            result.Add(input[lastCut..]);

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

        var sb = new StringBuilder(text.Length);
        foreach (var c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);

        return sb.ToString().Normalize(NormalizationForm.FormC);
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

        var pattern = $"(?:{Regex.Escape(textToRemove)}){{2,}}";
        return Regex.Replace(originalText, pattern, textToRemove);
    }
    #endregion

    #region Validation functions
    /// <summary>
    /// Validates whether the specified string contains only valid email addresses.
    /// Multiple email addresses can be separated by a specified character.
    /// </summary>
    /// <param name="emails">A string containing one or more email addresses to validate.</param>
    /// <param name="separator">The characters used to separate multiple email addresses.</param>
    /// <returns><see langword="true"/> if all email addresses are valid; otherwise, <see langword="false"/>.</returns>
    [StswPlannedChanges(StswPlannedChanges.Move | StswPlannedChanges.Rework, "Possibly replace with MimeKit implementation.")]
    public static bool AreValidEmails(string emails, char[] separator)
    {
        if (string.IsNullOrWhiteSpace(emails))
            return false;

        var emailList = emails.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return emailList.Length > 0 && emailList.All(EmailRegex().IsMatch);
    }

    /// <summary>
    /// Validates whether the specified email address is in a valid format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns><see langword="true"/> if the email address is valid; otherwise, <see langword="false"/>.</returns>
    [StswPlannedChanges(StswPlannedChanges.Move | StswPlannedChanges.Rework, "Possibly replace with MimeKit implementation.")]
    public static bool IsValidEmail(string email) => EmailRegex().IsMatch(email);

    /*
    /// <summary>
    /// Validates whether the specified email address is in a valid format using MimeKit library.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns><see langword="true"/> if the email address is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && MailboxAddress.TryParse(email, out _);

    /// <summary>
    /// Validates whether the specified string contains only valid email addresses using MimeKit library.
    /// </summary>
    /// <param name="emails">A string containing one or more email addresses to validate.</param>
    /// <returns><see langword="true"/> if all email addresses are valid; otherwise, <see langword="false"/>.</returns>
    public static bool AreValidEmails(string? emails)
        => !string.IsNullOrWhiteSpace(emails)
           && InternetAddressList.TryParse(emails, out var list)
           && list.All(a => a is MailboxAddress);

    /// <summary>
    /// Tries to parse a string containing one or more email addresses into valid and invalid lists using MimeKit library.
    /// </summary>
    /// <param name="input">A string containing one or more email addresses to parse.</param>
    /// <param name="valid">A list to receive valid email addresses as <see cref="MailboxAddress"/> objects.</param>
    /// <param name="invalid">A list to receive invalid email address strings.</param>
    /// <returns><see langword="true"/> if all email addresses are valid; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseEmails(string? input, out List<MailboxAddress> valid, out List<string> invalid)
    {
        valid = [];
        invalid = [];

        if (string.IsNullOrWhiteSpace(input))
            return false;

        if (!InternetAddressList.TryParse(input, out var list))
        {
            invalid.Add(input);
            return false;
        }

        foreach (var addr in list)
        {
            if (addr is MailboxAddress mbox)
                valid.Add(mbox);
            else
                invalid.Add(addr.ToString());
        }

        return invalid.Count == 0 && valid.Count > 0;
    }
    */

    /// <summary>
    /// Validates whether a phone number matches the expected format for a specified country.
    /// </summary>
    /// <param name="number">The phone number to validate.</param>
    /// <param name="countryCode">The country code to validate against (e.g., "PL" for Poland, "UK" for United Kingdom, "US" for United States).</param>
    /// <returns><see langword="true"/> if the phone number is valid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Supports: Poland (PL), United Kingdom (UK), United States (US), Germany (DE), France (FR).
    /// </remarks>
    [StswPlannedChanges(StswPlannedChanges.Remove, "Consider using a dedicated library for phone number validation, such as libphonenumber-csharp.")]
    public static bool IsValidPhoneNumber(string number, string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(number))
            return false;

        static string DigitsWithPlus(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                if (char.IsDigit(ch) || (sb.Length == 0 && ch == '+')) sb.Append(ch);
            return sb.ToString();
        }

        number = DigitsWithPlus(number);
        if (number.StartsWith('+'))
            return number.Length >= 9 && number.Length <= 16;

        return (countryCode?.ToUpperInvariant()) switch
        {
            "PL" => number.Length is 9 || (number.Length is 11 && number.StartsWith("48")),
            "UK" => number.Length is 10 or 11,
            "US" => number.Length is 10,
            "DE" => number.Length is >= 10 and <= 14,
            "FR" => number.Length is 9 or 10,
            _ => number.Length is >= 7 and <= 15,
        };
    }

    /// <summary>
    /// Validates whether the specified URL is in a valid format and uses HTTP or HTTPS.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns><see langword="true"/> if the URL is valid, uses HTTP or HTTPS, and has a valid domain; otherwise, <see langword="false"/>.</returns>
    [StswPlannedChanges(StswPlannedChanges.Remove, "Consider using a dedicated library for URL validation.")]
    public static bool IsValidUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var u)) return false;
        if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps) return false;

        var host = u.IdnHost;
        if (string.IsNullOrWhiteSpace(host))
            return false;

        return Uri.CheckHostName(host) != UriHostNameType.Unknown;
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
