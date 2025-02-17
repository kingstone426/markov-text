namespace MarkovText;

/// <summary>
/// The System.Random class implementation is abstracted through IRandomNumberGenerator to allow mocking during unit tests.
/// </summary>
public class DefaultRandom : IRandomNumberGenerator
{
    private readonly Random random;

    public DefaultRandom(Random random)
    {
        this.random = random;
    }

    public int Next(int maxValue)
    {
        return random.Next(maxValue);
    }
}