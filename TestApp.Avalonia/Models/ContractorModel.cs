using Avalonia.Media.Imaging;

namespace TestApp.Ava;
public partial class ContractorModel : StswObservableObject, IStswDetailedItem, IStswTrackableItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] ContractorType _type;
    [StswObservableProperty] string? _name;
    [StswObservableProperty] AddressModel _address = new();
    [StswObservableProperty] decimal _defaultDiscount;
    [StswObservableProperty] bool _isArchival;
    [StswObservableProperty] DateTime _createDT = DateTime.Now;
    [StswObservableProperty] StswItemState _itemState;
    [StswObservableProperty] bool? _showDetails = false;

    [StswObservableProperty] byte[]? _icon;
    //partial void OnIconChanged(byte[]? oldValue, byte[]? newValue) => IconSource ??= StswFnUI.BytesToBitmapImage(newValue);
    //public int IconByteSize => Icon?.Length ?? 0;

    //[StswObservableProperty] Bitmap? _iconSource;
    //partial void OnIconSourceChanged(Bitmap? oldValue, Bitmap? newValue) => Icon = newValue?.ToBytes();
}
