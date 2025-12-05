using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace StswExpress.Wpf;

/// <summary>
/// A validation rule that checks if a specified path exists based on the selection unit of the associated <see cref="StswPathPicker"/> control.
/// </summary>
internal class StswPathExistsValidationRule : ValidationRule
{
    /// <summary>
    /// Gets or sets the associated <see cref="StswPathPicker"/> control.
    /// </summary>
    public StswPathPicker? Host { get; set; }

    /// <summary>
    /// Validates whether the provided path exists based on the selection unit of the associated <see cref="StswPathPicker"/> control.
    /// </summary>
    /// <param name="value">The value to be validated, expected to be a string representing a file or directory path.</param>
    /// <param name="cultureInfo">The culture information to use during validation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating whether the path is valid or not.</returns>
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (Host is null)
            return ValidationResult.ValidResult;

        var text = value as string;
        if (string.IsNullOrWhiteSpace(text))
            return ValidationResult.ValidResult;

        try
        {
            var raw = Environment.ExpandEnvironmentVariables(text).Trim().Trim('"');
            var normalized = Path.GetFullPath(raw);

            switch (Host.SelectionUnit)
            {
                case StswPathType.OpenDirectory:
                    return Directory.Exists(normalized)
                        ? ValidationResult.ValidResult
                        : new ValidationResult(false, "Directory does not exist.");

                case StswPathType.OpenFile:
                    if (!File.Exists(normalized))
                        return new ValidationResult(false, "File does not exist.");

                    if (!StswPathFilterHelper.MatchesFilter(Path.GetFileName(normalized), Host.Filter))
                        return new ValidationResult(false, "File does not match the specified filter.");

                    return ValidationResult.ValidResult;

                case StswPathType.SaveFile:
                    var dir = Path.GetDirectoryName(normalized);
                    var file = Path.GetFileName(normalized);

                    if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                        return new ValidationResult(false, "Target directory does not exist.");

                    if (string.IsNullOrWhiteSpace(file) ||
                        file.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                        return new ValidationResult(false, "Invalid file name.");

                    return ValidationResult.ValidResult;

                default:
                    return new ValidationResult(false, "Unsupported selection unit.");
            }
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, ex.Message);
        }
    }
}
