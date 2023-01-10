using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswColumnFilterBase : UserControl
{
    /// DisplayMemberPath
    public static readonly DependencyProperty DisplayMemberPathProperty
        = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(string))
        );
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// FilterPadding
    public static readonly DependencyProperty FilterPaddingProperty
        = DependencyProperty.Register(
            nameof(FilterPadding),
            typeof(Thickness),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness FilterPadding
    {
        get => (Thickness)GetValue(FilterPaddingProperty);
        set => SetValue(FilterPaddingProperty, value);
    }

    /// FilterType
    public static readonly DependencyProperty FilterTypeProperty
        = DependencyProperty.Register(
            nameof(FilterType),
            typeof(Types),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(Types.Text)
        );
    public Types FilterType
    {
        get => (Types)GetValue(FilterTypeProperty);
        set => SetValue(FilterTypeProperty, value);
    }
    public enum Types
    {
        Check,
        Date,
        Number,
        Text,
        ListOfNumbers,
        ListOfTexts
    }

    /// FilterVisibility
    public static readonly DependencyProperty FilterVisibilityProperty
        = DependencyProperty.Register(
            nameof(FilterVisibility),
            typeof(Visibility),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility FilterVisibility
    {
        get => (Visibility)GetValue(FilterVisibilityProperty);
        set => SetValue(FilterVisibilityProperty, value);
    }

    /// Header
    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(object?))
        );
    public object? Header
    {
        get => (object?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// IsFilterCaseSensitive
    public static readonly DependencyProperty IsFilterCaseSensitiveProperty
        = DependencyProperty.Register(
            nameof(IsFilterCaseSensitive),
            typeof(bool),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(bool))
        );
    public bool IsFilterCaseSensitive
    {
        get => (bool)GetValue(IsFilterCaseSensitiveProperty);
        set => SetValue(IsFilterCaseSensitiveProperty, value);
    }

    /// IsFilterNullSensitive
    public static readonly DependencyProperty IsFilterNullSensitiveProperty
        = DependencyProperty.Register(
            nameof(IsFilterNullSensitive),
            typeof(bool),
            typeof(StswColumnFilterBase),
            new PropertyMetadata(default(bool))
        );
    public bool IsFilterNullSensitive
    {
        get => (bool)GetValue(IsFilterNullSensitiveProperty);
        set => SetValue(IsFilterNullSensitiveProperty, value);
    }

    /// ItemsSource
    public static readonly DependencyProperty ItemsSourceProperty
        = DependencyProperty.Register(
              nameof(ItemsSource),
              typeof(IList),
              typeof(StswColumnFilterBase),
              new PropertyMetadata(default(IList))
          );
    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// SelectedValuePath
    public static readonly DependencyProperty SelectedValuePathProperty
        = DependencyProperty.Register(
              nameof(SelectedValuePath),
              typeof(string),
              typeof(StswColumnFilterBase),
              new PropertyMetadata(default(string))
          );
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }
}
