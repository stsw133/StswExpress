using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;

public class StswLabelContext : ControlsContext
{
    public StswAsyncCommand<StswProgressState> ProcessCommand { get; }
    public StswCommand SetGridLengthAutoCommand => new(() => IconScale = GridLength.Auto);
    public StswCommand SetGridLengthFillCommand => new(() => IconScale = new GridLength(1, GridUnitType.Star));

    public StswLabelContext()
    {
        ProcessCommand = new(Process) { IsReusable = true };
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task Process(StswProgressState state)
    {
        await Task.Run(() =>
        {
            if (ProcessCommand.State.In(StswProgressState.Error, StswProgressState.Finished) && state == StswProgressState.Running)
                ProcessCommand.Value = ProcessCommand.Minimum;

            if (!(state.In(StswProgressState.Paused, StswProgressState.Error) && ProcessCommand.State.In(StswProgressState.Ready, StswProgressState.Finished)))
                ProcessCommand.State = state;

            if (state == StswProgressState.Running)
                for (var i = ProcessCommand.Value; i < ProcessCommand.Maximum; i++)
                {
                    Thread.Sleep(20);
                    if (ProcessCommand.State == StswProgressState.Running)
                    {
                        ProcessCommand.Value += 1;

                        if (ProcessCommand.Value == ProcessCommand.Maximum)
                            ProcessCommand.State = StswProgressState.Finished;
                    }
                    else break;
                }
        });
    }

    /// IconScale
    public GridLength IconScale
    {
        get => _iconScale;
        set => SetProperty(ref _iconScale, value);
    }
    private GridLength _iconScale;

    /// IsBusy
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    private bool _isBusy;

    /// IsContentVisible
    public bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetProperty(ref _isContentVisible, value);
    }
    private bool _isContentVisible;

    /// Orientation
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
    private Orientation _orientation;
}
