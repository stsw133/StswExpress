using System;
using System.Windows;

namespace StswExpress.Styles;

partial class ContextMenu
{
    public ContextMenu()
    {
        InitializeComponent();

        AddPrivateControlStyle(typeof(Application).Assembly.GetType("System.Windows.Documents.TextEditorContextMenu+EditorContextMenu"), typeof(System.Windows.Controls.ContextMenu));
        AddPrivateControlStyle(typeof(Application).Assembly.GetType("System.Windows.Documents.TextEditorContextMenu+EditorMenuItem"), typeof(System.Windows.Controls.MenuItem));
    }
    void AddPrivateControlStyle(Type targetType, Type baseType)
    {
        var baseStyle = this[baseType] as Style;
        var style = new Style(targetType, baseStyle);
        Add(targetType, style);
    }
}
