using System.Data;

namespace StswExpress;

public abstract class BaseM
{
    public DataRowState ItemState { get; internal set; }
}
