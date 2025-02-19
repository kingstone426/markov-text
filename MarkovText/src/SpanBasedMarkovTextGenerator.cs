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

    private readonly List<Range> SentenceStarters = new();

    // Maps prefix word sequences to suffix word sequences, e.g., "the big dog" => "big dog was"
    // Sequence ranges are expected to be the first occurrences in the corpus
    private readonly Dictionary<Range, List<Range>> StateTransitions = new();

    // Maps word sequences to the last word in the sequence, e.g., "the big dog" => "dog"
    private readonly Dictionary<Range, Range> FamousLastWords = new();

    // Tracks the first occurrence of a word sequence (substring) in the corpusAsText
    // Since ReadOnlySpan<char> cannot be used with Dictionaries, this uses a custom hashing solution
    private readonly Dictionary<int, List<Range>> FirstOccurrenceLookup = new ();

    // The sanitized corpus
    private ReadOnlyMemory<char> corpusMemory;

    // The order of the Markov chain (how many words in the "state" of the chain)
    private int Order;

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
        SentenceStarters.Clear();
        StateTransitions.Clear();
        FamousLastWords.Clear();

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

        var state = SentenceStarters.Random(random);
        stringBuilder.Append(corpusMemory[state]);
        var wordCount = Order;
        while (StateTransitions.TryGetValue(state, out var possibleTransitions))
        {
            if (++wordCount >= MaxWordCount)    // Prevents infinite loops
            {
                throw new SentenceOverflowException($"Word limit {wordCount} reached for sentence:\n{stringBuilder}");
            }

            var nextState = possibleTransitions.Random(random);
            var nextWord = corpusMemory[FamousLastWords[nextState]];
            stringBuilder.Append(' ');
            stringBuilder.Append(nextWord);

            state = nextState;
        }

        return stringBuilder.ToString();
    }

    private void AnalyzeCorpus(string corpusString)
    {
        // Replace line endings (e.g., poem line breaks) with a space
        corpusString = FixLineEndingsRegex().Replace(corpusString, " ");

        // Remove unwanted characters like page numbers, quotes, parentheses, etc.
        corpusString = SanitizerRegex().Replace(corpusString, "");

        // Normalize multiple consecutive spaces into a single space
        corpusString = MultipleWhitespaceRegex().Replace(corpusString, " ");

        corpusMemory = corpusString.AsMemory();

        var corpus = corpusMemory.Span;
        var slidingWindow = new CyclicArray<Range>(Order);
        var wordCount = 0;
        Range? previousRange = null;

        foreach (var word in corpus.Split(' '))
        {
            if (corpus[word].IsWhiteSpace())
            {
                continue;
            }

            slidingWindow[wordCount] = word;

            if (++wordCount < Order)
            {
                if (SentenceDelimiters.Contains(corpus[word.End.Value-1]))
                {
                    previousRange = default;
                    wordCount = 0;
                }

                continue;
            }

            var firstWord = slidingWindow[wordCount];
            var lastWord = slidingWindow[wordCount - 1];
            var range = new Range(firstWord.Start, lastWord.End);
            range = GetFirstWordSequenceOccurrence(corpus, range);

            if (previousRange == null)
            {
                SentenceStarters.Add(range);
            }
            else
            {
                StateTransitions.AddToList(previousRange.Value, range);
            }

            FamousLastWords.TryAdd(range, lastWord);
            previousRange = range;

            if (SentenceDelimiters.Contains(corpus[range.End.Value-1]))
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
