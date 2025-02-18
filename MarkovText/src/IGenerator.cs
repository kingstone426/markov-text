namespace MarkovText;

public interface IGenerator
{
    /// <summary>
    /// Indexes the given corpus text to prepare for GenerateSentence.
    /// Clears any previously indexed corpus.
    /// </summary>
    /// <param name="corpus">The text to analyze</param>
    /// <param name="order">The number of preceding words that determine the next word</param>
    public void BuildMarkovModel(string corpus, int order = 2);

    /// <summary>
    /// Generate a random Markov text using a default seed
    /// </summary>
    public string GenerateSentence() => GenerateSentence(Guid.NewGuid().ToString()[..8]);

    /// <summary>
    /// Generate a random Markov text using a custom seed
    /// </summary>
    public string GenerateSentence(string seed) => GenerateSentence(new Random(seed.GetStableHashCode()));

    /// <summary>
    /// Generate a random Markov text using a custom Random object
    /// </summary>
    public string GenerateSentence(Random random) => GenerateSentence(new DefaultRandom(random));

    /// <summary>
    /// Generate a random Markov text using a custom random number generator interface
    /// </summary>
    public string GenerateSentence(IRandomNumberGenerator random);
}
