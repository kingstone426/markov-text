namespace MarkovText.Tests;

public class RandomStub : IRandomNumberGenerator
{
    private readonly int value;
    public RandomStub(int value)
    {
        this.value = value;
    }
    public int Next(int maxValue)
    {
        return value % maxValue;
    }
}