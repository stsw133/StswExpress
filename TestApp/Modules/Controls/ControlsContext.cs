using System;
using System.Linq;
using System.Windows;

namespace TestApp;

public class ControlsContext : StswObservableObject
{
    public string ThisControlName => GetType().Name[..^7];

    public ControlsContext()
    {
        SetDefaults();
    }

    #region Events & methods
    /// SetDefaults
    public void SetDefaults()
    {
        var style = (Style)Application.Current.TryFindResource(Type.GetType($"StswExpress.{ThisControlName}, StswExpress"));
        if (style != null)
        {
            var setters = style.Setters.Select(x => (Setter)x);

            if (setters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalAlignment))) is Setter horizontalAlignment)
                HorizontalAlignment = (HorizontalAlignment)horizontalAlignment.Value;

            if (setters.FirstOrDefault(x => x.Property.Name.Equals(nameof(HorizontalContentAlignment))) is Setter horizontalContentAlignment)
                HorizontalContentAlignment = (HorizontalAlignment)horizontalContentAlignment.Value;

            if (setters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalAlignment))) is Setter verticalAlignment)
                VerticalAlignment = (VerticalAlignment)verticalAlignment.Value;

            if (setters.FirstOrDefault(x => x.Property.Name.Equals(nameof(VerticalContentAlignment))) is Setter verticalContentAlignment)
                VerticalContentAlignment = (VerticalAlignment)verticalContentAlignment.Value;

            if (setters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsEnabled))) is Setter isEnabled)
                IsEnabled = (bool)isEnabled.Value;
        }
    }
    #endregion

    #region Properties
    /// Alignment
    private HorizontalAlignment horizontalAlignment;
    public HorizontalAlignment HorizontalAlignment
    {
        get => horizontalAlignment;
        set => SetProperty(ref horizontalAlignment, value);
    }
    private HorizontalAlignment horizontalContentAlignment;
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => horizontalContentAlignment;
        set => SetProperty(ref horizontalContentAlignment, value);
    }
    private VerticalAlignment verticalAlignment;
    public VerticalAlignment VerticalAlignment
    {
        get => verticalAlignment;
        set => SetProperty(ref verticalAlignment, value);
    }
    private VerticalAlignment verticalContentAlignment;
    public VerticalAlignment VerticalContentAlignment
    {
        get => verticalContentAlignment;
        set => SetProperty(ref verticalContentAlignment, value);
    }

    /// IsEnabled
    private bool isEnabled = true;
    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }
    #endregion
}
