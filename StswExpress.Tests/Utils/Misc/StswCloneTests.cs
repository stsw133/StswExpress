namespace StswExpress.Commons.Tests;
public class StswCloneTests
{
    private class SimpleClass
    {
        public int IntValue;
        public string StringValue;
    }

    private class NestedClass
    {
        public SimpleClass Inner;
        public int[] Numbers;
    }

    private class CircularClass
    {
        public CircularClass? Reference;
        public int Value;
    }

    private class BaseClass
    {
        private int _privateBaseValue = 42;
        public int GetPrivateBaseValue() => _privateBaseValue;
    }

    private class DerivedClass : BaseClass
    {
        public string Name = "Derived";
    }

    [Fact]
    public void DeepCopy_PrimitiveType_ReturnsSameValue()
    {
        int original = 123;
        var copy = original.DeepCopy();
        Assert.Equal(original, copy);
    }

    [Fact]
    public void DeepCopy_String_ReturnsSameValue()
    {
        string original = "Hello";
        var copy = original.DeepCopy();
        Assert.Equal(original, copy);
    }

    [Fact]
    public void DeepCopy_SimpleClass_CreatesDistinctCopy()
    {
        var original = new SimpleClass { IntValue = 5, StringValue = "Test" };
        var copy = original.DeepCopy();

        Assert.NotSame(original, copy);
        Assert.Equal(original.IntValue, ((SimpleClass)copy).IntValue);
        Assert.Equal(original.StringValue, ((SimpleClass)copy).StringValue);
    }

    [Fact]
    public void DeepCopy_NestedClass_CopiesInnerObjects()
    {
        var original = new NestedClass
        {
            Inner = new SimpleClass { IntValue = 7, StringValue = "Inner" },
            Numbers = new[] { 1, 2, 3 }
        };
        var copy = original.DeepCopy();

        Assert.NotSame(original, copy);
        var copied = (NestedClass)copy;
        Assert.NotSame(original.Inner, copied.Inner);
        Assert.Equal(original.Inner.IntValue, copied.Inner.IntValue);
        Assert.Equal(original.Inner.StringValue, copied.Inner.StringValue);
        Assert.NotSame(original.Numbers, copied.Numbers);
        Assert.Equal(original.Numbers, copied.Numbers);
    }

    [Fact]
    public void DeepCopy_ArrayOfObjects_CopiesElements()
    {
        var original = new[] { new SimpleClass { IntValue = 1 }, new SimpleClass { IntValue = 2 } };
        var copy = original.DeepCopy();

        Assert.NotSame(original, copy);
        var copiedArray = (SimpleClass[])copy;
        Assert.Equal(original.Length, copiedArray.Length);
        for (int i = 0; i < original.Length; i++)
        {
            Assert.NotSame(original[i], copiedArray[i]);
            Assert.Equal(original[i].IntValue, copiedArray[i].IntValue);
        }
    }

    [Fact]
    public void DeepCopy_MultidimensionalArray_CopiesElements()
    {
        var original = new int[2, 2] { { 1, 2 }, { 3, 4 } };
        var copy = original.DeepCopy();

        Assert.NotSame(original, copy);
        var copiedArray = (int[,])copy;
        Assert.Equal(original, copiedArray);
    }

    [Fact]
    public void DeepCopy_CircularReference_HandlesCorrectly()
    {
        var original = new CircularClass { Value = 10 };
        original.Reference = original;

        var copy = original.DeepCopy();
        var copied = (CircularClass)copy;

        Assert.NotSame(original, copied);
        Assert.Equal(original.Value, copied.Value);
        Assert.Same(copied, copied.Reference);
    }

    [Fact]
    public void DeepCopy_DelegateField_ReturnsNull()
    {
        var original = new Action(() => { });
        var copy = original.DeepCopy();
        Assert.Null(copy);
    }

    [Fact]
    public void DeepCopy_BasePrivateFields_AreCopied()
    {
        var original = new DerivedClass();
        var copy = original.DeepCopy();

        Assert.NotSame(original, copy);
        var copied = (DerivedClass)copy;
        Assert.Equal(original.Name, copied.Name);
        Assert.Equal(original.GetPrivateBaseValue(), copied.GetPrivateBaseValue());
    }

    [Fact]
    public void DeepCopy_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => StswClone.DeepCopy<object>(null!));
        Assert.Throws<ArgumentNullException>(() => StswClone.DeepCopy(null!));
    }
}
