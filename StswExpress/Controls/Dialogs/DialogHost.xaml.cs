using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
[TemplatePart(Name = PopupContentPartName, Type = typeof(ContentControl))]
internal class DialogHost : ContentControl
{
    #region Properties
    private static readonly HashSet<WeakReference<DialogHost>> LoadedInstances = new();

    public const string PopupContentPartName = "PART_PopupContentElement";
    public const string OpenStateName = "Open";
    public const string ClosedStateName = "Closed";

    private ContentControl? _popupContentControl;

    public DialogSession? CurrentSession { get; private set; }
    private TaskCompletionSource<object?>? _dialogTaskCompletionSource;
    private IInputElement? _restoreFocusDialogClose;
    #endregion

    #region Constructor
    static DialogHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogHost), new FrameworkPropertyMetadata(typeof(DialogHost)));
    }
    public DialogHost()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    #endregion

    #region OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _popupContentControl = GetTemplateChild(PopupContentPartName) as ContentControl;
    }
    #endregion

    #region Dependency Properties
    #region Identifier
    public static readonly DependencyProperty IdentifierProperty = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(DialogHost),
            new PropertyMetadata(default(object)));

    /// <summary>
    /// Identifier which is used in conjunction with <see cref="Show(object)"/> to determine where a dialog should be shown.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    #endregion
    #region Dialog Content
    public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register(
            nameof(DialogContent),
            typeof(object),
            typeof(DialogHost),
            new PropertyMetadata(default(object)));

    public object? DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }
    #endregion
    #region Dialog Content Template
    public static readonly DependencyProperty DialogContentTemplateProperty = DependencyProperty.Register(
        nameof(DialogContentTemplate),
        typeof(DataTemplate),
        typeof(DialogHost),
        new PropertyMetadata(default(DataTemplate)));

    public DataTemplate? DialogContentTemplate
    {
        get => (DataTemplate?)GetValue(DialogContentTemplateProperty);
        set => SetValue(DialogContentTemplateProperty, value);
    }
    #endregion
    #region Dialog Content Template Selector
    public static readonly DependencyProperty DialogContentTemplateSelectorProperty = DependencyProperty.Register(
        nameof(DialogContentTemplateSelector),
        typeof(DataTemplateSelector),
        typeof(DialogHost),
        new PropertyMetadata(default(DataTemplateSelector)));
    public DataTemplateSelector? DialogContentTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(DialogContentTemplateSelectorProperty);
        set => SetValue(DialogContentTemplateSelectorProperty, value);
    }
    #endregion
    #region Dialog Content StringFormat
    public static readonly DependencyProperty DialogContentStringFormatProperty = DependencyProperty.Register(
        nameof(DialogContentStringFormat), typeof(string), typeof(DialogHost), new PropertyMetadata(default(string)));

    public string? DialogContentStringFormat
    {
        get => (string?)GetValue(DialogContentStringFormatProperty);
        set => SetValue(DialogContentStringFormatProperty, value);
    }
    #endregion
    #region Dialog Margin
    public static readonly DependencyProperty DialogMarginProperty = DependencyProperty.Register(
        nameof(DialogMargin), typeof(Thickness), typeof(DialogHost), new PropertyMetadata(default(Thickness)));

    public Thickness DialogMargin
    {
        get => (Thickness)GetValue(DialogMarginProperty);
        set => SetValue(DialogMarginProperty, value);
    }
    #endregion
    #region Dialog Background
    public static readonly DependencyProperty DialogBackgroundProperty = DependencyProperty.Register(
    nameof(DialogBackground),
    typeof(Brush),
    typeof(DialogHost),
    new PropertyMetadata(null));

    /// <summary>
    /// Represents the brush for the Dialog's background
    /// </summary>
    public Brush? DialogBackground
    {
        get => (Brush?)GetValue(DialogBackgroundProperty);
        set => SetValue(DialogBackgroundProperty, value);
    }
    #endregion
    #region Is Restore Focus Disabled
    public static readonly DependencyProperty IsRestoreFocusDisabledProperty = DependencyProperty.Register(
            nameof(IsRestoreFocusDisabled), typeof(bool), typeof(DialogHost), new PropertyMetadata(false));

    public bool IsRestoreFocusDisabled
    {
        get => (bool)GetValue(IsRestoreFocusDisabledProperty);
        set => SetValue(IsRestoreFocusDisabledProperty, value);
    }
    #endregion
    #region RestoreFocusElement
    public static readonly DependencyProperty RestoreFocusElementProperty = DependencyProperty.RegisterAttached(
    "RestoreFocusElement", typeof(IInputElement), typeof(DialogHost), new PropertyMetadata(default(IInputElement)));
    public static void SetRestoreFocusElement(DependencyObject element, IInputElement value)
        => element.SetValue(RestoreFocusElementProperty, value);
    public static IInputElement GetRestoreFocusElement(DependencyObject element)
        => (IInputElement)element.GetValue(RestoreFocusElementProperty);
    #endregion
    #region Is Open
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            nameof(IsOpen), typeof(bool), typeof(DialogHost), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsOpenPropertyChangedCallback));

    private static void IsOpenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        var dialogHost = (DialogHost)dependencyObject;

        VisualStateManager.GoToState(dialogHost, dialogHost.GetStateName(), true);

        if (!dialogHost.IsOpen)
        {
            object? closeParameter = null;
            if (dialogHost.CurrentSession is { } session)
            {
                if (!session.IsEnded)
                {
                    session.Close(session.CloseParameter);
                }
                //DialogSession.Close may attempt to cancel the closing of the dialog.
                //When the dialog is closed in this manner it is not valid
                if (!session.IsEnded)
                {
                    throw new InvalidOperationException($"Cannot cancel dialog closing after {nameof(IsOpen)} property has been set to {bool.FalseString}");
                }
                closeParameter = session.CloseParameter;
                dialogHost.CurrentSession = null;
            }

            //NB: _dialogTaskCompletionSource is only set in the case where the dialog is shown with Show
            dialogHost._dialogTaskCompletionSource?.TrySetResult(closeParameter);

            // Don't attempt to Invoke if _restoreFocusDialogClose hasn't been assigned yet. Can occur
            // if the MainWindow has started up minimized. Even when Show() has been called, this doesn't
            // seem to have been set.
            dialogHost.Dispatcher.InvokeAsync(() => dialogHost._restoreFocusDialogClose?.Focus(), DispatcherPriority.Input);

            return;
        }

        dialogHost.CurrentSession = new DialogSession(dialogHost);
        var window = Window.GetWindow(dialogHost);
        if (!dialogHost.IsRestoreFocusDisabled)
        {
            dialogHost._restoreFocusDialogClose = window != null ? FocusManager.GetFocusedElement(window) : null;

            // Check restore focus override
            if (dialogHost._restoreFocusDialogClose is DependencyObject dependencyObj &&
                GetRestoreFocusElement(dependencyObj) is { } focusOverride)
            {
                dialogHost._restoreFocusDialogClose = focusOverride;
            }
        }

        dialogHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            CommandManager.InvalidateRequerySuggested();
            UIElement? child = dialogHost.FocusPopup();

            if (child != null)
            {
                Task.Delay(300).ContinueWith(t => child.Dispatcher.BeginInvoke(new Action(() => child.InvalidateVisual())));
            }
        }));
    }
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    #endregion
    #region OverlayBackground
    public static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register(
            nameof(OverlayBackground), typeof(Brush), typeof(DialogHost), new PropertyMetadata(Brushes.Black));

    /// <summary>
    /// Represents the overlay brush that is used to dim the background behind the dialog
    /// </summary>
    public Brush? OverlayBackground
    {
        get => (Brush?)GetValue(OverlayBackgroundProperty);
        set => SetValue(OverlayBackgroundProperty, value);
    }
    #endregion
    #endregion

    #region EventHandlers
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in LoadedInstances.ToList())
        {
            if (weakRef.TryGetTarget(out DialogHost? dialogHost) && ReferenceEquals(dialogHost, this))
            {
                return;
            }
        }

        LoadedInstances.Add(new WeakReference<DialogHost>(this));
    }
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in LoadedInstances.ToList())
        {
            if (!weakRef.TryGetTarget(out DialogHost? dialogHost) || ReferenceEquals(dialogHost, this))
            {
                LoadedInstances.Remove(weakRef);
                break;
            }
        }
    }
    #endregion

    #region Functions
    private string GetStateName()
            => IsOpen ? OpenStateName : ClosedStateName;

    internal UIElement? FocusPopup()
    {
        var child = _popupContentControl;
        if (child is null) return null;

        CommandManager.InvalidateRequerySuggested();
        var focusable = child.VisualDepthFirstTraversal().OfType<UIElement>().FirstOrDefault(ui => ui.Focusable && ui.IsVisible);
        focusable?.Dispatcher.InvokeAsync(() =>
        {
            if (!focusable.Focus()) return;
            focusable.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }, DispatcherPriority.Background);

        return child;
    }

    internal void AssertTargetableContent()
    {
        var existingBinding = BindingOperations.GetBindingExpression(this, DialogContentProperty);
        if (existingBinding != null)
            throw new InvalidOperationException(
                "Content cannot be passed to a dialog via the OpenDialog if DialogContent already has a binding.");
    }

    internal void InternalClose(object? parameter)
    {
        var currentSession = CurrentSession ?? throw new InvalidOperationException($"{nameof(DialogHost)} does not have a current session");

        currentSession.CloseParameter = parameter;
        currentSession.IsEnded = true;

        SetCurrentValue(IsOpenProperty, false);
    }

    private static DialogHost GetInstance(object? dialogIdentifier)
    {
        if (LoadedInstances.Count == 0)
            throw new InvalidOperationException("No loaded DialogHost instances.");

        List<DialogHost> targets = new();
        foreach (var instance in LoadedInstances.ToList())
        {
            if (instance.TryGetTarget(out DialogHost? dialogInstance))
            {
                object? identifier = null;
                if (dialogInstance.CheckAccess())
                {
                    identifier = dialogInstance.Identifier;
                }
                else
                {
                    // Retrieve the identifier using the Dispatcher on the owning thread (effectively replaces the VerifyAccess() call previously used)
                    identifier = dialogInstance.Dispatcher.Invoke(() => dialogInstance.Identifier);
                }
                if (Equals(dialogIdentifier, identifier))
                {
                    targets.Add(dialogInstance);
                }
            }
            else
            {
                LoadedInstances.Remove(instance);
            }
        }

        if (targets.Count == 0)
            throw new InvalidOperationException($"No loaded DialogHost have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException("Multiple viable DialogHosts. Specify a unique Identifier on each DialogHost, especially where multiple Windows are a concern.");

        return targets[0];
    }

    internal async Task<object?> ShowInternal(object content)
    {
        if (IsOpen)
            throw new InvalidOperationException("DialogHost is already open.");

        _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

        AssertTargetableContent();

        if (content != null)
            DialogContent = content;

        SetCurrentValue(IsOpenProperty, true);

        object? result = await _dialogTaskCompletionSource.Task;

        return result;
    }
    #endregion

    #region Show
    public static async Task<object?> Show(object content, object? dialogIdentifier)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        return await GetInstance(dialogIdentifier).ShowInternal(content);
    }
    #endregion

    #region Close
    /// <summary>
    ///  Close a modal dialog.
    /// </summary>
    /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifier set in XAML. </param>
    public static void Close(object? dialogIdentifier)
        => Close(dialogIdentifier, null);

    /// <summary>
    ///  Close a modal dialog.
    /// </summary>
    /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifier set in XAML. </param>
    /// <param name="parameter"> to provide to close handler</param>
    public static void Close(object? dialogIdentifier, object? parameter)
    {
        DialogHost dialogHost = GetInstance(dialogIdentifier);
        if (dialogHost.CurrentSession is { } currentSession)
        {
            currentSession.Close(parameter);
            return;
        }
        throw new InvalidOperationException("DialogHost is not open.");
    }
    #endregion
}

/// <summary>
/// Allows an open dialog to be managed. Use is only permitted during a single display operation.
/// </summary>
public class DialogSession
{
    private readonly DialogHost _owner;

    internal DialogSession(DialogHost owner)
        => _owner = owner ?? throw new ArgumentNullException(nameof(owner));

    /// <summary>
    /// Indicates if the dialog session has ended.  Once ended no further method calls will be permitted.
    /// </summary>
    /// <remarks>
    /// Client code cannot set this directly, this is internally managed.  To end the dialog session use <see cref="Close()"/>.
    /// </remarks>
    public bool IsEnded { get; internal set; }

    /// <summary>
    /// The parameter passed to the <see cref="DialogHost.CloseDialogCommand" /> and return by <see cref="DialogHost.Show(object)"/>
    /// </summary>
    internal object? CloseParameter { get; set; }

    /// <summary>
    /// Gets the <see cref="DialogHost.DialogContent"/> which is currently displayed, so this could be a view model or a UI element.
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
        _owner.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            _owner.FocusPopup();
        }));
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the dialog session has ended, or a close operation is currently in progress.</exception>
    public void Close()
    {
        if (IsEnded) throw new InvalidOperationException("Dialog session has ended.");

        _owner.InternalClose(null);
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    /// <param name="parameter">Result parameter which will be returned in <see cref="DialogClosingEventArgs.Parameter"/> or from <see cref="DialogHost.Show(object)"/> method.</param>
    /// <exception cref="InvalidOperationException">Thrown if the dialog session has ended, or a close operation is currently in progress.</exception>
    public void Close(object? parameter)
    {
        if (IsEnded) throw new InvalidOperationException("Dialog session has ended.");

        _owner.InternalClose(parameter);
    }
}

public static class DialogExtensions
{
    public static IEnumerable<DependencyObject> VisualDepthFirstTraversal(this DependencyObject node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));

        yield return node;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
        {
            var child = VisualTreeHelper.GetChild(node, i);
            foreach (var descendant in child.VisualDepthFirstTraversal())
            {
                yield return descendant;
            }
        }
    }
}
