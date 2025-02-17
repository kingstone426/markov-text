namespace MarkovText.Tests;

public class Tests
{
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

    [Test, Description("Mocking random number generation with 0, 1 and 2 will produce the first three sentences of the corpus. " +
                       "This is because these sentences are written in all caps and have non-branching Markov chains.")]
    public void FirstSentencesTest()
    {
        Assert.That(new MarkovTextGenerator(new MockRandom(0), MarkovTextGenerator.DefaultCorpusPath, 2).GenerateMarkov(), Is.EqualTo("THE CORSET AND THE CRINOLINE."));
        Assert.That(new MarkovTextGenerator(new MockRandom(1), MarkovTextGenerator.DefaultCorpusPath, 2).GenerateMarkov(), Is.EqualTo("A BOOK OF MODES AND COSTUMES FROM REMOTE PERIODS TO THE PRESENT TIME."));
        Assert.That(new MarkovTextGenerator(new MockRandom(2), MarkovTextGenerator.DefaultCorpusPath, 2).GenerateMarkov(), Is.EqualTo("WITH 54 FULL-PAGE AND OTHER ENGRAVINGS."));
    }

    [Test, Description("Mocking random number generation with 173 produces a weird sentence that is created by pseudo-random jumps through the corpus.")]
    public void PseudoRandomSentenceTest()
    {
        Assert.That(new MarkovTextGenerator(new MockRandom(173), MarkovTextGenerator.DefaultCorpusPath, 2).GenerateMarkov(), Is.EqualTo("We learn from the throat downwards by silver plates."));
    }
}