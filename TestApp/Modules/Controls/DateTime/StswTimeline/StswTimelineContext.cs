using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestApp;
public partial class StswTimelineContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        //ApplyNaturalRange();
    }

    [StswCommand]
    private void ApplyNaturalRange()
    {
        if (Milestones.Count == 0)
        {
            Minimum = null;
            Maximum = null;
            return;
        }

        var min = Milestones.Min(x => x.Date);
        var max = Milestones.Max(x => x.Date);

        Minimum = min.AddDays(-7);
        Maximum = max.AddDays(7);
    }

    [StswCommand]
    private void ClearRange()
    {
        Minimum = null;
        Maximum = null;
    }

    public ObservableCollection<StswTimelineMilestone> Milestones { get; } =
    [
        new()
        {
            Date = new DateTime(2023, 9, 1, 9, 0, 0),
            Title = "Kick-off",
            Summary = "Initial workshop aligning stakeholders and defining success criteria.",
            Details = "Met with design, engineering and QA leads to set expectations and milestones."
        },
        new()
        {
            Date = new DateTime(2023, 10, 12, 14, 0, 0),
            Title = "Research sprint",
            Summary = "Completed customer interviews and synthesized insights.",
            Details = "Interviewed 12 participants to understand scheduling needs across time zones."
        },
        new()
        {
            Date = new DateTime(2023, 11, 6, 10, 30, 0),
            Title = "Design freeze",
            Summary = "Locked the visual language and primary interactions for the MVP.",
            Details = "Shared component guidelines with the platform and updated Figma libraries."
        },
        new()
        {
            Date = new DateTime(2024, 1, 18, 16, 0, 0),
            Title = "Beta release",
            Summary = "Shipped the private beta to our pilot customers.",
            Details = "10 customers received access with telemetry enabled for usage observations."
        },
        new()
        {
            Date = new DateTime(2024, 3, 5, 11, 0, 0),
            Title = "General availability",
            Summary = "Rolled out the feature flag globally and published documentation.",
            Details = "Adoption goals reached within the first week with positive feedback."
        }
    ];



    [StswObservableProperty] DateTime? _minimum;
    [StswObservableProperty] DateTime? _maximum;
    [StswObservableProperty] double _indicatorSize = 12d;
    [StswObservableProperty] double _itemHeight = 80d;
}

public class StswTimelineMilestone
{
    public DateTime Date { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Details { get; set; }
}
