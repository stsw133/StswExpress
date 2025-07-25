﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;
public partial class StswSubErrorContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //HasError = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HasError)))?.Value ?? default;
        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
    }

    [StswCommand] void SetGridLengthAuto() => IconScale = GridLength.Auto;
    [StswCommand] void SetGridLengthFill() => IconScale = new GridLength(1, GridUnitType.Star);

    [StswObservableProperty] ReadOnlyObservableCollection<ValidationError> _errors = new ReadOnlyObservableCollection<ValidationError>(
    [
        new ValidationError(new ExceptionValidationRule(), "Error 1", "Error 1", null),
        new ValidationError(new ExceptionValidationRule(), "Error 2", "Error 2", null),
        new ValidationError(new ExceptionValidationRule(), "Error 3", "Error 3", null)
    ]);
    [StswObservableProperty] bool _hasError = true;
    [StswObservableProperty] GridLength _iconScale;
    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isContentVisible;
}
