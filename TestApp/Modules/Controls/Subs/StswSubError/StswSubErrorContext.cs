using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswSubErrorContext : ControlsContext
{
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public override void SetDefaults()
    {
        base.SetDefaults();

        //HasError = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HasError)))?.Value ?? default;
        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
    }

    /// Errors
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => _errors;
        set => SetProperty(ref _errors, value);
    }
    private ReadOnlyObservableCollection<ValidationError> _errors = new ReadOnlyObservableCollection<ValidationError>(
    [
        new ValidationError(new ExceptionValidationRule(), "Error 1", "Error 1", null),
        new ValidationError(new ExceptionValidationRule(), "Error 2", "Error 2", null),
        new ValidationError(new ExceptionValidationRule(), "Error 3", "Error 3", null)
    ]);

    /// HasError
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }
    private bool _hasError = true;
    
    /// IconScale
    public GridLength IconScale
    {
        get => _iconScale;
        set => SetProperty(ref _iconScale, value);
    }
    private GridLength _iconScale;

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsContentVisible
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetProperty(ref _isContentVisible, value);
    }
    private bool _isContentVisible;
}
