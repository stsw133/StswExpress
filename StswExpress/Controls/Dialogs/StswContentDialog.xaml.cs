using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// Represents a customizable content dialog control for displaying various types of content.
/// </summary>
[TemplatePart(Name = "PART_PopupContentElement", Type = typeof(ContentControl))]
public class StswContentDialog : ContentControl
{
    public StswContentDialog()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    static StswContentDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswContentDialog), new FrameworkPropertyMetadata(typeof(StswContentDialog)));
    }

    #region Events & methods
    private static readonly HashSet<WeakReference<StswContentDialog>> LoadedInstances = new();

    private ContentControl? _popupContentElement;

    public StswDialogSession? CurrentSession { get; private set; }
    private TaskCompletionSource<object?>? _dialogTaskCompletionSource;
    private IInputElement? _restoreFocusDialogClose;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_PopupContentElement") is ContentControl popupContentElement)
            _popupContentElement = popupContentElement;
    }

    /// <summary>
    /// Handles the Loaded event to track the instances of the dialog.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in LoadedInstances.ToList())
            if (weakRef.TryGetTarget(out StswContentDialog? dialog) && ReferenceEquals(dialog, this))
                return;
        LoadedInstances.Add(new WeakReference<StswContentDialog>(this));
    }

    /// <summary>
    /// Handles the Unloaded event to remove the instances of the dialog.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in LoadedInstances.ToList())
            if (!weakRef.TryGetTarget(out StswContentDialog? dialog) || ReferenceEquals(dialog, this))
            {
                LoadedInstances.Remove(weakRef);
                break;
            }
    }

    /// <summary>
    /// Gets the name of the current state of the dialog (Open or Closed).
    /// </summary>
    /// <returns>The name of the current state.</returns>
    private string GetStateName() => IsOpen ? "Open" : "Closed";

    /// <summary>
    /// Focuses the popup content element within the dialog, if available.
    /// </summary>
    /// <returns>The focused UI element within the popup content.</returns>
    internal UIElement? FocusPopup()
    {
        var child = _popupContentElement;
        if (child is null)
            return null;

        CommandManager.InvalidateRequerySuggested();
        var focusable = child.VisualDepthFirstTraversal().OfType<UIElement>().FirstOrDefault(ui => ui.Focusable && ui.IsVisible);
        focusable?.Dispatcher.InvokeAsync(() =>
        {
            if (!focusable.Focus()) return;
            focusable.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }, DispatcherPriority.Background);

        return child;
    }

    /// <summary>
    /// Asserts that the content can be targeted for display in the dialog.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when content cannot be passed to the dialog.</exception>
    internal void AssertTargetableContent()
    {
        if (BindingOperations.GetBindingExpression(this, DialogContentProperty) != null)
            throw new InvalidOperationException("Content cannot be passed to a dialog via the OpenDialog if DialogContent already has a binding.");
    }

    /// <summary>
    /// Closes the dialog internally with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass on dialog closure.</param>
    /// <exception cref="InvalidOperationException">Thrown when content dialog does not have a current session.</exception>
    internal void InternalClose(object? parameter)
    {
        var currentSession = CurrentSession ?? throw new InvalidOperationException($"{nameof(StswContentDialog)} does not have a current session");

        currentSession.CloseParameter = parameter;
        currentSession.IsEnded = true;

        SetCurrentValue(IsOpenProperty, false);
    }

    /// <summary>
    /// Retrieves an instance of the dialog based on the provided identifier.
    /// </summary>
    /// <param name="dialogIdentifier">The identifier of the dialog instance.</param>
    /// <returns>The instance of the dialog.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching dialog instance is found.</exception>
    internal static StswContentDialog GetInstance(object? dialogIdentifier)
    {
        if (LoadedInstances.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswContentDialog)} instances.");

        var targets = new List<StswContentDialog>();
        foreach (var instance in LoadedInstances.ToList())
        {
            if (instance.TryGetTarget(out var dialogInstance))
            {
                object? identifier = null;

                if (dialogInstance.CheckAccess())
                    identifier = dialogInstance.Identifier;
                else
                    identifier = dialogInstance.Dispatcher.Invoke(() => dialogInstance.Identifier);

                if (Equals(dialogIdentifier, identifier))
                    targets.Add(dialogInstance);
            }
            else LoadedInstances.Remove(instance);
        }

        if (targets.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswContentDialog)} have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException($"Multiple viable {nameof(StswContentDialog)}s. Specify a unique Identifier on each {nameof(StswContentDialog)}, especially where multiple Windows are a concern.");

        return targets[0];
    }

    /// <summary>
    /// Shows the dialog internally with the specified content.
    /// </summary>
    /// <param name="content">The content to display in the dialog.</param>
    /// <returns>A task representing the asynchronous operation with the dialog.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the dialog is already open.</exception>
    internal async Task<object?> ShowInternal(object content)
    {
        if (IsOpen)
            throw new InvalidOperationException($"{nameof(StswContentDialog)} is already open.");

        _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

        AssertTargetableContent();

        if (content != null)
            DialogContent = content;

        SetCurrentValue(IsOpenProperty, true);

        object? result = await _dialogTaskCompletionSource.Task;

        return result;
    }

    /// <summary>
    /// Shows a modal dialog with the specified content and identifier.
    /// </summary>
    /// <param name="content">The content to display in the dialog.</param>
    /// <param name="dialogIdentifier">The identifier of the dialog instance.</param>
    /// <returns>A task representing the asynchronous operation with the dialog.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content is null.</exception>
    public static async Task<object?> Show(object content, object? dialogIdentifier)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        return await GetInstance(dialogIdentifier).ShowInternal(content);
    }

    /// <summary>
    /// Closes a modal dialog.
    /// </summary>
    /// <param name="dialogIdentifier">The identifier of the dialog instance to be closed.</param>
    public static void Close(object? dialogIdentifier) => Close(dialogIdentifier, null);

    /// <summary>
    /// Closes a modal dialog with a specified parameter.
    /// </summary>
    /// <param name="dialogIdentifier">The identifier of the dialog instance to be closed.</param>
    /// <param name="parameter">The parameter to pass on dialog closure.</param>
    public static void Close(object? dialogIdentifier, object? parameter)
    {
        if (GetInstance(dialogIdentifier).CurrentSession is { } currentSession)
        {
            currentSession.Close(parameter);
            return;
        }
        //throw new InvalidOperationException("DialogHost is not open.");
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the content to be displayed within the dialog.
    /// </summary>
    public object? DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }
    public static readonly DependencyProperty DialogContentProperty
        = DependencyProperty.Register(
            nameof(DialogContent),
            typeof(object),
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Gets or sets the string format for the dialog content.
    /// </summary>
    public string? DialogContentStringFormat
    {
        get => (string?)GetValue(DialogContentStringFormatProperty);
        set => SetValue(DialogContentStringFormatProperty, value);
    }
    public static readonly DependencyProperty DialogContentStringFormatProperty
        = DependencyProperty.Register(
            nameof(DialogContentStringFormat),
            typeof(string),
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Gets or sets the data template for the dialog content.
    /// </summary>
    public DataTemplate? DialogContentTemplate
    {
        get => (DataTemplate?)GetValue(DialogContentTemplateProperty);
        set => SetValue(DialogContentTemplateProperty, value);
    }
    public static readonly DependencyProperty DialogContentTemplateProperty
        = DependencyProperty.Register(
            nameof(DialogContentTemplate),
            typeof(DataTemplate),
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Gets or sets the data template selector for the dialog content.
    /// </summary>
    public DataTemplateSelector? DialogContentTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(DialogContentTemplateSelectorProperty);
        set => SetValue(DialogContentTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty DialogContentTemplateSelectorProperty
        = DependencyProperty.Register(
            nameof(DialogContentTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Gets or sets the margin for the dialog.
    /// </summary>
    public Thickness DialogMargin
    {
        get => (Thickness)GetValue(DialogMarginProperty);
        set => SetValue(DialogMarginProperty, value);
    }
    public static readonly DependencyProperty DialogMarginProperty
        = DependencyProperty.Register(
            nameof(DialogMargin),
            typeof(Thickness),
            typeof(StswContentDialog),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

    /// <summary>
    /// Identifier which is used in conjunction with <see cref="Show(object)"/> to determine where a dialog should be shown.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    public static readonly DependencyProperty IdentifierProperty
        = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the content dialog is open or not.
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty
        = DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(StswContentDialog),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static void OnIsOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswContentDialog stsw)
        {
            VisualStateManager.GoToState(stsw, stsw.GetStateName(), true);

            if (!stsw.IsOpen)
            {
                object? closeParameter = null;
                if (stsw.CurrentSession is { } session)
                {
                    if (!session.IsEnded)
                        session.Close(session.CloseParameter);
                    
                    if (!session.IsEnded)
                        throw new InvalidOperationException($"Cannot cancel dialog closing after {nameof(IsOpen)} property has been set to {bool.FalseString}");
                    
                    closeParameter = session.CloseParameter;
                    stsw.CurrentSession = null;
                }
                stsw._dialogTaskCompletionSource?.TrySetResult(closeParameter);
                stsw.Dispatcher.InvokeAsync(() => stsw._restoreFocusDialogClose?.Focus(), DispatcherPriority.Input);

                return;
            }

            stsw.CurrentSession = new StswDialogSession(stsw);
            var window = Window.GetWindow(stsw);
            if (!stsw.IsRestoreFocusDisabled)
            {
                stsw._restoreFocusDialogClose = window != null ? FocusManager.GetFocusedElement(window) : null;
                if (stsw._restoreFocusDialogClose is DependencyObject dependencyObj && GetRestoreFocusElement(dependencyObj) is { } focusOverride)
                    stsw._restoreFocusDialogClose = focusOverride;
            }

            stsw.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                CommandManager.InvalidateRequerySuggested();
                var child = stsw.FocusPopup();
                if (child != null)
                    Task.Delay(300).ContinueWith(t => child.Dispatcher.BeginInvoke(new Action(() => child.InvalidateVisual())));
            }));
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the focus restoration after dialog closure is disabled.
    /// </summary>
    public bool IsRestoreFocusDisabled
    {
        get => (bool)GetValue(IsRestoreFocusDisabledProperty);
        set => SetValue(IsRestoreFocusDisabledProperty, value);
    }
    public static readonly DependencyProperty IsRestoreFocusDisabledProperty
        = DependencyProperty.Register(
            nameof(IsRestoreFocusDisabled),
            typeof(bool),
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Identifies the RestoreFocusElement attached property.
    /// </summary>
    public static readonly DependencyProperty RestoreFocusElementProperty
        = DependencyProperty.RegisterAttached(
            "RestoreFocusElement",
            typeof(IInputElement),
            typeof(StswContentDialog)
        );
    public static IInputElement GetRestoreFocusElement(DependencyObject element) => (IInputElement)element.GetValue(RestoreFocusElementProperty);
    public static void SetRestoreFocusElement(DependencyObject element, IInputElement value) => element.SetValue(RestoreFocusElementProperty, value);
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswContentDialog),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Represents the brush for the Dialog's background
    /// </summary>
    public Brush DialogBackground
    {
        get => (Brush)GetValue(DialogBackgroundProperty);
        set => SetValue(DialogBackgroundProperty, value);
    }
    public static readonly DependencyProperty DialogBackgroundProperty
        = DependencyProperty.Register(
            nameof(DialogBackground),
            typeof(Brush),
            typeof(StswContentDialog),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/// <summary>
/// Allows an open dialog to be managed. Use is only permitted during a single display operation.
/// </summary>
public class StswDialogSession
{
    private readonly StswContentDialog _owner;

    internal StswDialogSession(StswContentDialog owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <summary>
    /// Indicates if the dialog session has ended.  Once ended no further method calls will be permitted.
    /// </summary>
    /// <remarks>
    /// Client code cannot set this directly, this is internally managed.  To end the dialog session use <see cref="Close()"/>.
    /// </remarks>
    public bool IsEnded { get; internal set; }

    /// <summary>
    /// The parameter passed to the <see cref="StswContentDialog.CloseDialogCommand" /> and return by <see cref="StswContentDialog.Show(object)"/>
    /// </summary>
    internal object? CloseParameter { get; set; }

    /// <summary>
    /// Gets the <see cref="StswContentDialog.DialogContent"/> which is currently displayed, so this could be a view model or a UI element.
    /// </summary>
    public object? Content => _owner.DialogContent;

    /// <summary>
    /// Update the current content in the dialog.
    /// </summary>
    /// <param name="content"></param>
    public void UpdateContent(object? content)
    {
        _owner.AssertTargetableContent();
        _owner.DialogContent = content;
        _owner.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _owner.FocusPopup()));
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the dialog session has ended, or a close operation is currently in progress.</exception>
    public void Close()
    {
        if (IsEnded)
            throw new InvalidOperationException("Dialog session has ended.");
        _owner.InternalClose(null);
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    /// <param name="parameter">Result parameter which will be returned in <see cref="DialogClosingEventArgs.Parameter"/> or from <see cref="StswContentDialog.Show(object)"/> method.</param>
    /// <exception cref="InvalidOperationException">Thrown if the dialog session has ended, or a close operation is currently in progress.</exception>
    public void Close(object? parameter)
    {
        if (IsEnded)
            throw new InvalidOperationException("Dialog session has ended.");
        _owner.InternalClose(parameter);
    }
}

/// <summary>
/// Contains extension methods for visual tree traversal.
/// </summary>
internal static class StswDialogExtensions
{
    internal static IEnumerable<DependencyObject> VisualDepthFirstTraversal(this DependencyObject node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        yield return node;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
        {
            var child = VisualTreeHelper.GetChild(node, i);
            foreach (var descendant in VisualDepthFirstTraversal(child))
                yield return descendant;
        }
    }
}
