using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a control that can be used to display or edit unformatted text.
/// </summary>
[ContentProperty(nameof(Text))]
public class StswTextBox : StswBoxBase
{
    static StswTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextBox), new FrameworkPropertyMetadata(typeof(StswTextBox)));
    }

    #region Events & methods
    /// <summary>
    /// Updates the main property associated with the text in the control based on user input.
    /// </summary>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        if (alwaysUpdate)
        {
            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active, BindingStatus.UpdateSourceError))
                bindingExpression.UpdateSource();
        }
    }
    #endregion
}
