using System;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
internal class StswEventToCommandExtension : MarkupExtension
{
    //TODO - gimme some code

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new();
    }
}
