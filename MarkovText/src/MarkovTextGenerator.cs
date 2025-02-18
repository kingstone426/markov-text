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

    // List of starter keys for initializing the Markov chain
    private readonly List<string[]> StarterKeys = new();

    // Dictionary mapping prefixes (key) to possible suffixes (values)
    private readonly Dictionary<string[], List<string>> PrefixToSuffix = new(new StringArrayEqualityComparer());

    // The order of the Markov chain (how many words in the "state" of the chain)
    private int Order;

    // Thread-local StringBuilder to avoid memory overhead from multiple threads
    private readonly ThreadLocal<StringBuilder> threadLocalStringBuilder = new(() => new StringBuilder());

    // Regex patterns for sentence splitting, sanitization, and whitespace normalization
    [GeneratedRegex(@"(?<=[.!?])")]
    private static partial Regex SentenceDelimiterRegex();

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

        StarterKeys.Clear();
        PrefixToSuffix.Clear();

        AnalyzeCorpus(corpus);  // Analyze the provided corpus to build the Markov model
    }

    public string GenerateSentence(IRandomNumberGenerator random)
    {
        if (StarterKeys.Count == 0)
        {
            throw new InvalidOperationException($"There is no Markov model. You need to call {nameof(BuildMarkovModel)} first.");
        }

        var stringBuilder = threadLocalStringBuilder.Value;
        stringBuilder!.Clear();  // Clear the StringBuilder for reuse

        var wordCount = 0;  // Track the current word count to prevent infinite loops

        // Choose a random starter key from the available starter keys
        var words = StarterKeys[random.Next(StarterKeys.Count)].ToArray();
        stringBuilder.Append(words[0]);  // Append the first word of the chosen starter key

        // Continuously generate words based on the Markov chain
        while (PrefixToSuffix.TryGetValue(words, out var newPossibleWords))
        {
            // Safety check to prevent infinite loops
            if (++wordCount >= MaxWordCount)
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            // Get the next word (suffix) based on the current key (prefix)
            var newWord = newPossibleWords.Random(random);

            // Shift the words in the key to make space for the new word
            for (var i = 0; i < Order - 1; i++)
            {
                words[i] = words[i + 1];
            }
            words[^1] = newWord;

            // Append the next word to the generated text
            stringBuilder.Append(' ');
            stringBuilder.Append(words[0]);
        }

        // Append any remaining words to the result
        for (var i = 1; i < Order; i++)
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
        // Replace line endings (e.g., poem line breaks) with a space
        corpus = FixLineEndingsRegex().Replace(corpus, " ");

        // Remove unwanted characters like page numbers, quotes, parentheses, etc.
        corpus = SanitizerRegex().Replace(corpus, "");

        // Normalize multiple consecutive spaces into a single space
        corpus = MultipleWhitespaceRegex().Replace(corpus, " ");

        var sentences = SentenceDelimiterRegex().Split(corpus); // Split corpus by sentence
        foreach (var sentence in sentences)
        {
            AnalyzeSentence(sentence);  // Analyze each sentence individually
        }

        if (StarterKeys.Count == 0)
        {
            throw new ArgumentException($"No phrases of order {Order} could be generated from the corpus: {corpus}");
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

        // Split the sentence into words
        var words = sentence.Trim().Split(' ');

        // If there are not enough words to create a Markov chain of the specified order, skip
        if (words.Length < Order)
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

        if (words.Length == Order)
        {
            return;
        }

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
