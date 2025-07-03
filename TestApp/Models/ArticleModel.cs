using System;
using System.Windows.Media;

namespace TestApp;

public partial class ArticleModel : StswObservableObject, IStswCollectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] ArticleType _type;

    //public byte[]? Icon
    //{
    //    get => _icon;
    //    set => SetProperty(ref _icon, value, () => IconSource ??= StswFnUI.BytesToBitmapImage(value));
    //}
    [StswObservableProperty(CallbackMethod = "() => IconSource ??= StswFnUI.BytesToBitmapImage(value)")] byte[]? _icon;

    //public ImageSource? IconSource
    //{
    //    get => _iconSource;
    //    set => SetProperty(ref _iconSource, value, () => Icon = value?.ToBytes());
    //}
    [StswObservableProperty(CallbackMethod = "() => Icon = value?.ToBytes()")] ImageSource? _iconSource;

    [StswObservableProperty] string? _name;
    [StswObservableProperty] string? _ean;
    [StswObservableProperty] string? _uom;
    [StswObservableProperty] decimal _weight;
    [StswObservableProperty] string? _weightUoM;
    [StswObservableProperty] decimal _grossWeight;
    [StswObservableProperty] string? _grossWeightUoM;
    [StswObservableProperty] DiscountType _discountType;
    [StswObservableProperty] int _producerID;
    [StswObservableProperty] bool _isArchival;
    [StswObservableProperty] int _creatorID;
    [StswObservableProperty] DateTime _createDT = DateTime.Now;
    [StswObservableProperty] StswItemState _itemState;
    [StswObservableProperty] bool? _showDetails = false;
}
