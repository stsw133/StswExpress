using System;
using System.Windows.Media;

namespace TestApp;
public partial class ContractorModel : StswObservableObject, IStswCollectionItem
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
    partial void OnIconChanged(byte[]? oldValue, byte[]? newValue) => IconSource ??= StswFnUI.BytesToBitmapImage(newValue);

    [StswObservableProperty] ImageSource? _iconSource;
    partial void OnIconSourceChanged(ImageSource? oldValue, ImageSource? newValue) => Icon = newValue?.ToBytes();
}
