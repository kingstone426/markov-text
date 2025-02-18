namespace MarkovText.Tests;

public class Tests
{
    [TestCaseSource(nameof(Generators))]
    [Description("By mocking the random number generation we can produce the first three sentences of the corpus.")]
    public void First_three_sentences(IGenerator generator)
    {
        const string corpus = "The first sentence. The second sentence. The third sentence.";

        generator.BuildMarkovModel(corpus);

        Assert.That(generator.GenerateSentence(new RandomStub(0)), Is.EqualTo("The first sentence."));
        Assert.That(generator.GenerateSentence(new RandomStub(1)), Is.EqualTo("The second sentence."));
        Assert.That(generator.GenerateSentence(new RandomStub(2)), Is.EqualTo("The third sentence."));
    }

    [TestCaseSource(nameof(Generators))]
    [Description("Mocking random number generation with 1 so that the second alternative for 'dog was' gets selected, proceeding directly to 'very sad.'")]
    public void Branching_sentence(IGenerator generator)
    {
        const string corpus = "The big dog was happy but the small dog was very sad.";

        generator.BuildMarkovModel(corpus);

        Assert.That(generator.GenerateSentence(new RandomStub(1)), Is.EqualTo("The big dog was very sad."));
    }

    [TestCaseSource(nameof(Generators))]
    [Description("Mocking random number generation with 0 will generate an infinite sentence 'The big dog was happy but the small dog was happy but the small dog was happy but...'")]
    public void Infinitely_branching_sentence(IGenerator generator)
    {
        const string corpus = "The big dog was happy but the small dog was very sad.";

        generator.BuildMarkovModel(corpus);

        Assert.Throws<SentenceOverflowException>(() => generator.GenerateSentence(new RandomStub(0)));
    }

    [TestCaseSource(nameof(Generators))]
    [Description("Created a pseudo-random sentence from the Corset and Crinoline book.")]
    public void Corset_and_crinoline_sentence(IGenerator generator)
    {
        var corpus = File.ReadAllText(MarkovTextGenerator.DefaultCorpusPath);

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

    private static IEnumerable<IGenerator> Generators()
    {
        yield return new MarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator();
    }
}