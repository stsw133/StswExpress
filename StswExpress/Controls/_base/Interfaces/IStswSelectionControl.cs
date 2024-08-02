using System.Collections;
using System.ComponentModel;
using System.Windows;

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
    /// Gets or sets a value indicating whether the control uses selection items that implement
    /// the <see cref="IStswSelectionItem"/> interface to enable advanced selection features.
    /// This allows for enhanced functionality when items in the collection implement a specific interface.
    /// </summary>
    bool UsesSelectionItems { get; set; }
    static readonly DependencyProperty? UsesSelectionItemsProperty;

    /// <summary>
    /// Handles changes to the ItemsSource property of the selection control.
    /// This method determines the underlying data source of the control, adjusting properties
    /// such as <see cref="UsesSelectionItems"/>, <see cref="DisplayMemberPath"/>, and <see cref="SelectedValuePath"/>
    /// based on the type of items in the source collection.
    /// </summary>
    /// <param name="selectionControl">The selection control whose ItemsSource has changed.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    static void ItemsSourceChanged(IStswSelectionControl selectionControl, IEnumerable newValue)
    {
        IEnumerable actualSource = newValue;

        /// check if newValue is a CollectionView and get the SourceCollection
        if (newValue is ICollectionView collectionView)
            actualSource = collectionView.SourceCollection;

        /// continue with the logic using actualSource
        if (actualSource?.GetType()?.IsListType(out var innerType) == true)
        {
            selectionControl.UsesSelectionItems = innerType?.IsAssignableTo(typeof(IStswSelectionItem)) == true;

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
