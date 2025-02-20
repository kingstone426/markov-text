using System.Text;
using System.Text.RegularExpressions;

namespace MarkovText;

/// <summary>
/// Class that generates text based on the Markov chain algorithm
/// </summary>
public partial class StringBasedMarkovTextGenerator : IGenerator
{
    // Safety limit for longest sentence that can be generated, to prevent infinite loops
    public int MaxWordCount = 1000;

    // The order of the Markov chain (how many words in the "state" of the chain)
    private int Order;

    // Default file path for the corpus text
    public const string DefaultCorpusPath = "Resources/thecorsetandthecrinoline.txt";

    // Phrases at the start of sentences are the initial states of the Markov chain
    private readonly List<string> SentenceStarterPhrases = new();

    // Maps prefix word phrases to suffix phrases, e.g., "the big dog" => "big dog was"
    // Tuple also holds the last word of the suffix phrase, e.g., "was"
    private readonly Dictionary<string, List<Tuple<string,string>>> PhraseTransitions = new();

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

    public override string ToString() => "StringBasedMarkovTextGenerator";

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
        var phrase = SentenceStarterPhrases.Random(random); // Choose a random starter key from the available starter keys
        stringBuilder.Append(phrase);  // Write the entire sentence starter phrase

        // Continuously generate words based on the Markov chain
        while (PhraseTransitions.TryGetValue(phrase, out var possibleTransitions))
        {
            if (++wordCount >= MaxWordCount)    // Safety check to prevent infinite loops
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            (phrase, var lastWordInPhrase) = possibleTransitions.Random(random);

            stringBuilder.Append(' ');
            stringBuilder.Append(lastWordInPhrase);   // Write the last word of the phrase to the generated text
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
        string? previousPhraseString = null;

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
                    previousPhraseString = null;
                    wordCount = 0;
                }

                continue;
            }

            var phrase = slidingWindow.CreateOffsetArray(wordCount);
            var phraseString = string.Join(' ', phrase);

            if (previousPhraseString == null)
            {
                SentenceStarterPhrases.Add(phraseString);
            }
            else
            {
                PhraseTransitions.AddToList(previousPhraseString, new Tuple<string, string>(phraseString, word));
            }

            previousPhraseString = phraseString;

            if (SentenceDelimiters.Contains(word[^1]))
            {
                previousPhraseString = null;
                wordCount = 0;
            }
        }
    }
}
