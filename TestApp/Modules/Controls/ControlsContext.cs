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
        get => horizontalAlignment;
        set => SetProperty(ref horizontalAlignment, value);
    }
    private HorizontalAlignment horizontalAlignment;

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
        get => horizontalContentAlignment;
        set => SetProperty(ref horizontalContentAlignment, value);
    }
    private HorizontalAlignment horizontalContentAlignment;

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
        get => verticalAlignment;
        set => SetProperty(ref verticalAlignment, value);
    }
    private VerticalAlignment verticalAlignment;

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
        get => verticalContentAlignment;
        set => SetProperty(ref verticalContentAlignment, value);
    }
    private VerticalAlignment verticalContentAlignment;

    /// IsEnabled
    private bool isEnabled = true;
    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }
    #endregion
}
