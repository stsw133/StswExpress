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
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    private string _description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta.";

    /// IsClosable
    public bool IsClosable
    {
        get => _isClosable;
        set => SetProperty(ref _isClosable, value);
    }
    private bool _isClosable;

    /// Title
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    private string _title = DateTime.Now.ToString();

    /// Type
    public StswInfoType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private StswInfoType _type;
}
