namespace StswExpress;

/// <summary>
/// Data model for <see cref="StswRatingControl"/>'s items.
/// </summary>
internal class StswRatingItem : StswObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the item is checked.
    /// </summary>
    public bool IsChecked
    {
        get => _isChecked;
        internal set => SetProperty(ref _isChecked, value);
    }
    private bool _isChecked;

    /// <summary>
    /// Gets or sets a value indicating whether the mouse is over the item.
    /// </summary>
    public bool IsMouseOver
    {
        get => _isMouseOver;
        internal set => SetProperty(ref _isMouseOver, value);
    }
    private bool _isMouseOver;

    /// <summary>
    /// Gets or sets the value associated with the item.
    /// </summary>
    public int Value
    {
        get => _value;
        internal set => SetProperty(ref _value, value);
    }
    private int _value;
}
