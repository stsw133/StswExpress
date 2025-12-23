namespace TestApp.Wpf;
public partial class StswCalculatorContext : ControlsContext
{
    [StswCommand] void ClearExpression() => ExpressionText = string.Empty;
    
    [StswObservableProperty] string _displayText = "0";
    [StswObservableProperty] string _expressionText = string.Empty;
}
