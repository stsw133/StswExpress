using System.Data;

namespace StswExpress;

public abstract class BaseModel
{
    public DataRowState ItemState { get; internal set; }
}
