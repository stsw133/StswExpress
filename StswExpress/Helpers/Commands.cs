using System;
using System.Windows.Input;

namespace StswExpress
{
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
        public static readonly RoutedUICommand Add = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.ADD),
            Enum.GetName(typeof(Type), Type.ADD),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Insert)
            }
        );
        /// <summary>
        /// Archive [Ctrl + I]
        /// </summary>
        public static readonly RoutedUICommand Archive = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.ARCHIVE),
            Enum.GetName(typeof(Type), Type.ARCHIVE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.I, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Clear [F9]
        /// </summary>
        public static readonly RoutedUICommand Clear = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.CLEAR),
            Enum.GetName(typeof(Type), Type.CLEAR),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F9)
            }
        );
        /// <summary>
        /// Decrease [-]
        /// </summary>
        public static readonly RoutedUICommand Decrease = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.DECREASE),
            Enum.GetName(typeof(Type), Type.DECREASE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.OemMinus)
            }
        );
        /// <summary>
        /// Delete [Ctrl + D]
        /// </summary>
        public static readonly RoutedUICommand Delete = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.DELETE),
            Enum.GetName(typeof(Type), Type.DELETE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.D, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Duplicate [Ctrl + Insert]
        /// </summary>
        public static readonly RoutedUICommand Duplicate = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.DUPLICATE),
            Enum.GetName(typeof(Type), Type.DUPLICATE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Insert, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Edit [Ctrl + E]
        /// </summary>
        public static readonly RoutedUICommand Edit = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.EDIT),
            Enum.GetName(typeof(Type), Type.EDIT),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.E, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Find [Ctrl + F]
        /// </summary>
        public static readonly RoutedUICommand Find = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.FIND),
            Enum.GetName(typeof(Type), Type.FIND),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Fullscreen [F11]
        /// </summary>
        public static readonly RoutedUICommand Fullscreen = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.FULLSCREEN),
            Enum.GetName(typeof(Type), Type.FULLSCREEN),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F11)
            }
        );
        /// <summary>
        /// Help [F1]
        /// </summary>
        public static readonly RoutedUICommand Help = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.HELP),
            Enum.GetName(typeof(Type), Type.HELP),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F1)
            }
        );
        /// <summary>
        /// Increase [+]
        /// </summary>
        public static readonly RoutedUICommand Increase = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.INCREASE),
            Enum.GetName(typeof(Type), Type.INCREASE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.OemPlus)
            }
        );
        /// <summary>
        /// List [F3]
        /// </summary>
        public static readonly RoutedUICommand List = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.LIST),
            Enum.GetName(typeof(Type), Type.LIST),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F3)
            }
        );
        /// <summary>
        /// New [Ctrl + N]
        /// </summary>
        public static readonly RoutedUICommand New = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.NEW),
            Enum.GetName(typeof(Type), Type.NEW),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.N, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Preview [Ctrl + P]
        /// </summary>
        public static readonly RoutedUICommand Preview = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.PREVIEW),
            Enum.GetName(typeof(Type), Type.PREVIEW),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.P, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Print [F4]
        /// </summary>
        public static readonly RoutedUICommand Print = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.PRINT),
            Enum.GetName(typeof(Type), Type.PRINT),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F4)
            }
        );
        /// <summary>
        /// Refresh [F5]
        /// </summary>
        public static readonly RoutedUICommand Refresh = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.REFRESH),
            Enum.GetName(typeof(Type), Type.REFRESH),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F5)
            }
        );
        /// <summary>
        /// Remove [Delete]
        /// </summary>
        public static readonly RoutedUICommand Remove = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.REMOVE),
            Enum.GetName(typeof(Type), Type.REMOVE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Delete)
            }
        );
        /// <summary>
        /// Save [Ctrl + S]
        /// </summary>
        public static readonly RoutedUICommand Save = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.SAVE),
            Enum.GetName(typeof(Type), Type.SAVE),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Select [Ctrl + Space]
        /// </summary>
        public static readonly RoutedUICommand Select = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.SELECT),
            Enum.GetName(typeof(Type), Type.SELECT),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Space, ModifierKeys.Control)
            }
        );
        /// <summary>
        /// Settings [F2]
        /// </summary>
        public static readonly RoutedUICommand Settings = new RoutedUICommand
        (
            Enum.GetName(typeof(Type), Type.SETTINGS),
            Enum.GetName(typeof(Type), Type.SETTINGS),
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F2)
            }
        );
    }
}
