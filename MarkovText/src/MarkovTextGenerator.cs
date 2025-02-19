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

    // Default file path for the corpus text
    public const string DefaultCorpusPath = "Resources/thecorsetandthecrinoline.txt";

    private readonly List<string[]> SentenceStarters = new();

    private readonly Dictionary<string[], List<string[]>> StateTransitions = new(new StringArrayEqualityComparer());

    // The order of the Markov chain (how many words in the "state" of the chain)
    private int Order;

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

        SentenceStarters.Clear();
        StateTransitions.Clear();

        AnalyzeCorpus(corpus);  // Analyze the provided corpus to build the Markov model

        if (SentenceStarters.Count == 0)
        {
            throw new ArgumentException($"No phrases of order {Order} could be generated from the corpus: {corpus}");
        }
    }

    public string GenerateSentence(IRandomNumberGenerator random)
    {
        if (SentenceStarters.Count == 0)
        {
            throw new InvalidOperationException($"There is no Markov model. You need to call {nameof(BuildMarkovModel)} first.");
        }

        var stringBuilder = threadLocalStringBuilder.Value;
        stringBuilder!.Clear();  // Clear the StringBuilder for reuse

        var wordCount = Order;  // Track the current word count to prevent infinite loops

        // Choose a random starter key from the available starter keys
        var phrase = SentenceStarters.Random(random);//[random.Next(StarterKeys.Count)].ToArray();
        stringBuilder.Append(string.Join(' ', phrase));  // Append the starter phrase

        // Continuously generate words based on the Markov chain
        while (StateTransitions.TryGetValue(phrase, out var possibleTransitions))
        {
            // Safety check to prevent infinite loops
            if (++wordCount >= MaxWordCount)
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            phrase = possibleTransitions.Random(random);

            // Append the next word to the generated text
            stringBuilder.Append(' ');
            stringBuilder.Append(phrase[^1]);
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

        foreach (var word in corpus.Trim().Split(' '))    // Split the sentence into words
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
                SentenceStarters.Add(phrase);
            }
            else
            {
                StateTransitions.AddToList(previousPhrase, phrase);
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
