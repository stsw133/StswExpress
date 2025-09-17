using System.ComponentModel.DataAnnotations;

namespace StswExpress.Commons.Tests;
public class StswObservableValidatorTests
{
    public class TestViewModel : StswExpress.Commons.StswObservableValidator
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        public string? Name
        {
            get => _name;
            set => SetValidatedProperty(ref _name, value);
        }
        internal string? _name; // Changed from private to internal

        [Range(1, 10, ErrorMessage = "Value must be between 1 and 10.")]
        public int Value
        {
            get => _value;
            set => SetValidatedProperty(ref _value, value);
        }
        internal int _value; // Changed from private to internal

        // Expose a public method to access the protected SetValidatedProperty method
        public bool SetValidatedPropertyWrapper<T>(ref T field, T value, string propertyName)
        {
            return SetValidatedProperty(ref field, value, propertyName);
        }
    }

    [Fact]
    public void HasErrors_False_WhenNoValidationErrors()
    {
        var vm = new TestViewModel
        {
            Name = "John",
            Value = 5
        };
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public void HasErrors_True_WhenValidationErrorExists()
    {
        var vm = new TestViewModel
        {
            Name = "",
            Value = 5
        };
        Assert.True(vm.HasErrors);
        var errors = vm.GetErrors(nameof(vm.Name)).Cast<string>().ToList();
        Assert.Contains("Name is required.", errors);
    }

    [Fact]
    public void GetErrors_ReturnsAllErrors_WhenPropertyNameIsNull()
    {
        var vm = new TestViewModel
        {
            Name = "",
            Value = 20
        };
        var errors = vm.GetErrors(null).Cast<string>().ToList();
        Assert.Contains("Name is required.", errors);
        Assert.Contains("Value must be between 1 and 10.", errors);
    }

    [Fact]
    public void ValidateAllProperties_UpdatesErrors()
    {
        var vm = new TestViewModel
        {
            Name = "",
            Value = 20
        };
        vm.ValidateAllProperties();
        Assert.True(vm.HasErrors);
        var nameErrors = vm.GetErrors(nameof(vm.Name)).Cast<string>().ToList();
        var valueErrors = vm.GetErrors(nameof(vm.Value)).Cast<string>().ToList();
        Assert.Contains("Name is required.", nameErrors);
        Assert.Contains("Value must be between 1 and 10.", valueErrors);
    }

    [Fact]
    public void ValidateProperty_RemovesOldErrors()
    {
        var vm = new TestViewModel
        {
            Name = "",
            Value = 5
        };
        Assert.True(vm.HasErrors);
        vm.Name = "Jane";
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public void SetValidatedProperty_ReturnsTrue_WhenValueChanges()
    {
        var vm = new TestViewModel();
        var result = vm.SetValidatedPropertyWrapper(ref vm._name, "Test", nameof(vm.Name));
        Assert.True(result);
    }

    [Fact]
    public void SetValidatedProperty_ReturnsFalse_WhenValueDoesNotChange()
    {
        var vm = new TestViewModel();
        vm.Name = "Test";
        var result = vm.SetValidatedPropertyWrapper(ref vm._name, "Test", nameof(vm.Name));
        Assert.False(result);
    }

    [Fact]
    public void ErrorsChanged_Event_IsRaised()
    {
        var vm = new TestViewModel();
        string? changedProperty = null;
        vm.ErrorsChanged += (s, e) => changedProperty = e.PropertyName;
        vm.Name = "";
        Assert.Equal(nameof(vm.Name), changedProperty);
    }
}
