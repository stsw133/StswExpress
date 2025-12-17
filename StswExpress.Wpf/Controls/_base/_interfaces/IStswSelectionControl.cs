using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress.Wpf;

/// <summary>
/// Defines a contract for selection controls, providing properties and methods
/// for managing item selection and presentation in custom selection controls.
/// </summary>
public interface IStswSelectionControl
{
    /// <summary>
    /// Gets or sets the path to a value on the source object to serve as the visual representation of the object.
    /// </summary>
    public string DisplayMemberPath { get; set; }
    public static readonly DependencyProperty? DisplayMemberPathProperty;

    /// <summary>
    /// Gets or sets a value indicating whether control is in read-only mode.
    /// When set to <see langword="true"/>, the scroll with items is accessible, but all items within the scroll are unclickable.
    /// </summary>
    public bool IsReadOnly { get; set; }
    public static readonly DependencyProperty? IsReadOnlyProperty;

    /// <summary>
    /// Gets or sets the collection used to generate the content of the control.
    /// </summary>
    public IEnumerable ItemsSource { get; set; }
    public static readonly DependencyProperty? ItemsSourceProperty;

    /// <summary>
    /// Gets or sets the template used to display each item in the control.
    /// </summary>
    public DataTemplate ItemTemplate { get; set; }
    public static readonly DependencyProperty? ItemTemplateProperty;

    /// <summary>
    /// Gets or sets the path to a value on the source object that should be used to identify the selected item.
    /// </summary>
    public string SelectedValuePath { get; set; }
    public static readonly DependencyProperty? SelectedValuePathProperty;

    /// <summary>
    /// Handles changes to the <see cref="ItemsSource"/> property, adjusting properties like <see cref="DisplayMemberPath"/> and <see cref="SelectedValuePath"/>.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="newValue">The new ItemsSource value.</param>
    public static void ItemsSourceChanged(IStswSelectionControl selectionControl, IEnumerable? newValue)
    {
        /// check if newValue is a CollectionView and get the SourceCollection
        IEnumerable? ResolveActualSource() => newValue is ICollectionView collectionView
            ? collectionView.SourceCollection
            : newValue;

        void ApplyDefaultPaths()
        {
            /// check if user has provided custom paths
            var hasDisplayMemberPath = !string.IsNullOrEmpty(selectionControl.DisplayMemberPath);
            var hasSelectedValuePath = !string.IsNullOrEmpty(selectionControl.SelectedValuePath);

            if (selectionControl is DependencyObject dependencyObject)
            {
                hasDisplayMemberPath |= HasUserProvidedValue(dependencyObject, ItemsControl.DisplayMemberPathProperty);
                hasSelectedValuePath |= HasUserProvidedValue(dependencyObject, Selector.SelectedValuePathProperty);
            }

            /// analyze the actual source type to determine default paths
            var actualSource = ResolveActualSource();
            if (actualSource?.GetType()?.IsListType(out var innerType) == true)
            {
                /// KeyValuePair usage
                if (innerType?.IsGenericType == true && innerType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    if (!hasDisplayMemberPath && selectionControl.ItemTemplate == null)
                        selectionControl.DisplayMemberPath = nameof(KeyValuePair<object, object>.Key);
                    if (!hasSelectedValuePath)
                        selectionControl.SelectedValuePath = nameof(KeyValuePair<object, object>.Value);
                }
                /// StswComboItem short usage
                else if (innerType?.IsAssignableTo(typeof(StswComboItem)) == true)
                {
                    if (!hasDisplayMemberPath && selectionControl.ItemTemplate == null)
                        selectionControl.DisplayMemberPath = nameof(StswComboItem.Display);
                    if (!hasSelectedValuePath)
                        selectionControl.SelectedValuePath = nameof(StswComboItem.Value);
                }
            }
        }

        /// defer applying default paths to ensure ItemsSource is fully updated
        if (selectionControl is DispatcherObject dispatcherObject && dispatcherObject.Dispatcher != null)
            dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(ApplyDefaultPaths));
        else
            ApplyDefaultPaths();
    }

    /// <summary>
    /// Handles changes to the <see cref="ItemTemplate"/> property, ensuring <see cref="DisplayMemberPath"/> is cleared when a template is used.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="itemTemplate">The new ItemTemplate value.</param>
    public static void ItemTemplateChanged(IStswSelectionControl selectionControl, DataTemplate itemTemplate)
    {
        if (itemTemplate != null && !string.IsNullOrEmpty(selectionControl.DisplayMemberPath))
            selectionControl.DisplayMemberPath = string.Empty;
    }

    /// <summary>
    /// Handles key preview events for selection controls, allowing specific keys to be processed
    /// </summary>
    /// <param name="selectionControl">The selection control to handle key events for.</param>
    /// <param name="e"> The key event arguments containing the key pressed.</param>
    public static bool PreviewKeyDown(IStswSelectionControl selectionControl, KeyEventArgs e)
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

    /// <summary>
    /// Handles selection changes by triggering animations on selected and unselected items.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="addedItems">The newly selected items.</param>
    /// <param name="removedItems">The deselected items.</param>
    public static void SelectionChanged(ItemsControl selectionControl, IList? addedItems, IList? removedItems)
    {
        if (StswApp.Settings.AnimationsEnabled && StswControl.GetEnableAnimations(selectionControl))
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
    /// Determines if a user has explicitly set a value for a given dependency property on the selection control.
    /// </summary>
    /// <param name="selectionControl">The selection control.</param>
    /// <param name="property">The dependency property to check.</param>
    /// <returns><see langword="true"/> if the user has provided a value; otherwise, <see langword="false"/>.</returns>
    private static bool HasUserProvidedValue(DependencyObject selectionControl, DependencyProperty property)
    {
        var valueSource = DependencyPropertyHelper.GetValueSource(selectionControl, property);
        return valueSource.BaseValueSource != BaseValueSource.Default || valueSource.IsExpression;
    }
}
