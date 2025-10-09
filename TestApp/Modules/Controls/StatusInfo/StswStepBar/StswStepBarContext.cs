namespace TestApp;

public partial class StswStepBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
    }
    
    [StswCommand] void NextStep() => StepNumber++;
    [StswCommand] void PreviousStep() => StepNumber--;

    [StswObservableProperty] int _stepNumber = 1;
}
