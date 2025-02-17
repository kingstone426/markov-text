namespace MarkovText.Tests;

public class Tests
{
    private MarkovTextGenerator generator = null!;

    private class MockRandom : IRandomNumberGenerator
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

    [SetUp]
    public void Setup()
    {
        generator = new MarkovTextGenerator(File.ReadAllText(MarkovTextGenerator.DefaultCorpusPath));
    }

    [Test, Description("Mocking random number generation with 0, 1 and 2 will produce the first three sentences of the corpus. " +
                       "This is because these sentences are written in all caps and have non-branching Markov chains.")]
    public void FirstSentencesTest()
    {
        Assert.That(generator.GenerateMarkov(new MockRandom(0)), Is.EqualTo("THE CORSET AND THE CRINOLINE."));
        Assert.That(generator.GenerateMarkov(new MockRandom(1)), Is.EqualTo("A BOOK OF MODES AND COSTUMES FROM REMOTE PERIODS TO THE PRESENT TIME."));
        Assert.That(generator.GenerateMarkov(new MockRandom(2)), Is.EqualTo("WITH 54 FULL-PAGE AND OTHER ENGRAVINGS."));
    }

    [Test, Description("Mocking random number generation with 173 produces a weird sentence that is created by pseudo-random jumps through the corpus.")]
    public void PseudoRandomSentenceTest()
    {
        Assert.That(generator.GenerateMarkov(new MockRandom(173)), Is.EqualTo("We learn from the throat downwards by silver plates."));
    }
}