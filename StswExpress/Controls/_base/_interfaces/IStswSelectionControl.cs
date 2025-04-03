using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Defines a contract for selection controls, providing properties and methods
/// for managing item selection and presentation in custom selection controls.
/// </summary>
public interface IStswSelectionControl
{
    /// <summary>
    /// Gets or sets the path to a value on the source object to serve as the visual representation of the object.
    /// </summary>
    string DisplayMemberPath { get; set; }
    static readonly DependencyProperty? DisplayMemberPathProperty;

    /// <summary>
    /// Gets or sets a value indicating whether control is in read-only mode.
    /// When set to <see langword="true"/>, the scroll with items is accessible, but all items within the scroll are unclickable.
    /// </summary>
    bool IsReadOnly { get; set; }
    static readonly DependencyProperty? IsReadOnlyProperty;

    /// <summary>
    /// Gets or sets the collection used to generate the content of the control.
    /// </summary>
    IEnumerable ItemsSource { get; set; }
    static readonly DependencyProperty? ItemsSourceProperty;

    /// <summary>
    /// Gets or sets the template used to display each item in the control.
    /// </summary>
    DataTemplate ItemTemplate { get; set; }
    static readonly DependencyProperty? ItemTemplateProperty;

    /// <summary>
    /// Gets or sets the path to a value on the source object that should be used to identify the selected item.
    /// </summary>
    string SelectedValuePath { get; set; }
    static readonly DependencyProperty? SelectedValuePathProperty;

    /// <summary>
    /// Handles selection changes by triggering animations on selected and unselected items.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="addedItems">The newly selected items.</param>
    /// <param name="removedItems">The deselected items.</param>
    static void SelectionChanged(ItemsControl selectionControl, IList? addedItems, IList? removedItems)
    {
        if (StswSettings.Default.EnableAnimations && StswControl.GetEnableAnimations(selectionControl))
        {
            if (addedItems != null)
                foreach (var selectedItem in addedItems)
                    if (selectionControl.ItemContainerGenerator.ContainerFromItem(selectedItem) is Control item && item.Template.FindName("OPT_MainBorder", item) is Border border)
                        StswSharedAnimations.AnimateClick(selectionControl, border, true);

            if (removedItems != null)
                foreach (var unselectedItem in removedItems)
                    if (selectionControl.ItemContainerGenerator.ContainerFromItem(unselectedItem) is Control item && item.Template.FindName("OPT_MainBorder", item) is Border border)
                        StswSharedAnimations.AnimateClick(selectionControl, border, false);
        }
    }

    /// <summary>
    /// Handles changes to the <see cref="ItemsSource"/> property, adjusting properties like <see cref="DisplayMemberPath"/> and <see cref="SelectedValuePath"/>.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="newValue">The new ItemsSource value.</param>
    static void ItemsSourceChanged(IStswSelectionControl selectionControl, IEnumerable? newValue)
    {
        IEnumerable? actualSource = newValue;

        /// check if newValue is a CollectionView and get the SourceCollection
        if (newValue is ICollectionView collectionView)
            actualSource = collectionView.SourceCollection;

        /// continue with the logic using actualSource
        if (actualSource?.GetType()?.IsListType(out var innerType) == true)
        {
            /// StswComboItem short usage
            if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
            {
                if (string.IsNullOrEmpty(selectionControl.DisplayMemberPath) && selectionControl.ItemTemplate == null)
                    selectionControl.DisplayMemberPath = nameof(StswComboItem.Display);
                if (string.IsNullOrEmpty(selectionControl.SelectedValuePath))
                    selectionControl.SelectedValuePath = nameof(StswComboItem.Value);
            }
        }
    }

    /// <summary>
    /// Handles changes to the <see cref="ItemTemplate"/> property, ensuring <see cref="DisplayMemberPath"/> is cleared when a template is used.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="itemTemplate">The new ItemTemplate value.</param>
    static void ItemTemplateChanged(IStswSelectionControl selectionControl, DataTemplate itemTemplate)
    {
        if (itemTemplate != null && !string.IsNullOrEmpty(selectionControl.DisplayMemberPath))
            selectionControl.DisplayMemberPath = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selectionControl"></param>
    /// <param name="e"></param>
    static bool PreviewKeyDown(IStswSelectionControl selectionControl, KeyEventArgs e)
    {
        if (selectionControl.IsReadOnly)
        {
            if (!e.Key.In(Key.Space, Key.Tab))
            {
                e.Handled = true;
                return false;
            }
        }
        return true;
    }
}
