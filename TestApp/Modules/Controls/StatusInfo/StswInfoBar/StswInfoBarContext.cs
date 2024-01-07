using System;
using System.Linq;

namespace TestApp;

public class StswInfoBarContext : ControlsContext
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        IsClosable = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsClosable)))?.Value ?? default;
        Type = (StswInfoType?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(Type)))?.Value ?? default;
    }

    /// Description
    private string description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta.";
    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    /// IsClosable
    private bool isClosable;
    public bool IsClosable
    {
        get => isClosable;
        set => SetProperty(ref isClosable, value);
    }

    /// Title
    private string title = DateTime.Now.ToString();
    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    /// Type
    private StswInfoType type;
    public StswInfoType Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
}
