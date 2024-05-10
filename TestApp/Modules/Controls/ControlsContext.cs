using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace TestApp;

public class ControlsContext : StswObservableObject
{
    public string ThisControlName => GetType().Name[..^7];
    public Style ThisControlStyle = new();
    public List<Setter> ThisControlSetters = new();

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
            ThisControlStyle = (Style)Application.Current.TryFindResource(Type.GetType($"StswExpress.{ThisControlName}, StswExpress"));
        }
        catch { }

        if (ThisControlStyle != null)
        {
            ThisControlSetters = ThisControlStyle.Setters.Select(x => (Setter)x).ToList();

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
    #endregion

    #region Properties
    /// HorizontalAlignment
    public ObservableCollection<StswComboItem> HorizontalAlignmentSelector { get; set; } = new()
    {
        new() { Display = StswIcons.AlignHorizontalLeft, Value = HorizontalAlignment.Left },
        new() { Display = StswIcons.AlignHorizontalCenter, Value = HorizontalAlignment.Center },
        new() { Display = StswIcons.AlignHorizontalRight, Value = HorizontalAlignment.Right },
        new() { Display = StswIcons.AlignHorizontalDistribute, Value = HorizontalAlignment.Stretch },
    };
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => SetProperty(ref _horizontalAlignment, value);
    }
    private HorizontalAlignment _horizontalAlignment;

    /// HorizontalContentAlignment
    public ObservableCollection<StswComboItem> HorizontalContentAlignmentSelector { get; set; } = new()
    {
        new() { Display = StswIcons.AlignHorizontalLeft, Value = HorizontalAlignment.Left },
        new() { Display = StswIcons.AlignHorizontalCenter, Value = HorizontalAlignment.Center },
        new() { Display = StswIcons.AlignHorizontalRight, Value = HorizontalAlignment.Right },
        new() { Display = StswIcons.AlignHorizontalDistribute, Value = HorizontalAlignment.Stretch },
    };
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => _horizontalContentAlignment;
        set => SetProperty(ref _horizontalContentAlignment, value);
    }
    private HorizontalAlignment _horizontalContentAlignment;

    /// VerticalAlignment
    public ObservableCollection<StswComboItem> VerticalAlignmentSelector { get; set; } = new()
    {
        new() { Display = StswIcons.AlignVerticalTop, Value = VerticalAlignment.Top },
        new() { Display = StswIcons.AlignVerticalCenter, Value = VerticalAlignment.Center },
        new() { Display = StswIcons.AlignVerticalBottom, Value = VerticalAlignment.Bottom },
        new() { Display = StswIcons.AlignVerticalDistribute, Value = VerticalAlignment.Stretch },
    };
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set => SetProperty(ref _verticalAlignment, value);
    }
    private VerticalAlignment _verticalAlignment;

    /// VerticalContentAlignment
    public ObservableCollection<StswComboItem> VerticalContentAlignmentSelector { get; set; } = new()
    {
        new() { Display = StswIcons.AlignVerticalTop, Value = VerticalAlignment.Top },
        new() { Display = StswIcons.AlignVerticalCenter, Value = VerticalAlignment.Center },
        new() { Display = StswIcons.AlignVerticalBottom, Value = VerticalAlignment.Bottom },
        new() { Display = StswIcons.AlignVerticalDistribute, Value = VerticalAlignment.Stretch },
    };
    public VerticalAlignment VerticalContentAlignment
    {
        get => _verticalContentAlignment;
        set => SetProperty(ref _verticalContentAlignment, value);
    }
    private VerticalAlignment _verticalContentAlignment;

    /// IsEnabled
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
    private bool _isEnabled = true;
    #endregion
}
