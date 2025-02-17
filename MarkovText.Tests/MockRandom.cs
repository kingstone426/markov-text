namespace MarkovText.Tests;

public class MockRandom : IRandomNumberGenerator
{
    private readonly int value;
    public MockRandom(int value)
    {
        this.value = value;
    }
    public int Next(int maxValue)
    {
        return value % maxValue;
    }
}