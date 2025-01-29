using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Defines a contract for selection controls, providing properties and methods
/// for managing item selection and presentation in custom selection controls.
/// </summary>
public interface IStswSelectionControl
{
    /// <summary>
    /// Gets or sets the path to a value on the source object to serve as the visual representation of the object.
    /// This property is typically used to specify a member of an item in a collection to display in the control.
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
    /// This property allows binding the control to a collection of items that will be displayed.
    /// </summary>
    IEnumerable ItemsSource { get; set; }
    static readonly DependencyProperty? ItemsSourceProperty;

    /// <summary>
    /// Gets or sets the template used to display each item in the control.
    /// This property allows customization of how each item appears in the control by specifying a <see cref="DataTemplate"/>.
    /// </summary>
    DataTemplate ItemTemplate { get; set; }
    static readonly DependencyProperty? ItemTemplateProperty;

    /// <summary>
    /// Gets or sets the path to a value on the source object that should be used to identify the selected item.
    /// This is useful for scenarios where you want to bind the selected value of the control to a specific property.
    /// </summary>
    string SelectedValuePath { get; set; }
    static readonly DependencyProperty? SelectedValuePathProperty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selectionControl"></param>
    /// <param name="addedItems"></param>
    /// <param name="removedItems"></param>
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
    /// Handles changes to the ItemsSource property of the selection control.
    /// This method determines the underlying data source of the control, adjusting properties
    /// such as <see cref="DisplayMemberPath"/> and <see cref="SelectedValuePath"/>
    /// based on the type of items in the source collection.
    /// </summary>
    /// <param name="selectionControl">The selection control whose ItemsSource has changed.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
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
    /// Handles changes to the ItemTemplate property of the selection control.
    /// This method clears the <see cref="DisplayMemberPath"/> property if a new template is provided,
    /// ensuring that the control uses the DataTemplate to display items rather than a simple string path.
    /// </summary>
    /// <param name="selectionControl">The selection control whose ItemTemplate has changed.</param>
    /// <param name="itemTemplate">The new DataTemplate to be used for displaying items.</param>
    static void ItemTemplateChanged(IStswSelectionControl selectionControl, DataTemplate itemTemplate)
    {
        if (itemTemplate != null && !string.IsNullOrEmpty(selectionControl.DisplayMemberPath))
            selectionControl.DisplayMemberPath = string.Empty;
    }
}
