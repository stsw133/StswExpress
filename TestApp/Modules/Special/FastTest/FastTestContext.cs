using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp;
public partial class FastTestContext : StswObservableObject
{
    [StswCommand]
    async Task Test(CancellationToken token)
    {
        try
        {
            Logs.Add(new(StswInfoType.Information, "Processing..."));
            TestCommand.Value = 0;

            for (var i = 0; i < 100; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Logs.Add(new(StswInfoType.Information, "Operation cancelled."));
                    return;
                }

                Logs.Add(new(StswInfoType.Information, (i + 1).ToString()));
                TestCommand.Value = i;
                await Task.Delay(50, token);
            }

            Logs.Add(new(StswInfoType.Success, "Finished!"));
        }
        catch (OperationCanceledException)
        {
            Logs.Add(new(StswInfoType.Information, "Operation cancelled."));
        }
        catch (Exception ex)
        {
            Logs.Add(new(StswInfoType.Error, "Error occured!"));
            await StswMessageDialog.Show(ex, $"Method error: {MethodBase.GetCurrentMethod()?.Name}");
        }
    }

    [StswObservableProperty] ObservableCollection<StswLogItem> _logs = [];
}
