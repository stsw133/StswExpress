using System.ComponentModel;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswBindingTriggerTests
{
    [Fact]
    public void Constructor_CreatesBindingWithCorrectSourceAndPath()
    {
        // Arrange & Act
        var trigger = new StswBindingTrigger();

        // Assert
        Assert.NotNull(trigger.Binding);
        Assert.Equal(trigger, trigger.Binding.Source);
        Assert.Equal("Value", trigger.Binding.Path.Path);
    }

    [Fact]
    public void ValueProperty_DefaultValue_IsNull()
    {
        // Arrange
        var trigger = new StswBindingTrigger();

        // Act & Assert
        Assert.Null(trigger.Value);
    }

    [Fact]
    public void Refresh_RaisesPropertyChangedEventForValue()
    {
        // Arrange
        var trigger = new StswBindingTrigger();
        string? propertyName = null;
        trigger.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        // Act
        trigger.Refresh();

        // Assert
        Assert.Equal("Value", propertyName);
    }

    [Fact]
    public void PropertyChanged_Event_IsInvoked()
    {
        // Arrange
        var trigger = new StswBindingTrigger();
        bool eventRaised = false;
        trigger.PropertyChanged += (s, e) => eventRaised = true;

        // Act
        trigger.Refresh();

        // Assert
        Assert.True(eventRaised);
    }
}
