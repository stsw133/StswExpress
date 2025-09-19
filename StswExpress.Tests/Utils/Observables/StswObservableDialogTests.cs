namespace StswExpress.Commons.Tests.Utils.Observables;
public class StswObservableDialogTests
{
    private class TestDialog : StswExpress.Commons.StswObservableDialog
    {
        private string? _testProperty;
        public string? TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }
    }

    [Fact]
    public void DialogIdentifier_SetAndGet_ReturnsExpectedValue()
    {
        var dialog = new TestDialog();
        dialog.DialogIdentifier = "Dialog123";
        Assert.Equal("Dialog123", dialog.DialogIdentifier);
    }

    [Fact]
    public void Dispose_SuppressesFinalization()
    {
        var dialog = new TestDialog();
        // Dispose should not throw
        dialog.Dispose();
        GC.SuppressFinalize(dialog); // Should not throw, but no way to assert suppression directly
    }

    [Fact]
    public void SetProperty_UpdatesValueAndRaisesPropertyChanged()
    {
        var dialog = new TestDialog();
        bool propertyChangedRaised = false;
        dialog.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TestDialog.TestProperty))
                propertyChangedRaised = true;
        };

        dialog.TestProperty = "new value";
        Assert.Equal("new value", dialog.TestProperty);
        Assert.True(propertyChangedRaised);
    }
}
