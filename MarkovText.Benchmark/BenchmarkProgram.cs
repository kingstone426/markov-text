using BenchmarkDotNet.Attributes;

namespace MarkovText.Benchmark;

[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class BenchmarkProgram
{
    private const string Seed = "6a4156e2"; // Using a constant seed for deterministic benchmarking

    [ParamsSource(nameof(Generators))]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public IGenerator Generator = null!;

    [GlobalSetup]
    public void Setup()
    {
        Generator.BuildMarkovModel(File.ReadAllText(ArrayBasedMarkovTextGenerator.DefaultCorpusPath));
    }

    [Benchmark]
    public string BenchmarkGenerator()
    {
        return Generator.GenerateSentence(Seed);
    }

    public static IEnumerable<IGenerator> Generators()
    {
        yield return new ArrayBasedMarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator();
    }
}