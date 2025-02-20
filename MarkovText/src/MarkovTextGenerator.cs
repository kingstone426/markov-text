using System.Text;
using System.Text.RegularExpressions;

namespace MarkovText;

/// <summary>
/// Class that generates text based on the Markov chain algorithm
/// </summary>
public partial class MarkovTextGenerator : IGenerator
{
    // Safety limit for longest sentence that can be generated, to prevent infinite loops
    public int MaxWordCount = 1000;

    // The order of the Markov chain (how many words in the "state" of the chain)
    private int Order;

    // Default file path for the corpus text
    public const string DefaultCorpusPath = "Resources/thecorsetandthecrinoline.txt";

    // Phrases at the start of sentences are the initial states of the Markov chain
    private readonly List<string[]> SentenceStarterPhrases = new();

    // Maps prefix word phrases to suffix phrases, e.g., "the big dog" => "big dog was"
    private readonly Dictionary<string[], List<string[]>> PhraseTransitions = new(new StringArrayEqualityComparer());

    // Thread-local StringBuilder to avoid memory overhead from multiple threads
    private readonly ThreadLocal<StringBuilder> threadLocalStringBuilder = new(() => new StringBuilder());

    // Sentence delimiters used to detect sentence boundaries
    private static readonly char[] SentenceDelimiters = { '.', '?', '!' };

    // Regex patterns for sentence splitting, sanitization, and whitespace normalization
    [GeneratedRegex(@"\[.+?\]|\""|\)|\(|\'|\n|\r|“|”|’|_")]
    private static partial Regex SanitizerRegex();

    [GeneratedRegex("\n")]
    private static partial Regex FixLineEndingsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    public override string ToString() => "ArrayBasedMarkovTextGenerator";

    public void BuildMarkovModel(string corpus, int order = 2)
    {
        Order = order;

        SentenceStarterPhrases.Clear();
        PhraseTransitions.Clear();

        AnalyzeCorpus(corpus);  // Analyze the corpus and build the Markov model

        if (SentenceStarterPhrases.Count == 0)
        {
            throw new ArgumentException($"No phrases of order {Order} could be generated from the corpus: {corpus}");
        }
    }

    public string GenerateSentence(IRandomNumberGenerator random)
    {
        if (SentenceStarterPhrases.Count == 0)
        {
            throw new InvalidOperationException($"There is no Markov model. You need to call {nameof(BuildMarkovModel)} first.");
        }

        var stringBuilder = threadLocalStringBuilder.Value;
        stringBuilder!.Clear();  // Clear the StringBuilder for reuse

        var wordCount = Order;  // Track the current word count to prevent infinite loops

        // Choose a random starter key from the available starter keys
        var phrase = SentenceStarterPhrases.Random(random);
        stringBuilder.Append(string.Join(' ', phrase));  // Append the starter phrase

        // Continuously generate words based on the Markov chain
        while (PhraseTransitions.TryGetValue(phrase, out var possibleTransitions))
        {
            if (++wordCount >= MaxWordCount)    // Safety check to prevent infinite loops
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            phrase = possibleTransitions.Random(random);

            stringBuilder.Append(' ');
            stringBuilder.Append(phrase[^1]);   // Append the last word of the phrase to the generated text
        }

        return stringBuilder.ToString();  // Return the generated Markov text
    }


    private void AnalyzeCorpus(string corpus)
    {
        // Replace line endings (e.g., poem line breaks) with a space
        corpus = FixLineEndingsRegex().Replace(corpus, " ");

        // Remove unwanted characters like page numbers, quotes, parentheses, etc.
        corpus = SanitizerRegex().Replace(corpus, "");

        // Normalize multiple consecutive spaces into a single space
        corpus = MultipleWhitespaceRegex().Replace(corpus, " ");

        var wordCount = 0;
        var slidingWindow = new CyclicArray<string>(Order);
        string[]? previousPhrase = null;

        foreach (var word in corpus.Trim().Split(' '))  // Split the corpus into words
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                continue;
            }

            slidingWindow[wordCount] = word;

            if (++wordCount < Order)
            {
                if (SentenceDelimiters.Contains(word[^1]))
                {
                    previousPhrase = null;
                    wordCount = 0;
                }

                continue;
            }

            var phrase = slidingWindow.CreateOffsetArray(wordCount);

            if (previousPhrase == null)
            {
                SentenceStarterPhrases.Add(phrase);
            }
            else
            {
                PhraseTransitions.AddToList(previousPhrase, phrase);
            }

            previousPhrase = phrase;

            if (SentenceDelimiters.Contains(word[^1]))
            {
                previousPhrase = null;
                wordCount = 0;
            }
        }
    }
}
