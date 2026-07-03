using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Provides a simple way to write log messages and handle logging errors.
/// </summary>
public static class StswLog
{
    private static readonly Dictionary<Guid, Dictionary<StswInfoType, int>> _logCounters = [];
    private static readonly SemaphoreSlim _logSemaphore = new(1, 1);
    private static int _failureCount = 0;

    static StswLog()
    {
        EnsureLogDirectoryExists();
        StswLogArchiving.Initialize();
    }

    /// <summary>
    /// Gets the current configuration settings for logging.
    /// </summary>
    public static StswLogConfig Config { get; } = new();

    /// <summary>
    /// Occurs when the <see cref="StswLogTarget.Custom"/> target is enabled and a log entry is written.
    /// </summary>
    public static event Action<StswLogItem>? LogWritten;

    /// <summary>
    /// Shared file semaphore used by logging and archiving operations.
    /// </summary>
    internal static SemaphoreSlim FileSemaphore => _logSemaphore;

    #region Import
    /// <summary>
    /// Synchronously imports log entries from log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    /// <returns>An enumerable collection of <see cref="StswLogItem"/> objects representing the imported log entries.</returns>
    /// <remarks>
    /// This method blocks the calling thread while the log files are being read. 
    /// For non-blocking operations, consider using the asynchronous version <see cref="ImportListAsync"/>.
    /// </remarks>
    public static IEnumerable<StswLogItem> ImportList(DateTime dateFrom, DateTime dateTo)
    {
        var logItems = new List<StswLogItem>();

        foreach (var filePath in GetFilesInRange(dateFrom, dateTo))
        {
            if (!File.Exists(filePath))
                continue;

            using var reader = new StreamReader(filePath);
            var currentBlock = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (IsNewLogEntryLine(line, out _))
                {
                    TryAddLogBlock(currentBlock, logItems);
                    currentBlock.Clear();
                }

                currentBlock.Add(line);
            }

            TryAddLogBlock(currentBlock, logItems);
        }

        return logItems;
    }

    /// <summary>
    /// Asynchronously imports log entries from log files within the specified date range.
    /// </summary>
    /// <param name="dateFrom">The start date of the range.</param>
    /// <param name="dateTo">The end date of the range.</param>
    /// <returns>A task representing the asynchronous operation, with a result of a collection of log entries.</returns>
    public static async Task<IEnumerable<StswLogItem>> ImportListAsync(DateTime dateFrom, DateTime dateTo)
    {
        var logItems = new List<StswLogItem>();

        foreach (var filePath in GetFilesInRange(dateFrom, dateTo))
        {
            if (!File.Exists(filePath))
                continue;

            using var reader = new StreamReader(filePath);
            var currentBlock = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (IsNewLogEntryLine(line, out _))
                {
                    TryAddLogBlock(currentBlock, logItems);
                    currentBlock.Clear();
                }

                currentBlock.Add(line);
            }

            TryAddLogBlock(currentBlock, logItems);
        }

        return logItems;
    }

    /// <summary>
    /// Gets a list of log files within the specified date range.
    /// </summary>
    /// <param name="from">The start date of the range.</param>
    /// <param name="to">The end date of the range.</param>
    /// <returns>The list of file paths that match the date range.</returns>
    private static IEnumerable<string> GetFilesInRange(DateTime from, DateTime to)
        => Directory
            .GetFiles(Config.LogDirectoryPath, StswLogArchiving.LogFilePattern)
            .Where(path =>
                StswLogArchiving.TryGetLogDate(path, out var fileDate) &&
                fileDate.Date >= from.Date && fileDate.Date <= to.Date);

    /// <summary>
    /// Determines whether a line in the log file is the start of a new log entry.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="date">The parsed date from the line if it is a new log entry.</param>
    /// <returns><see langword="true"/> if the line is a new log entry; otherwise, <see langword="false"/>.</returns>
    private static bool IsNewLogEntryLine(string line, out DateTime date)
    {
        date = default;
        return line.Length >= 19 && DateTime.TryParse(line.Substring(0, 19), out date);
    }

    /// <summary>
    /// Parses a single line from the log file and converts it into a <see cref="StswLogItem"/> object.
    /// </summary>
    /// <param name="logEntryLines">The lines that make up a single log entry, where the first line contains the timestamp and log type, and subsequent lines contain the log text.</param>
    /// <returns>
    /// A <see cref="StswLogItem"/> object if the line is valid; otherwise, <see langword="null"/> if the line is invalid or cannot be parsed.
    /// </returns>
    /// <remarks>
    /// This method assumes that the log line is formatted as "yyyy-MM-dd HH:mm:ss | T | Log text", where 'T' represents the first character of the log type.
    /// </remarks>
    private static StswLogItem? ParseLogEntry(List<string> logEntryLines)
    {
        if (logEntryLines.Count == 0 || !IsNewLogEntryLine(logEntryLines[0], out var date))
            return null;

        var typeChar = logEntryLines[0].Length > 22 ? logEntryLines[0][22] : (char?)null;
        var type = Enum.GetValues(typeof(StswInfoType))
                       .Cast<StswInfoType>()
                       .FirstOrDefault(t =>
                            t.ToString().StartsWith(typeChar?.ToString() ?? "",
                                                    StringComparison.OrdinalIgnoreCase));

        var textList = new List<string>
        {
            logEntryLines[0].Length > 26 ? logEntryLines[0].Substring(26) : string.Empty
        };
        textList.AddRange(logEntryLines.Skip(1));

        var text = string.Join(Environment.NewLine, textList);
        return new StswLogItem(type, text, date);
    }

    /// <summary>
    /// Tries to add a parsed log block to the output list.
    /// </summary>
    /// <param name="currentBlock">The current block of log lines to parse.</param>
    /// <param name="output">The output list to which the parsed log item will be added.</param>
    private static void TryAddLogBlock(List<string> currentBlock, List<StswLogItem> output)
    {
        if (currentBlock.Count == 0)
            return;

        var parsed = ParseLogEntry(currentBlock);
        if (parsed.HasValue)
            output.Add(parsed.Value);
    }
    #endregion

    #region Write
    /// <summary>
    /// Writes a log entry synchronously using configured log targets, or the specified target override.
    /// </summary>
    /// <param name="type">The type of the log entry.</param>
    /// <param name="text">The text to log.</param>
    /// <param name="targets">Optional target override. If not specified, <see cref="StswLogConfig.Targets"/> is used.</param>
    public static void Write(StswInfoType? type, string text, StswLogTarget? targets = null)
    {
        if (!CanLog(type, out var resolvedTargets, targets))
            return;

        _ = WriteInternal(type, text, resolvedTargets);
    }

    /// <summary>
    /// Writes a log entry synchronously without specifying a log type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    /// <param name="targets">Optional target override. If not specified, <see cref="StswLogConfig.Targets"/> is used.</param>
    public static void Write(string text, StswLogTarget? targets = null) => Write(null, text, targets);

    /// <summary>
    /// Writes a log entry asynchronously using configured log targets, or the specified target override.
    /// </summary>
    /// <param name="type">The type of the log entry.</param>
    /// <param name="text">The text to log.</param>
    /// <param name="targets">Optional target override. If not specified, <see cref="StswLogConfig.Targets"/> is used.</param>
    public static Task WriteAsync(StswInfoType? type, string text, StswLogTarget? targets = null)
    {
        if (!CanLog(type, out var resolvedTargets, targets))
            return Task.CompletedTask;

        return WriteInternal(type, text, resolvedTargets);
    }

    /// <summary>
    /// Writes a log entry asynchronously without specifying a log type.
    /// </summary>
    /// <param name="text">The text to log.</param>
    /// <param name="targets">Optional target override. If not specified, <see cref="StswLogConfig.Targets"/> is used.</param>
    public static Task WriteAsync(string text, StswLogTarget? targets = null) => WriteAsync(null, text, targets);

    /// <summary>
    /// Writes an exception to enabled log targets, including its type, message, stack trace, and any inner exceptions.
    /// </summary>
    /// <param name="ex">Exception to log.</param>
    /// <param name="type">Optional type of the log entry. If not specified, defaults to <see cref="StswInfoType.Error"/>.</param>
    /// <param name="context">Optional context for the log entry, which can provide additional information about where the exception occurred.</param>
    /// <param name="targets">Optional target override. If not specified, <see cref="StswLogConfig.Targets"/> is used.</param>
    public static void WriteException(Exception ex, StswInfoType? type = StswInfoType.Error, string? context = null, StswLogTarget? targets = null)
    {
        if (!CanLog(type, out _, targets))
            return;

        try
        {
            var msg = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(context))
                msg.AppendLine($"[{context}]");

            void AppendException(Exception e, int level)
            {
                var prefix = new string('>', level);
                msg.AppendLine($"{prefix} {e.GetType().Name}: {e.Message}");
                msg.AppendLine($"{prefix} {e.StackTrace}");
                if (e.InnerException != null)
                    AppendException(e.InnerException, level + 1);
            }

            AppendException(ex, 0);
            Write(type, msg.ToString(), targets);
            _failureCount = 0;
        }
        catch (Exception ex2)
        {
            HandleLoggingFailure(ex2);
        }
    }

    /// <summary>
    /// Writes a log entry asynchronously, including the type of the log entry and the text to log.
    /// </summary>
    /// <param name="type">The type of the log entry.</param>
    /// <param name="text">The text to log.</param>
    /// <param name="targets">The output targets to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task WriteInternal(StswInfoType? type, string text, StswLogTarget targets)
    {
        var logItem = new StswLogItem(type, text);
        var logLine = FormatLogLine(logItem);
        var hasSuccess = false;
        var hasFailure = false;

        if (targets.HasFlag(StswLogTarget.File))
            hasSuccess |= await TryWriteToTargetAsync(() => WriteToFileAsync(logLine), () => hasFailure = true);

        if (targets.HasFlag(StswLogTarget.EventViewer))
            hasSuccess |= TryWriteToTarget(() => WriteToEventViewer(logItem, logLine), () => hasFailure = true);

        if (targets.HasFlag(StswLogTarget.MessageBox))
            hasSuccess |= TryWriteToTarget(() => ShowMessageBox(logItem), () => hasFailure = true);

        if (targets.HasFlag(StswLogTarget.Custom))
            hasSuccess |= TryWriteToTarget(() => WriteToCustomTargets(logItem), () => hasFailure = true);

        if (hasSuccess)
            CountLogToActiveCounters(type);

        if (!hasFailure)
            _failureCount = 0;
    }

    /// <summary>
    /// Writes a log entry to a file synchronously, including caller information such as member name, file path, and line number.
    /// </summary>
    /// <param name="type">Type of the log entry.</param>
    /// <param name="text">Text to log.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void WriteWithCaller(StswInfoType? type, string text, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => WriteWithCaller(type, text, null, memberName, filePath, lineNumber);

    /// <summary>
    /// Writes a log entry to a file synchronously, including caller information such as member name, file path, and line number.
    /// </summary>
    /// <param name="type">Type of the log entry.</param>
    /// <param name="text">Text to log.</param>
    /// <param name="targets">Optional target override. If not specified, <see cref="StswLogConfig.Targets"/> is used.</param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void WriteWithCaller(StswInfoType? type, string text, StswLogTarget? targets, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileName(filePath);
        text = $"[{fileName}:{lineNumber} {memberName}] {text}";
        Write(type, text, targets);
    }

    /// <summary>
    /// Determines whether the log entry can be written based on the type of the log, the current configuration, and selected targets.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <param name="resolvedTargets">The resolved output targets.</param>
    /// <param name="targetOverride">Optional target override.</param>
    /// <returns><see langword="true"/> if the log entry should be written; otherwise, <see langword="false"/>.</returns>
    private static bool CanLog(StswInfoType? type, out StswLogTarget resolvedTargets, StswLogTarget? targetOverride = null)
    {
        resolvedTargets = targetOverride ?? Config.Targets;

        if (Config.IsLoggingDisabled)
            return false;

        if (resolvedTargets == StswLogTarget.None)
            return false;

        if (!ShouldLog(type))
            return false;

        return true;
    }

    /// <summary>
    /// Formats a log item into the default log line format.
    /// </summary>
    /// <param name="item">The log item to format.</param>
    /// <returns>The formatted log line.</returns>
    private static string FormatLogLine(StswLogItem item)
        => $"{item.DateTime:yyyy-MM-dd HH:mm:ss} | {(item.Type ?? StswInfoType.None).ToString()[0]} | {item.Text}";

    /// <summary>
    /// Writes a log line to the daily file.
    /// </summary>
    /// <param name="logLine">The formatted log line.</param>
    private static async Task WriteToFileAsync(string logLine)
    {
        await _logSemaphore.WaitAsync();
        try
        {
            StswLogArchiving.ForceSizeArchiveIfNeeded();
            EnsureLogDirectoryExists();

            Console.WriteLine(logLine);
            using var sw = new StreamWriter(GetDailyLogFilePath(), true);
            await sw.WriteLineAsync(logLine);
        }
        finally
        {
            _logSemaphore.Release();
        }
    }

    /// <summary>
    /// Writes a log entry to Windows Event Viewer using reflection to avoid a hard dependency on System.Diagnostics.EventLog.
    /// </summary>
    /// <param name="item">The log item to write.</param>
    /// <param name="logLine">The formatted log line.</param>
    private static void WriteToEventViewer(StswLogItem item, string logLine)
    {
        var eventLogType = GetTypeFromLoadedOrReferencedAssemblies("System.Diagnostics.EventLog", "System.Diagnostics.EventLog");
        var eventLogEntryType = GetTypeFromLoadedOrReferencedAssemblies("System.Diagnostics.EventLogEntryType", "System.Diagnostics.EventLog");

        if (eventLogType == null || eventLogEntryType == null)
            throw new InvalidOperationException("System.Diagnostics.EventLog is not available. Add the System.Diagnostics.EventLog package/reference or disable the EventViewer log target.");

        var sourceName = string.IsNullOrWhiteSpace(Config.EventViewerSourceName)
            ? AppDomain.CurrentDomain.FriendlyName
            : Config.EventViewerSourceName;
        var logName = string.IsNullOrWhiteSpace(Config.EventViewerLogName)
            ? "Application"
            : Config.EventViewerLogName;

        var sourceExists = eventLogType.GetMethod("SourceExists", [typeof(string)]);
        var createEventSource = eventLogType.GetMethod("CreateEventSource", [typeof(string), typeof(string)]);
        var writeEntry = eventLogType.GetMethod("WriteEntry", [typeof(string), typeof(string), eventLogEntryType]);

        if (sourceExists == null || createEventSource == null || writeEntry == null)
            throw new MissingMethodException(eventLogType.FullName, "SourceExists/CreateEventSource/WriteEntry");

        if (sourceExists.Invoke(null, [sourceName]) is false)
            createEventSource.Invoke(null, [sourceName, logName]);

        var eventLogEntry = Enum.Parse(eventLogEntryType, GetEventViewerEntryTypeName(item.Type));
        writeEntry.Invoke(null, [sourceName, logLine, eventLogEntry]);
    }

    /// <summary>
    /// Shows a log entry through StswExpress.Wpf message dialog/message box if the type is available at runtime.
    /// </summary>
    /// <param name="item">The log item to show.</param>
    private static void ShowMessageBox(StswLogItem item)
    {
        var messageBoxType = GetTypeFromLoadedOrReferencedAssemblies("StswExpress.Wpf.StswMessageBox", "StswExpress.Wpf")
                          ?? GetTypeFromLoadedOrReferencedAssemblies("StswExpress.Wpf.StswMessageDialog", "StswExpress.Wpf");

        if (messageBoxType == null)
            throw new InvalidOperationException("StswExpress.Wpf.StswMessageBox/StswMessageDialog is not available. Reference StswExpress.Wpf or disable the MessageBox log target.");

        var showMethod = messageBoxType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(IsSupportedMessageBoxShowMethod)
            .OrderByDescending(x => x.GetParameters()[0].ParameterType == typeof(string))
            .ThenByDescending(x => x.GetParameters().Length)
            .FirstOrDefault();

        if (showMethod == null)
            throw new MissingMethodException(messageBoxType.FullName, "Show");

        var parameters = showMethod.GetParameters();
        var args = new object?[parameters.Length];
        args[0] = item.Text ?? string.Empty;

        for (var i = 1; i < parameters.Length; i++)
            args[i] = CreateMessageBoxArgument(parameters[i], item);

        var result = showMethod.Invoke(null, args);
        ObserveFaultedTask(result);
    }

    /// <summary>
    /// Writes a log entry to custom delegates and events.
    /// </summary>
    /// <param name="item">The log item to write.</param>
    private static void WriteToCustomTargets(StswLogItem item)
    {
        Config.CustomLogger?.Invoke(item);
        LogWritten?.Invoke(item);
    }

    /// <summary>
    /// Executes a logging target and handles target-specific errors.
    /// </summary>
    /// <param name="action">The target action.</param>
    /// <param name="onFailure">The failure callback.</param>
    /// <returns><see langword="true"/> if the target completed successfully; otherwise, <see langword="false"/>.</returns>
    private static bool TryWriteToTarget(Action action, Action onFailure)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            onFailure();
            HandleLoggingFailure(GetInnermostException(ex));
            return false;
        }
    }

    /// <summary>
    /// Executes an asynchronous logging target and handles target-specific errors.
    /// </summary>
    /// <param name="action">The target action.</param>
    /// <param name="onFailure">The failure callback.</param>
    /// <returns><see langword="true"/> if the target completed successfully; otherwise, <see langword="false"/>.</returns>
    private static async Task<bool> TryWriteToTargetAsync(Func<Task> action, Action onFailure)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            onFailure();
            HandleLoggingFailure(GetInnermostException(ex));
            return false;
        }
    }

    /// <summary>
    /// Checks whether a reflected Show method can be used for message box logging.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns><see langword="true"/> if the method is supported; otherwise, <see langword="false"/>.</returns>
    private static bool IsSupportedMessageBoxShowMethod(MethodInfo method)
    {
        if (method.Name != "Show")
            return false;

        var parameters = method.GetParameters();
        if (parameters.Length == 0)
            return false;

        return parameters[0].ParameterType == typeof(string) || parameters[0].ParameterType == typeof(object);
    }

    /// <summary>
    /// Creates an argument for a reflected message box Show method.
    /// </summary>
    /// <param name="parameter">The target parameter.</param>
    /// <param name="item">The log item.</param>
    /// <returns>The argument value.</returns>
    private static object? CreateMessageBoxArgument(ParameterInfo parameter, StswLogItem item)
    {
        if (parameter.Name?.Equals("title", StringComparison.OrdinalIgnoreCase) == true)
            return GetMessageBoxTitle(item.Type);

        if (parameter.Name?.Equals("details", StringComparison.OrdinalIgnoreCase) == true)
            return null;

        if (parameter.Name?.Equals("image", StringComparison.OrdinalIgnoreCase) == true && parameter.ParameterType.IsEnum)
            return TryCreateEnumValue(parameter.ParameterType, (item.Type ?? StswInfoType.None).ToString()) ?? GetDefaultParameterValue(parameter);

        return GetDefaultParameterValue(parameter);
    }

    /// <summary>
    /// Gets a default argument value for a reflected method parameter.
    /// </summary>
    /// <param name="parameter">The reflected parameter.</param>
    /// <returns>The default argument value.</returns>
    private static object? GetDefaultParameterValue(ParameterInfo parameter)
    {
        if (parameter.HasDefaultValue && parameter.DefaultValue != DBNull.Value)
        {
            if (parameter.ParameterType.IsEnum && parameter.DefaultValue is not null)
            {
                if (parameter.DefaultValue.GetType() == parameter.ParameterType)
                    return parameter.DefaultValue;

                return Enum.ToObject(parameter.ParameterType, parameter.DefaultValue);
            }

            return parameter.DefaultValue;
        }

        var underlyingNullableType = Nullable.GetUnderlyingType(parameter.ParameterType);
        if (!parameter.ParameterType.IsValueType || underlyingNullableType != null)
            return null;

        return Activator.CreateInstance(parameter.ParameterType);
    }

    /// <summary>
    /// Tries to create an enum value from a string.
    /// </summary>
    /// <param name="enumType">The enum type.</param>
    /// <param name="valueName">The enum value name.</param>
    /// <returns>The enum value if successful; otherwise, <see langword="null"/>.</returns>
    private static object? TryCreateEnumValue(Type enumType, string valueName)
    {
        if (Enum.GetNames(enumType).Any(x => x.Equals(valueName, StringComparison.OrdinalIgnoreCase)))
            return Enum.Parse(enumType, valueName, true);

        return null;
    }

    /// <summary>
    /// Observes a reflected Task result to prevent unobserved exceptions.
    /// </summary>
    /// <param name="result">The reflected method result.</param>
    private static void ObserveFaultedTask(object? result)
    {
        if (result is not Task task)
            return;

        _ = task.ContinueWith(x =>
        {
            if (x.Exception != null)
                HandleLoggingFailure(GetInnermostException(x.Exception));
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Gets a runtime type from loaded assemblies or attempts to load a known assembly.
    /// </summary>
    /// <param name="typeName">The full type name.</param>
    /// <param name="assemblyName">The optional assembly name to load.</param>
    /// <returns>The type if found; otherwise, <see langword="null"/>.</returns>
    private static Type? GetTypeFromLoadedOrReferencedAssemblies(string typeName, string? assemblyName = null)
    {
        var type = Type.GetType(string.IsNullOrWhiteSpace(assemblyName) ? typeName : $"{typeName}, {assemblyName}", false);
        if (type != null)
            return type;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName, false);
            if (type != null)
                return type;
        }

        if (string.IsNullOrWhiteSpace(assemblyName))
            return null;

        try
        {
            var assembly = Assembly.Load(assemblyName);
            return assembly.GetType(typeName, false);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the Windows Event Viewer entry type name for the specified log type.
    /// </summary>
    /// <param name="type">The log type.</param>
    /// <returns>The Event Viewer entry type name.</returns>
    private static string GetEventViewerEntryTypeName(StswInfoType? type) => type switch
    {
        StswInfoType.Error or StswInfoType.Fatal => "Error",
        StswInfoType.Warning => "Warning",
        _ => "Information",
    };

    /// <summary>
    /// Gets the message box title for the specified log type.
    /// </summary>
    /// <param name="type">The log type.</param>
    /// <returns>The message box title.</returns>
    private static string GetMessageBoxTitle(StswInfoType? type) => type?.ToString() ?? nameof(StswInfoType.None);

    /// <summary>
    /// Gets the innermost exception from reflection and aggregate wrappers.
    /// </summary>
    /// <param name="ex">The exception to unwrap.</param>
    /// <returns>The innermost exception.</returns>
    private static Exception GetInnermostException(Exception ex)
    {
        if (ex is TargetInvocationException { InnerException: not null } targetInvocationException)
            return GetInnermostException(targetInvocationException.InnerException);

        if (ex is AggregateException aggregateException)
            return GetInnermostException(aggregateException.GetBaseException());

        return ex;
    }

    /// <summary>
    /// Increments the active counters for the specified log type.
    /// </summary>
    /// <param name="type">The type of log entry to increment the counter for.</param>
    private static void CountLogToActiveCounters(StswInfoType? type)
    {
        if (type == null)
            return;

        lock (_logCounters)
        {
            foreach (var counter in _logCounters.Values)
            {
                if (counter.TryGetValue(type.Value, out var value))
                    counter[type.Value] = ++value;
                else
                    counter[type.Value] = 1;
            }
        }
    }

    /// <summary>
    /// Handles logging failures by incrementing the failure count and disabling logging if the maximum number of failures is reached.
    /// Also invokes a custom failure action if one is configured.
    /// </summary>
    /// <param name="ex">The exception that occurred during the logging process.</param>
    internal static void HandleLoggingFailure(Exception ex)
    {
        _failureCount++;
        if (Config.MaxFailures.HasValue && _failureCount >= Config.MaxFailures.Value)
            Config.IsLoggingDisabled = true;

        Config.OnLogFailure?.Invoke(ex);
    }

    /// <summary>
    /// Determines whether the log entry should be written based on the type of the log and the current configuration.
    /// </summary>
    /// <param name="type">The type of log entry.</param>
    /// <returns><see langword="true"/> if the log entry should be written; otherwise, <see langword="false"/>.</returns>
    private static bool ShouldLog(StswInfoType? type)
    {
        if (type == null)
            return true;

        return StswFn.IsInDebugMode
            ? type.Value.In(Config.LogTypes_DEBUG)
            : type.Value.In(Config.LogTypes_RELEASE);
    }
    #endregion

    /// <summary>
    /// Ensures the active log directory exists.
    /// </summary>
    internal static void EnsureLogDirectoryExists()
    {
        if (!string.IsNullOrEmpty(Config.LogDirectoryPath) && !Directory.Exists(Config.LogDirectoryPath))
            Directory.CreateDirectory(Config.LogDirectoryPath);
    }

    /// <summary>
    /// Gets the path to the log file for the current day.
    /// </summary>
    /// <returns>The full path to the log file for today.</returns>
    internal static string GetDailyLogFilePath() => Path.Combine(Config.LogDirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log");
}
