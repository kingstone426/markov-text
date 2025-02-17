﻿using System.Text;
using System.Text.RegularExpressions;

namespace MarkovText;

public partial class MarkovTextGenerator
{
    public const string DefaultCorpusPath = "Resources/thecorsetandthecrinoline.txt";

    private readonly List<string[]> StarterKeys = new();
    private readonly Dictionary<string[], List<string>> PrefixToSuffix = new(new StringArrayEqualityComparer());
    private readonly int Order;
    private readonly ThreadLocal<StringBuilder> stringBuilder = new (() => new StringBuilder());
    private static readonly char[] SentenceDelimiters = { '.', '?', '!' };

    [GeneratedRegex(@"(?<=[.!?])")]
    private static partial Regex SentenceDelimiterRegex();

    [GeneratedRegex(@"\[.*\]|\""|\)|\(|\'|\n|\r|“|”|’|_")]
    private static partial Regex SanitizerRegex();

    [GeneratedRegex("\n")]
    private static partial Regex FixLineEndingsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    public MarkovTextGenerator(string corpus, int order=2)
    {
        Order = order;

        AnalyzeCorpus(corpus);
    }

    private void AnalyzeCorpus(string corpus)
    {
        var sentences = SentenceDelimiterRegex().Split(corpus);
        foreach (var sentence in sentences)
        {
            AnalyzeSentence(sentence);
        }
    }

    private void AnalyzeSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
        {
            return;
        }

        // Some poems end lines with no space or delimiter, causing the word to stick to the word on the next line.
        sentence = FixLineEndingsRegex().Replace(sentence, " ");

        // Remove page numbers, quotes, parentheses etc
        sentence = SanitizerRegex().Replace(sentence, "");

        // Remove multiple consecutive spaces and tabs
        sentence = MultipleWhitespaceRegex().Replace(sentence, " ");

        var words = sentence.Trim().Split(' ');

        if (words.Length <= Order)
            return;

        var key = new string[Order];
        for (var j = 0; j < Order; j++)
        {
            key[j] = words[j].Trim();
        }
        StarterKeys.Add(key.ToArray());
        PrefixToSuffix.Add(key, words[Order].Trim());

        for (var i = 1; i < words.Length - Order; i++)
        {
            key = new string[Order]; // Need to instantiate new array so each key is unique
            for (var j = 0; j < Order; j++)
            {
                key[j] = words[i+j].Trim();
            }

            var wordToAdd = words[i + Order].Trim();
            PrefixToSuffix.Add(key, wordToAdd);
        }
    }

    public string GenerateMarkov() => GenerateMarkov(Guid.NewGuid().ToString()[..8]);

    public string GenerateMarkov(string seed) => GenerateMarkov(new Random(seed.GetStableHashCode()));

    public string GenerateMarkov(Random random) => GenerateMarkov(new DefaultRandom(random));

    public string GenerateMarkov(IRandomNumberGenerator random)
    {
        var sb = stringBuilder.Value;
        sb!.Clear();

        var words = StarterKeys[random.Next(StarterKeys.Count)].ToArray();  // ToArray clones the array to prevent mutating it inside the list
        sb.Append(words[0]);

        while (true)
        {
            var newWord = PrefixToSuffix[words].Random(random);

            // Shift words to the left
            for (var i = 0; i < Order - 1; i++)
            {
                words[i] = words[i + 1];
            }
            words[^1] = newWord;

            var lastCharOfLastWord = words[^1][^1];
            if (SentenceDelimiters.Contains(lastCharOfLastWord))
            {
                break;
            }

            sb.Append(' ');
            sb.Append(words[0]);
        }

        for (var i = 0;i<Order;i++ )
        {
            sb.Append(' ');
            sb.Append(words[i]);
        }

        return sb.ToString();
    }
}

