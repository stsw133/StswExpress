using System;
using System.Windows.Media;

namespace TestApp;
public partial class ArticleModel : StswObservableObject, IStswCollectionItem
{
    [StswObservableProperty] int _id;
    [StswObservableProperty] ArticleType _type;
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

    [StswObservableProperty] byte[]? _icon;
    partial void OnIconChanged(byte[]? oldValue, byte[]? newValue) => IconSource ??= StswFnUI.BytesToBitmapImage(newValue);

    [StswObservableProperty] ImageSource? _iconSource;
    partial void OnIconSourceChanged(ImageSource? oldValue, ImageSource? newValue) => Icon = newValue?.ToBytes();

}
