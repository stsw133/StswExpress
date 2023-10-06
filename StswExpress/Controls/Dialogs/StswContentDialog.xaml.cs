using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;

namespace StswExpress;

/// <summary>
/// Represents a control behaving like content dialog with various properties for customization.
/// </summary>
[TemplatePart(Name = PopupContentPartName, Type = typeof(ContentControl))]
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

    public const string PopupContentPartName = "PART_PopupContentElement";
    public const string OpenStateName = "Open";
    public const string ClosedStateName = "Closed";

    private ContentControl? _popupContentControl;

    public StswDialogSession? CurrentSession { get; private set; }
    private TaskCompletionSource<object?>? _dialogTaskCompletionSource;
    private IInputElement? _restoreFocusDialogClose;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _popupContentControl = GetTemplateChild(PopupContentPartName) as ContentControl;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in LoadedInstances.ToList())
        {
            if (weakRef.TryGetTarget(out StswContentDialog? dialog) && ReferenceEquals(dialog, this))
                return;
        }
        LoadedInstances.Add(new WeakReference<StswContentDialog>(this));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in LoadedInstances.ToList())
        {
            if (!weakRef.TryGetTarget(out StswContentDialog? dialog) || ReferenceEquals(dialog, this))
            {
                LoadedInstances.Remove(weakRef);
                break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string GetStateName() => IsOpen ? OpenStateName : ClosedStateName;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal UIElement? FocusPopup()
    {
        var child = _popupContentControl;
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
    /// 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    internal void AssertTargetableContent()
    {
        var existingBinding = BindingOperations.GetBindingExpression(this, DialogContentProperty);
        if (existingBinding != null)
            throw new InvalidOperationException("Content cannot be passed to a dialog via the OpenDialog if DialogContent already has a binding.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal void InternalClose(object? parameter)
    {
        var currentSession = CurrentSession ?? throw new InvalidOperationException($"{nameof(StswContentDialog)} does not have a current session");

        currentSession.CloseParameter = parameter;
        currentSession.IsEnded = true;

        SetCurrentValue(IsOpenProperty, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dialogIdentifier"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static StswContentDialog GetInstance(object? dialogIdentifier)
    {
        if (LoadedInstances.Count == 0)
            throw new InvalidOperationException("No loaded StswContentDialog instances.");

        List<StswContentDialog> targets = new();
        foreach (var instance in LoadedInstances.ToList())
        {
            if (instance.TryGetTarget(out StswContentDialog? dialogInstance))
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
            throw new InvalidOperationException($"No loaded StswContentDialog have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException("Multiple viable StswContentDialogs. Specify a unique Identifier on each StswContentDialog, especially where multiple Windows are a concern.");

        return targets[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task<object?> ShowInternal(object content)
    {
        if (IsOpen)
            throw new InvalidOperationException("StswContentDialog is already open.");

        _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

        AssertTargetableContent();

        if (content != null)
            DialogContent = content;

        SetCurrentValue(IsOpenProperty, true);

        object? result = await _dialogTaskCompletionSource.Task;

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="dialogIdentifier"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<object?> Show(object content, object? dialogIdentifier)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        return await GetInstance(dialogIdentifier).ShowInternal(content);
    }

    /// <summary>
    /// Close a modal dialog.
    /// </summary>
    /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifier set in XAML. </param>
    public static void Close(object? dialogIdentifier) => Close(dialogIdentifier, null);

    /// <summary>
    /// Close a modal dialog.
    /// </summary>
    /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifier set in XAML. </param>
    /// <param name="parameter"> to provide to close handler</param>
    public static void Close(object? dialogIdentifier, object? parameter)
    {
        var dialogHost = GetInstance(dialogIdentifier);
        if (dialogHost.CurrentSession is { } currentSession)
        {
            currentSession.Close(parameter);
            return;
        }
        throw new InvalidOperationException("DialogHost is not open.");
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
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
    /// 
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
    /// 
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
    /// 
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
    /// 
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
            typeof(StswContentDialog)
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
                    {
                        session.Close(session.CloseParameter);
                    }
                    //StswDialogSession.Close may attempt to cancel the closing of the dialog.
                    //When the dialog is closed in this manner it is not valid
                    if (!session.IsEnded)
                    {
                        throw new InvalidOperationException($"Cannot cancel dialog closing after {nameof(IsOpen)} property has been set to {bool.FalseString}");
                    }
                    closeParameter = session.CloseParameter;
                    stsw.CurrentSession = null;
                }

                //NB: _dialogTaskCompletionSource is only set in the case where the dialog is shown with Show
                stsw._dialogTaskCompletionSource?.TrySetResult(closeParameter);

                // Don't attempt to Invoke if _restoreFocusDialogClose hasn't been assigned yet. Can occur
                // if the MainWindow has started up minimized. Even when Show() has been called, this doesn't
                // seem to have been set.
                stsw.Dispatcher.InvokeAsync(() => stsw._restoreFocusDialogClose?.Focus(), DispatcherPriority.Input);

                return;
            }

            stsw.CurrentSession = new StswDialogSession(stsw);
            var window = Window.GetWindow(stsw);
            if (!stsw.IsRestoreFocusDisabled)
            {
                stsw._restoreFocusDialogClose = window != null ? FocusManager.GetFocusedElement(window) : null;

                // Check restore focus override
                if (stsw._restoreFocusDialogClose is DependencyObject dependencyObj &&
                    GetRestoreFocusElement(dependencyObj) is { } focusOverride)
                {
                    stsw._restoreFocusDialogClose = focusOverride;
                }
            }

            stsw.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                CommandManager.InvalidateRequerySuggested();
                UIElement? child = stsw.FocusPopup();

                if (child != null)
                {
                    Task.Delay(300).ContinueWith(t => child.Dispatcher.BeginInvoke(new Action(() => child.InvalidateVisual())));
                }
            }));
        }
    }

    /// <summary>
    /// 
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
    /// 
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
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswContentDialog)
        );

    /// <summary>
    /// Represents the brush for the Dialog's background
    /// </summary>
    public Brush? DialogBackground
    {
        get => (Brush?)GetValue(DialogBackgroundProperty);
        set => SetValue(DialogBackgroundProperty, value);
    }
    public static readonly DependencyProperty DialogBackgroundProperty
        = DependencyProperty.Register(
            nameof(DialogBackground),
            typeof(Brush),
            typeof(StswContentDialog)
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
/// 
/// </summary>
public static class StswDialogExtensions
{
    public static IEnumerable<DependencyObject> VisualDepthFirstTraversal(this DependencyObject node)
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
