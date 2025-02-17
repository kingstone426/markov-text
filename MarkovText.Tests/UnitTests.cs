namespace MarkovText.Tests;

public class Tests
{
    [Test, Description("Mocking random number generation with 0, 1 and 2 will produce the first three sentences of the corpus. " +
                       "This is because these sentences are written in all caps and have non-branching Markov chains.")]
    public void FirstSentencesTest()
    {
        var generator = new MarkovTextGenerator(File.ReadAllText(MarkovTextGenerator.DefaultCorpusPath));
        Assert.That(generator.GenerateMarkov(new MockRandom(0)), Is.EqualTo("THE CORSET AND THE CRINOLINE."));
        Assert.That(generator.GenerateMarkov(new MockRandom(1)), Is.EqualTo("A BOOK OF MODES AND COSTUMES FROM REMOTE PERIODS TO THE PRESENT TIME."));
        Assert.That(generator.GenerateMarkov(new MockRandom(2)), Is.EqualTo("WITH 54 FULL-PAGE AND OTHER ENGRAVINGS."));
    }

    [Test, Description("Mocking random number generation with 173 produces a weird sentence that is created by pseudo-random jumps through the corpus.")]
    public void PseudoRandomSentenceTest()
    {
        var generator = new MarkovTextGenerator(File.ReadAllText(MarkovTextGenerator.DefaultCorpusPath));
        Assert.That(generator.GenerateMarkov(new MockRandom(173)), Is.EqualTo("We learn from the throat downwards by silver plates."));
    }

    [Test, Description("Mocking random number generation with 1 so that the secondary alternative for 'dog was' gets selected, proceeding directly to 'very sad.'")]
    public void DeterministicTest()
    {
        var generator = new MarkovTextGenerator("The big dog was happy but the small dog was very sad.");
        Assert.That(generator.GenerateMarkov(new MockRandom(1)), Is.EqualTo("The big dog was very sad."));
    }
}