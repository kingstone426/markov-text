namespace MarkovText.Tests;

public class MarkovTextGenerator
{
    [TestCaseSource(nameof(Generators))]
    [Description("Generates the first three sentences of the corpus.")]
    public void First_three_sentences(IGenerator generator)
    {
        const string corpus = "The first sentence. The second sentence. The third sentence.";

        generator.BuildMarkovModel(corpus);

        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("The first sentence."));
        Assert.That(generator.GenerateSentence(new RandomStub(1)), Is.EqualTo("The second sentence."));
        Assert.That(generator.GenerateSentence(new RandomStub(2)), Is.EqualTo("The third sentence."));
    }

    [TestCaseSource(nameof(Generators))]
    [Description("Selects the second transition alternative 'dog was' -> 'very sad.' ")]
    public void Branching_sentence(IGenerator generator)
    {
        const string corpus = "The big dog was happy but the small dog was very sad.";

        generator.BuildMarkovModel(corpus);

        Assert.That(generator.GenerateSentence(new RandomStub(1)), Is.EqualTo("The big dog was very sad."));
    }

    [TestCaseSource(nameof(Generators))]
    [Description("Generates an infinite sentence 'The big dog was happy but the small dog was happy but the small dog was happy but...'")]
    public void Infinitely_branching_sentence(IGenerator generator)
    {
        const string corpus = "The big dog was happy but the small dog was very sad.";

        generator.BuildMarkovModel(corpus);

        Assert.Throws<SentenceOverflowException>(() => generator.GenerateSentence(new RandomStub(0)));
    }

    [TestCaseSource(nameof(Generators))]
    [Description("Creates a pseudo-random sentence from the Corset and Crinoline book.")]
    public void Corset_and_crinoline_sentence(IGenerator generator)
    {
        var corpus = File.ReadAllText(MarkovText.MarkovTextGenerator.DefaultCorpusPath);

        generator.BuildMarkovModel(corpus);

        Assert.That(generator.GenerateSentence(new RandomStub(73)), Is.EqualTo("These plates are deficient in width and insufficient in stiffness the corset were true, not a good deal discussed in my opinion there is a plate representing a lady faint at a royal fête."));
    }

    [TestCaseSource(nameof(Generators))]
    public void Build_single_word_model(IGenerator generator)
    {
        const string corpus = "Word.";

        generator.BuildMarkovModel(corpus, 1);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Word."));

        Assert.Throws<ArgumentException>(() => generator.BuildMarkovModel(corpus, 2));

        Assert.Throws<ArgumentException>(() => generator.BuildMarkovModel(corpus, 3));
    }

    [TestCaseSource(nameof(Generators))]
    public void Build_two_word_model(IGenerator generator)
    {
        const string corpus = "Two words.";

        generator.BuildMarkovModel(corpus, 1);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Two words."));

        generator.BuildMarkovModel(corpus, 2);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Two words."));

        Assert.Throws<ArgumentException>(() => generator.BuildMarkovModel(corpus, 3));
    }

    [TestCaseSource(nameof(Generators))]
    public void Build_three_word_model(IGenerator generator)
    {
        const string corpus = "Three word sentence.";

        generator.BuildMarkovModel(corpus, 1);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Three word sentence."));

        generator.BuildMarkovModel(corpus, 2);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Three word sentence."));

        generator.BuildMarkovModel(corpus, 3);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Three word sentence."));
    }

    [TestCaseSource(nameof(Generators))]
    public void Skip_sentences_with_too_few_words(IGenerator generator)
    {
        const string corpus = "Skip. Word sentence.";

        generator.BuildMarkovModel(corpus, 1);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Skip."));

        generator.BuildMarkovModel(corpus, 2);
        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("Word sentence."));

        Assert.Throws<ArgumentException>(() => generator.BuildMarkovModel(corpus, 3));
    }

    [TestCase("6a4b56d2")]
    [TestCase("7189a168")]
    [TestCase("1bf92c3c")]
    [TestCase("91691121")]
    [Description("All generator implementations produce same sentence given the same seed.")]
    public void MatchTest(string seed)
    {
        var corpus = File.ReadAllText(MarkovText.MarkovTextGenerator.DefaultCorpusPath);

        IGenerator generator1 = new MarkovText.MarkovTextGenerator();
        IGenerator generator2 = new SpanBasedMarkovTextGenerator();

        generator1.BuildMarkovModel(corpus);
        generator2.BuildMarkovModel(corpus);

        var sentence1 = generator1.GenerateSentence(seed);
        var sentence2 = generator2.GenerateSentence(seed);

        Console.WriteLine(sentence1);
        Console.WriteLine(sentence2);

        Assert.That(sentence1, Is.EqualTo(sentence2));
    }

    private static IEnumerable<IGenerator> Generators()
    {
        yield return new MarkovText.MarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator();
    }
}