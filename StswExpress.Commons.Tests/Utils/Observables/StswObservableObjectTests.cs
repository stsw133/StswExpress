namespace StswExpress.Commons.Tests.Utils.Observables;
public class StswObservableObjectTests
{
    private class TestObservableObject : StswObservableObject
    {
        private int _number;
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private string? _text;
        public string? Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }

    [Fact]
    public void SetProperty_ChangesValue_RaisesPropertyChanged()
    {
        var obj = new TestObservableObject();
        string? changedProp = null;
        obj.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        obj.Number = 42;

        Assert.Equal(42, obj.Number);
        Assert.Equal(nameof(TestObservableObject.Number), changedProp);
    }

    [Fact]
    public void SetProperty_SameValue_DoesNotRaisePropertyChanged()
    {
        var obj = new TestObservableObject();
        obj.Number = 10;
        bool eventRaised = false;
        obj.PropertyChanged += (s, e) => eventRaised = true;

        obj.Number = 10;

        Assert.False(eventRaised);
    }

    [Fact]
    public void SetProperty_ReferenceType_ChangesValue_RaisesPropertyChanged()
    {
        var obj = new TestObservableObject();
        string? changedProp = null;
        obj.PropertyChanged += (s, e) => changedProp = e.PropertyName;

        obj.Text = "Hello";

        Assert.Equal("Hello", obj.Text);
        Assert.Equal(nameof(TestObservableObject.Text), changedProp);
    }

    //[Fact]
    //public void OnPropertyChanged_RaisesPropertyChanged_WithCustomName()
    //{
    //    var obj = new TestObservableObject();
    //    string? changedProp = null;
    //    obj.PropertyChanged += (s, e) => changedProp = e.PropertyName;
    //
    //    obj.OnPropertyChanged("CustomProperty");
    //
    //    Assert.Equal("CustomProperty", changedProp);
    //}
}
