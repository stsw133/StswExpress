using System;
using System.Windows.Media;

namespace TestApp;

public partial class ContractorModel : StswObservableObject, IStswCollectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] ContractorType _type;

    //[StswIgnoreAutoGenerateColumn]
    //public byte[]? Icon
    //{
    //    get => _icon;
    //    set => SetProperty(ref _icon, value, );
    //}
    [StswObservableProperty(CallbackMethod = "() => IconSource ??= StswFnUI.BytesToBitmapImage(value)")] byte[]? _icon;

    //[JsonIgnore]
    //[StswIgnoreAutoGenerateColumn]
    //public ImageSource? IconSource
    //{
    //    get => _iconSource;
    //    set => SetProperty(ref _iconSource, value, () => Icon = value?.ToBytes());
    //}
    [StswObservableProperty(CallbackMethod = "() => Icon = value?.ToBytes()")] ImageSource? _iconSource;

    [StswObservableProperty] string? _name;

    //[StswIgnoreAutoGenerateColumn]
    [StswObservableProperty] AddressModel _address = new();
    [StswObservableProperty] decimal _defaultDiscount;
    [StswObservableProperty] bool _isArchival;
    [StswObservableProperty] DateTime _createDT = DateTime.Now;
    
    /// ItemState
    //[StswIgnoreAutoGenerateColumn]
    [StswObservableProperty] StswItemState _itemState;

    /// ShowDetails
    //[StswIgnoreAutoGenerateColumn]
    [StswObservableProperty] bool? _showDetails = false;
}
