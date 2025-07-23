namespace TestApp;

public class StswStepBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
    }

    public StswCommand NextStepCommand { get; set; }
    public StswCommand PreviousStepCommand { get; set; }

    public StswStepBarContext()
    {
        NextStepCommand = new(() => StepNumber++);
        PreviousStepCommand = new(() => StepNumber--);
    }

    private int _stepNumber = 1;
    public int StepNumber
    {
        get => _stepNumber;
        set { SetProperty(ref _stepNumber, value); }
    }
}
