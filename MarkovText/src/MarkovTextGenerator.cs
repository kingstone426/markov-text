using System.Text;
using System.Text.RegularExpressions;

namespace MarkovText;

/// <summary>
/// Class that generates text based on the Markov chain algorithm
/// </summary>
public partial class MarkovTextGenerator
{
    // Safety limit for longest sentence that can be generated, to prevent infinite loops
    public int MaxWordCount = 1000;

    // Default file path for the corpus text
    public const string DefaultCorpusPath = "Resources/thecorsetandthecrinoline.txt";

    // List of starter keys for initializing the Markov chain
    private readonly List<string[]> StarterKeys = new();

    // Dictionary mapping prefixes (key) to possible suffixes (values)
    private readonly Dictionary<string[], List<string>> PrefixToSuffix = new(new StringArrayEqualityComparer());

    // The order of the Markov chain (how many words in the "state" of the chain)
    private readonly int Order;

    // Thread-local StringBuilder to avoid memory overhead from multiple threads
    private readonly ThreadLocal<StringBuilder> threadLocalStringBuilder = new(() => new StringBuilder());

    // Sentence delimiters used to detect sentence boundaries
    private static readonly char[] SentenceDelimiters = { '.', '?', '!' };

    // Regex patterns for sentence splitting, sanitization, and whitespace normalization
    [GeneratedRegex(@"(?<=[.!?])")]
    private static partial Regex SentenceDelimiterRegex();

    [GeneratedRegex(@"\[.*\]|\""|\)|\(|\'|\n|\r|“|”|’|_")]
    private static partial Regex SanitizerRegex();

    [GeneratedRegex("\n")]
    private static partial Regex FixLineEndingsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    /// <summary>
    /// Constructor for the MarkovTextGenerator, takes a corpus and an optional order
    /// </summary>
    /// <param name="corpus">The text to analyze</param>
    /// <param name="order">The number of preceding words that determine the next word</param>
    public MarkovTextGenerator(string corpus, int order = 2)
    {
        Order = order;
        AnalyzeCorpus(corpus);  // Analyze the provided corpus to build the Markov model
    }

    /// <summary>
    /// Generate a random Markov text using a default seed
    /// </summary>
    public string GenerateMarkov() => GenerateMarkov(Guid.NewGuid().ToString()[..8]);

    /// <summary>
    /// Generate a random Markov text using a custom seed (string)
    /// </summary>
    public string GenerateMarkov(string seed) => GenerateMarkov(new Random(seed.GetStableHashCode()));

    /// <summary>
    /// Generate a random Markov text using a custom Random object
    /// </summary>
    public string GenerateMarkov(Random random) => GenerateMarkov(new DefaultRandom(random));

    /// <summary>
    /// Generate a random Markov text using a custom random number generator interface
    /// </summary>
    public string GenerateMarkov(IRandomNumberGenerator random)
    {
        var stringBuilder = threadLocalStringBuilder.Value;
        stringBuilder!.Clear();  // Clear the StringBuilder for reuse

        var wordCount = 0;  // Track the current word count to prevent infinite loops

        // Choose a random starter key from the available starter keys
        var words = StarterKeys[random.Next(StarterKeys.Count)].ToArray();
        stringBuilder.Append(words[0]);  // Append the first word of the chosen starter key

        // Continuously generate words based on the Markov chain
        while (true)
        {
            // Safety check to prevent infinite loops
            if (++wordCount >= MaxWordCount)
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            // Get the next word (suffix) based on the current key (prefix)
            var newWord = PrefixToSuffix[words].Random(random);

            // Shift the words in the key to make space for the new word
            for (var i = 0; i < Order - 1; i++)
            {
                words[i] = words[i + 1];
            }
            words[^1] = newWord;

            // If the last word ends with a sentence delimiter, stop the generation
            var lastCharOfLastWord = words[^1][^1];
            if (SentenceDelimiters.Contains(lastCharOfLastWord))
            {
                break;
            }

            // Otherwise, append the next word to the generated text
            stringBuilder.Append(' ');
            stringBuilder.Append(words[0]);
        }

        // Append any remaining words to the result
        for (var i = 0; i < Order; i++)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(words[i]);
        }

        return stringBuilder.ToString();  // Return the generated Markov text
    }

    /// <summary>
    /// Analyzes the entire corpus by splitting it into sentences
    /// </summary>
    private void AnalyzeCorpus(string corpus)
    {
        var sentences = SentenceDelimiterRegex().Split(corpus); // Split corpus by sentence
        foreach (var sentence in sentences)
        {
            AnalyzeSentence(sentence);  // Analyze each sentence individually
        }
    }

    /// <summary>
    /// Analyzes a single sentence and populates the Markov chain model
    /// </summary>
    private void AnalyzeSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
        {
            return;  // Skip empty or whitespace-only sentences
        }

        // Replace line endings (e.g., poem line breaks) with a space
        sentence = FixLineEndingsRegex().Replace(sentence, " ");

        // Remove unwanted characters like page numbers, quotes, parentheses, etc.
        sentence = SanitizerRegex().Replace(sentence, "");

        // Normalize multiple consecutive spaces into a single space
        sentence = MultipleWhitespaceRegex().Replace(sentence, " ");

        // Split the sentence into words
        var words = sentence.Trim().Split(' ');

        // If there are not enough words to create a Markov chain of the specified order, skip
        if (words.Length <= Order)
        {
            return;
        }

        // Process the first "Order" words as the initial key for the Markov chain
        var key = new string[Order];
        for (var j = 0; j < Order; j++)
        {
            key[j] = words[j].Trim();
        }

        // Add the initial key to the list of starter keys
        StarterKeys.Add(key.ToArray());

        // Map the key to the first possible word (suffix) that follows the key
        PrefixToSuffix.AddToList(key, words[Order].Trim());

        // Now process the rest of the words to build the Markov chain
        for (var i = 1; i < words.Length - Order; i++)
        {
            key = new string[Order]; // Create a new key for each sliding window of words
            for (var j = 0; j < Order; j++)
            {
                key[j] = words[i + j].Trim();
            }

            // Add the new word (suffix) for the current key
            var wordToAdd = words[i + Order].Trim();
            PrefixToSuffix.AddToList(key, wordToAdd);
        }
    }
}
