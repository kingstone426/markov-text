using System.Text;
using System.Text.RegularExpressions;

namespace MarkovText;

/// <summary>
/// Class that generates text based on the Markov chain algorithm
/// </summary>
public partial class SpanBasedMarkovTextGenerator : IGenerator
{
    // Safety limit for longest sentence that can be generated, to prevent infinite loops
    public int MaxWordCount = 1000;

    // The order of the Markov chain (how many words in the "state" of the chain)
    private int Order;

    // Phrases at the start of sentences are the initial states of the Markov chain
    private readonly List<Range> SentenceStarterPhrases = new();

    // Maps prefix word phrases to suffix phrases, e.g., "the big dog" => "big dog was"
    // Phrases are expected to be the first occurrences in the corpus
    private readonly Dictionary<Range, List<Range>> PhraseTransitions = new();

    // Maps phrases to the last word in the phrase, e.g., "the big dog" => "dog"
    private readonly Dictionary<Range, Range> FamousLastWords = new();

    // Tracks the first occurrence of a phrase (substring) in the corpus
    // Since ReadOnlySpan<char> cannot be used with Dictionaries, this uses a custom hashing solution
    private readonly Dictionary<int, List<Range>> FirstOccurrenceLookup = new ();

    // The sanitized corpus
    private ReadOnlyMemory<char> Corpus;

    // Sentence delimiters used to detect sentence boundaries
    private static readonly char[] SentenceDelimiters = { '.', '?', '!' };

    // Thread-local StringBuilder to avoid memory overhead from multiple threads
    private readonly ThreadLocal<StringBuilder> threadLocalStringBuilder = new(() => new StringBuilder());

    [GeneratedRegex(@"\[.+?\]|\""|\)|\(|\'|\n|\r|“|”|’|_")]
    private static partial Regex SanitizerRegex();

    [GeneratedRegex("\n")]
    private static partial Regex FixLineEndingsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    public override string ToString() => "SpanBasedMarkovTextGenerator";

    public void BuildMarkovModel(string corpus, int order = 2)
    {
        Order = order;

        FirstOccurrenceLookup.Clear();
        SentenceStarterPhrases.Clear();
        PhraseTransitions.Clear();
        FamousLastWords.Clear();

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
        stringBuilder.Append(Corpus[phrase]);

        // Continuously generate words based on the Markov chain
        while (PhraseTransitions.TryGetValue(phrase, out var possibleTransitions))
        {
            if (++wordCount >= MaxWordCount)    // Safety check to prevent infinite loops
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            phrase = possibleTransitions.Random(random);
            var nextWord = Corpus[FamousLastWords[phrase]];
            stringBuilder.Append(' ');
            stringBuilder.Append(nextWord); // Append the next word to the generated text
        }

        return stringBuilder.ToString();    // Return the generated Markov text
    }

    private void AnalyzeCorpus(string corpus)
    {
        // Replace line endings (e.g., poem line breaks) with a space
        corpus = FixLineEndingsRegex().Replace(corpus, " ");

        // Remove unwanted characters like page numbers, quotes, parentheses, etc.
        corpus = SanitizerRegex().Replace(corpus, "");

        // Normalize multiple consecutive spaces into a single space
        corpus = MultipleWhitespaceRegex().Replace(corpus, " ");

        // Store the sanitized corpus
        Corpus = corpus.AsMemory();

        var corpusSpan = Corpus.Span;
        var slidingWindow = new CyclicArray<Range>(Order);
        var wordCount = 0;
        Range? previousRange = null;

        foreach (var word in corpusSpan.Split(' ')) // Split the corpus into words
        {
            if (corpusSpan[word].IsWhiteSpace())
            {
                continue;
            }

            slidingWindow[wordCount] = word;

            if (++wordCount < Order)
            {
                if (SentenceDelimiters.Contains(corpusSpan[word.End.Value-1]))
                {
                    previousRange = default;
                    wordCount = 0;
                }

                continue;
            }

            var firstWord = slidingWindow[wordCount];
            var lastWord = slidingWindow[wordCount - 1];
            var range = new Range(firstWord.Start, lastWord.End);
            range = GetFirstWordSequenceOccurrence(corpusSpan, range);

            if (previousRange == null)
            {
                SentenceStarterPhrases.Add(range);
            }
            else
            {
                PhraseTransitions.AddToList(previousRange.Value, range);
            }

            FamousLastWords.TryAdd(range, lastWord);
            previousRange = range;

            if (SentenceDelimiters.Contains(corpusSpan[range.End.Value-1]))
            {
                previousRange = default;
                wordCount = 0;
            }
        }
    }

    private Range GetFirstWordSequenceOccurrence(ReadOnlySpan<char> span, Range newRange)
    {
        var hash = span[newRange].GetStableHashCode();

        if (!FirstOccurrenceLookup.TryGetValue(hash, out var list))
        {
            FirstOccurrenceLookup.Add(hash, new List<Range> { newRange });
            return newRange;
        }

        foreach (var existingRange in list)
        {
            if (span[existingRange].Equals(span[newRange], StringComparison.Ordinal))
            {
                return existingRange;
            }
        }

        list.Add(newRange);

        return newRange;
    }
}
