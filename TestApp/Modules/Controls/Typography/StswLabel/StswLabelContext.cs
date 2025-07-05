using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestApp;
public partial class StswLabelContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IconScale = (GridLength?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IconScale)))?.Value ?? default;
        IsBusy = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsBusy)))?.Value ?? default;
        IsContentVisible = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsContentVisible)))?.Value ?? default;
        Orientation = (Orientation?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Orientation)))?.Value ?? default;
        TextTrimming = (TextTrimming?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(TextTrimming)))?.Value ?? default;
    }
    
    [StswCommand] void SetGridLengthAuto() => IconScale = GridLength.Auto;
    [StswCommand] void SetGridLengthFill() => IconScale = new GridLength(1, GridUnitType.Star);
    [StswAsyncCommand(IsReusable = true)] async Task Process(StswProgressState state)
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

    [StswObservableProperty] GridLength _iconScale;
    [StswObservableProperty] bool _isBusy;
    [StswObservableProperty] bool _isContentVisible;
    [StswObservableProperty] Orientation _orientation;
    [StswObservableProperty] TextTrimming _textTrimming;
}
