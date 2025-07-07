using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;
/// <summary>
/// Provides a base implementation of the <see cref="INotifyPropertyChanged"/> interface to enable objects to send notifications to clients when the value of a property changes.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// public class MainViewModel : StswObservableValidator
/// {
///     [Required(ErrorMessage = "Username is required.")]
///     [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
///     public string? Username
///     {
///         get => _username;
///         set => SetProperty(ref _username, value);
///     }
///     private string? _username;
///     
///     [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
///     public int Age
///     {
///         get => _age;
///         set => SetValidatedProperty(ref _age, value);
///     }
///     private int _age;
/// }
/// </code>
/// </example>
[Stsw("0.19.0", Changes = StswPlannedChanges.None, IsTested = false)]
public abstract class StswObservableValidator : StswObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = [];

    /// <inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc/>
    public bool HasErrors => _errors.Any();

    /// <inheritdoc/>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName is null)
            return _errors.SelectMany(e => e.Value);
        else if (_errors.TryGetValue(propertyName, out var propertyErrors))
            return propertyErrors;
        else
            return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Validates all properties of the object using data annotations.
    /// </summary>
    public void ValidateAllProperties()
    {
        var context = new ValidationContext(this);
        var results = new List<ValidationResult>();

        _errors.Clear();
        Validator.TryValidateObject(this, context, results, validateAllProperties: true);

        foreach (var result in results)
            foreach (var propertyName in result.MemberNames)
                AddError(propertyName, result.ErrorMessage!);

        foreach (var propertyName in _errors.Keys)
            OnErrorsChanged(propertyName);

        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Validates a specific property of the object using data annotations.
    /// </summary>
    /// <param name="value">The value of the property to validate.</param>
    /// <param name="propertyName">The name of the property to validate. This is optional and can be automatically provided by the compiler.</param>
    protected virtual void ValidateProperty(object? value, [CallerMemberName] string propertyName = "")
    {
        var context = new ValidationContext(this)
        {
            MemberName = propertyName
        };

        var results = new List<ValidationResult>();

        if (!string.IsNullOrWhiteSpace(propertyName))
            _errors.Remove(propertyName);

        if (!Validator.TryValidateProperty(value, context, results))
        {
            foreach (var result in results)
                AddError(propertyName, result.ErrorMessage!);
        }

        OnErrorsChanged(propertyName);
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Adds an error message for the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that has an error.</param>
    /// <param name="error">The error message to add.</param>
    private void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var propertyErrors))
        {
            propertyErrors = [];
            _errors[propertyName] = propertyErrors;
        }

        if (!propertyErrors.Contains(error))
            propertyErrors.Add(error);
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event for the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    private void OnErrorsChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

    /// <summary>
    /// Sets the property value and validates it using data annotations.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">Reference to the field storing the property's value.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The name of the property. This is optional and can be automatically provided by the compiler.</param>
    /// <returns><see langword="true"/> if the property value was changed and validated; otherwise, <see langword="false"/>.</returns>
    protected override bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        var result = base.SetProperty(ref field, value, propertyName);
        if (result)
            ValidateProperty(value, propertyName);

        return result;
    }
}
