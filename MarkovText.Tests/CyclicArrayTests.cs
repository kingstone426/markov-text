namespace MarkovText.Tests;

public class CyclicArrayTests
{
    private CyclicArray<int> cyclic = null!;

    [SetUp]
    public void Setup()
    {
        cyclic = new CyclicArray<int>(3)
        {
            [0] = 0,
            [1] = 1,
            [2] = 2,
            [3] = 3     // Overwrites [0] so the array becomes {3, 1, 2}
        };
    }

    [Test]
    public void Values_wrap_around()
    {
        Assert.That(cyclic[1], Is.EqualTo(1));
        Assert.That(cyclic[2], Is.EqualTo(2));
        Assert.That(cyclic[0], Is.EqualTo(3));
    }

    [Test]
    public void Offset_array_is_shifted_correctly()
    {
        var offset = cyclic.CreateOffsetArray(4);

        Assert.That(offset[0], Is.EqualTo(1));
        Assert.That(offset[1], Is.EqualTo(2));
        Assert.That(offset[2], Is.EqualTo(3));
    }
}
