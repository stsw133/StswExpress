using System;
using System.Windows.Input;

namespace StswExpress.Globals
{
	public static class Commands
	{
		public enum Type
		{
			NONE,
			ADD,
			ARCHIVE,
			CLEAR,
			CLOSE,
			DELETE,
			DUPLICATE,
			EDIT,
			FIND,
			HELP,
			LIST,
			NEW,
			PREVIEW,
			PRINT,
			REFRESH,
			REMOVE,
			SAVE,
			SELECT
		}

		/// <summary>
		/// Add
		/// </summary>
		public static readonly RoutedUICommand Add = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.ADD),
			Enum.GetName(typeof(Type), Type.ADD),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.OemPlus, ModifierKeys.Control)
			}
		);
		/// <summary>
		/// Archive
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
		/// Clear
		/// </summary>
		public static readonly RoutedUICommand Clear = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.CLEAR),
			Enum.GetName(typeof(Type), Type.CLEAR),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F5, ModifierKeys.Control)
			}
		);
		/// <summary>
		/// Close
		/// </summary>
		public static readonly RoutedUICommand Close = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.CLOSE),
			Enum.GetName(typeof(Type), Type.CLOSE),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F4, ModifierKeys.Alt)
			}
		);
		/// <summary>
		/// Delete
		/// </summary>
		public static readonly RoutedUICommand Delete = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.DELETE),
			Enum.GetName(typeof(Type), Type.DELETE),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Delete)
			}
		);
		/// <summary>
		/// Duplicate
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
		/// Edit
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
		/// Find
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
		/// Help
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
		/// List
		/// </summary>
		public static readonly RoutedUICommand List = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.LIST),
			Enum.GetName(typeof(Type), Type.LIST),
			typeof(Commands)
		);
		/// <summary>
		/// New
		/// </summary>
		public static readonly RoutedUICommand New = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.NEW),
			Enum.GetName(typeof(Type), Type.NEW),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Insert)
			}
		);
		/// <summary>
		/// Preview
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
		/// Print
		/// </summary>
		public static readonly RoutedUICommand Print = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.PRINT),
			Enum.GetName(typeof(Type), Type.PRINT),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.P, ModifierKeys.Alt)
			}
		);
		/// <summary>
		/// Refresh
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
		/// Remove
		/// </summary>
		public static readonly RoutedUICommand Remove = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.REMOVE),
			Enum.GetName(typeof(Type), Type.REMOVE),
			typeof(Commands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.OemMinus, ModifierKeys.Control)
			}
		);
		/// <summary>
		/// Save
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
		/// Select
		/// </summary>
		public static readonly RoutedUICommand Select = new RoutedUICommand
		(
			Enum.GetName(typeof(Type), Type.SELECT),
			Enum.GetName(typeof(Type), Type.SELECT),
			typeof(Commands)
		);
	}
}
