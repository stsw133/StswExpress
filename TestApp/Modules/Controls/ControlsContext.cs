using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace TestApp;

public partial class ControlsContext : StswObservableObject
{
    public string ThisControlName => GetType().Name[..^7];
    public IEnumerable<Setter> ThisControlSetters = [];

    public ControlsContext()
    {
        SetDefaults();
    }

    #region Events & methods
    /// SetDefaults
    public virtual void SetDefaults()
    {
        try
        {
            var style = (Style)Application.Current.TryFindResource(Type.GetType($"StswExpress.{ThisControlName}, StswExpress"));
            if (style != null)
                ThisControlSetters = [.. GetAllSetters(style).OfType<Setter>()];
        }
        catch { }

        if (ThisControlSetters != null)
        {
            if (ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalAlignment))) is Setter horizontalAlignment)
                HorizontalAlignment = (HorizontalAlignment)horizontalAlignment.Value;

            if (ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalContentAlignment))) is Setter horizontalContentAlignment)
                HorizontalContentAlignment = (HorizontalAlignment)horizontalContentAlignment.Value;

            if (ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalAlignment))) is Setter verticalAlignment)
                VerticalAlignment = (VerticalAlignment)verticalAlignment.Value;

            if (ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalContentAlignment))) is Setter verticalContentAlignment)
                VerticalContentAlignment = (VerticalAlignment)verticalContentAlignment.Value;
            
            if (ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsEnabled))) is Setter isEnabled)
                IsEnabled = (bool)isEnabled.Value;
        }
    }

    /// GetAllSetters
    private static List<SetterBase> GetAllSetters(Style style)
    {
        var setters = new List<SetterBase>();
        setters.AddRange(style.Setters);

        if (style.BasedOn != null)
            setters.AddRange(GetAllSetters(style.BasedOn));

        return setters;
    }
    #endregion

    public ObservableCollection<StswComboItem> HorizontalAlignmentSource { get; set; } =
    [
        new() { Display = StswIcons.AlignHorizontalLeft, Value = HorizontalAlignment.Left },
        new() { Display = StswIcons.AlignHorizontalCenter, Value = HorizontalAlignment.Center },
        new() { Display = StswIcons.AlignHorizontalRight, Value = HorizontalAlignment.Right },
        new() { Display = StswIcons.AlignHorizontalDistribute, Value = HorizontalAlignment.Stretch },
    ];
    public ObservableCollection<StswComboItem> VerticalAlignmentSource { get; set; } =
    [
        new() { Display = StswIcons.AlignVerticalTop, Value = VerticalAlignment.Top },
        new() { Display = StswIcons.AlignVerticalCenter, Value = VerticalAlignment.Center },
        new() { Display = StswIcons.AlignVerticalBottom, Value = VerticalAlignment.Bottom },
        new() { Display = StswIcons.AlignVerticalDistribute, Value = VerticalAlignment.Stretch },
    ];

    [StswObservableProperty] HorizontalAlignment _horizontalAlignment;
    [StswObservableProperty] HorizontalAlignment _horizontalContentAlignment;
    [StswObservableProperty] VerticalAlignment _verticalAlignment;
    [StswObservableProperty] VerticalAlignment _verticalContentAlignment;
    [StswObservableProperty] bool _isEnabled = true;
}
