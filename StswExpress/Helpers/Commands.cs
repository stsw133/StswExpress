using System;
using System.Windows.Input;

namespace StswExpress;

public class RelayCommand : ICommand
{
    private Action mAction;
    private bool mCanExecute;
    public event EventHandler? CanExecuteChanged = (sender, e) => { };

    public RelayCommand(Action action, bool canExecute)
    {
        mAction = action;
        mCanExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => mCanExecute;
    public void Execute(object? parameter) => mAction();
}

public static class Commands
{
    public enum Type
    {
        ADD,
        ARCHIVE,
        CLEAR,
        DECREASE,
        DELETE,
        DUPLICATE,
        EDIT,
        FIND,
        FULLSCREEN,
        HELP,
        INCREASE,
        LIST,
        NEW,
        PREVIEW,
        PRINT,
        REFRESH,
        REMOVE,
        SAVE,
        SELECT,
        SETTINGS
    }

    /// <summary>
    /// Add [Insert]
    /// </summary>
    public static KeyGesture AddKey => new KeyGesture(Key.Insert);
    public static readonly RoutedUICommand Add = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.ADD),
        Enum.GetName(typeof(Type), Type.ADD),
        typeof(Commands),
        new InputGestureCollection() { AddKey }
    );
    /// <summary>
    /// Archive [Ctrl + I]
    /// </summary>
    public static KeyGesture ArchiveKey => new KeyGesture(Key.I, ModifierKeys.Control);
    public static readonly RoutedUICommand Archive = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.ARCHIVE),
        Enum.GetName(typeof(Type), Type.ARCHIVE),
        typeof(Commands),
        new InputGestureCollection() { ArchiveKey }
    );
    /// <summary>
    /// Clear [F9]
    /// </summary>
    public static KeyGesture ClearKey => new KeyGesture(Key.F9);
    public static readonly RoutedUICommand Clear = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.CLEAR),
        Enum.GetName(typeof(Type), Type.CLEAR),
        typeof(Commands),
        new InputGestureCollection() { ClearKey }
    );
    /// <summary>
    /// Decrease [-]
    /// </summary>
    public static KeyGesture DecreaseKey => new KeyGesture(Key.OemMinus);
    public static readonly RoutedUICommand Decrease = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.DECREASE),
        Enum.GetName(typeof(Type), Type.DECREASE),
        typeof(Commands),
        new InputGestureCollection() { DecreaseKey }
    );
    /// <summary>
    /// Delete
    /// </summary>
    //public static KeyGesture DeleteKey => new KeyGesture(Key.Delete);
    public static readonly RoutedUICommand Delete = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.DELETE),
        Enum.GetName(typeof(Type), Type.DELETE),
        typeof(Commands)
    );
    /// <summary>
    /// Duplicate [Ctrl + Insert, Ctrl + D]
    /// </summary>
    public static KeyGesture Duplicate1Key => new KeyGesture(Key.Insert, ModifierKeys.Control);
    public static KeyGesture Duplicate2Key => new KeyGesture(Key.D, ModifierKeys.Control);
    public static readonly RoutedUICommand Duplicate = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.DUPLICATE),
        Enum.GetName(typeof(Type), Type.DUPLICATE),
        typeof(Commands),
        new InputGestureCollection() { Duplicate1Key, Duplicate2Key }
    );
    /// <summary>
    /// Edit [Ctrl + E]
    /// </summary>
    public static KeyGesture EditKey => new KeyGesture(Key.E, ModifierKeys.Control);
    public static readonly RoutedUICommand Edit = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.EDIT),
        Enum.GetName(typeof(Type), Type.EDIT),
        typeof(Commands),
        new InputGestureCollection() { EditKey }
    );
    /// <summary>
    /// Find [Ctrl + F]
    /// </summary>
    public static KeyGesture FindKey => new KeyGesture(Key.F, ModifierKeys.Control);
    public static readonly RoutedUICommand Find = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.FIND),
        Enum.GetName(typeof(Type), Type.FIND),
        typeof(Commands),
        new InputGestureCollection() { FindKey }
    );
    /// <summary>
    /// Fullscreen [F11]
    /// </summary>
    public static KeyGesture FullscreenKey => new KeyGesture(Key.F11);
    public static readonly RoutedUICommand Fullscreen = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.FULLSCREEN),
        Enum.GetName(typeof(Type), Type.FULLSCREEN),
        typeof(Commands),
        new InputGestureCollection() { FullscreenKey }
    );
    /// <summary>
    /// Help [F1]
    /// </summary>
    public static KeyGesture HelpKey => new KeyGesture(Key.F1);
    public static readonly RoutedUICommand Help = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.HELP),
        Enum.GetName(typeof(Type), Type.HELP),
        typeof(Commands),
        new InputGestureCollection() { HelpKey }
    );
    /// <summary>
    /// Increase [+]
    /// </summary>
    public static KeyGesture IncreaseKey => new KeyGesture(Key.OemPlus);
    public static readonly RoutedUICommand Increase = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.INCREASE),
        Enum.GetName(typeof(Type), Type.INCREASE),
        typeof(Commands),
        new InputGestureCollection() { IncreaseKey }
    );
    /// <summary>
    /// List [F3]
    /// </summary>
    public static KeyGesture ListKey => new KeyGesture(Key.F3);
    public static readonly RoutedUICommand List = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.LIST),
        Enum.GetName(typeof(Type), Type.LIST),
        typeof(Commands),
        new InputGestureCollection() { ListKey }
    );

    /// <summary>
    /// New [Ctrl + N]
    /// </summary>
    public static KeyGesture NewKey => new KeyGesture(Key.N, ModifierKeys.Control);
    public static readonly RoutedUICommand New = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.NEW),
        Enum.GetName(typeof(Type), Type.NEW),
        typeof(Commands),
        new InputGestureCollection() { NewKey }
    );
    /// <summary>
    /// Preview [Ctrl + P]
    /// </summary>
    public static KeyGesture PreviewKey => new KeyGesture(Key.P, ModifierKeys.Control);
    public static readonly RoutedUICommand Preview = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.PREVIEW),
        Enum.GetName(typeof(Type), Type.PREVIEW),
        typeof(Commands),
        new InputGestureCollection() { PreviewKey }
    );
    /// <summary>
    /// Print [F4]
    /// </summary>
    public static KeyGesture PrintKey => new KeyGesture(Key.F4);
    public static readonly RoutedUICommand Print = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.PRINT),
        Enum.GetName(typeof(Type), Type.PRINT),
        typeof(Commands),
        new InputGestureCollection() { PrintKey }
    );
    /// <summary>
    /// Refresh [F5]
    /// </summary>
    public static KeyGesture RefreshKey => new KeyGesture(Key.F5);
    public static readonly RoutedUICommand Refresh = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.REFRESH),
        Enum.GetName(typeof(Type), Type.REFRESH),
        typeof(Commands),
        new InputGestureCollection() { RefreshKey }
    );
    /// <summary>
    /// Remove [Delete]
    /// </summary>
    public static KeyGesture RemoveKey => new KeyGesture(Key.Delete);
    public static readonly RoutedUICommand Remove = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.REMOVE),
        Enum.GetName(typeof(Type), Type.REMOVE),
        typeof(Commands),
        new InputGestureCollection() { RemoveKey }
    );
    /// <summary>
    /// Save [Ctrl + S]
    /// </summary>
    public static KeyGesture SaveKey => new KeyGesture(Key.S, ModifierKeys.Control);
    public static readonly RoutedUICommand Save = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.SAVE),
        Enum.GetName(typeof(Type), Type.SAVE),
        typeof(Commands),
        new InputGestureCollection() { SaveKey }
    );
    /// <summary>
    /// Select [Ctrl + Space]
    /// </summary>
    public static KeyGesture SelectKey => new KeyGesture(Key.Space, ModifierKeys.Control);
    public static readonly RoutedUICommand Select = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.SELECT),
        Enum.GetName(typeof(Type), Type.SELECT),
        typeof(Commands),
        new InputGestureCollection() { SelectKey }
    );
    /// <summary>
    /// Settings [F2]
    /// </summary>
    public static KeyGesture SettingsKey => new KeyGesture(Key.F2);
    public static readonly RoutedUICommand Settings = new RoutedUICommand
    (
        Enum.GetName(typeof(Type), Type.SETTINGS),
        Enum.GetName(typeof(Type), Type.SETTINGS),
        typeof(Commands),
        new InputGestureCollection() { SettingsKey }
    );
}
