using BenchmarkDotNet.Attributes;

namespace MarkovText.Benchmark;

[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class BenchmarkProgram
{
    private const string Seed = "f4e98e86"; // Using a constant seed for deterministic benchmarking

    [ParamsSource(nameof(Generators))]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public IGenerator Generator = null!;

    [GlobalSetup]
    public void Setup()
    {
        Generator.BuildMarkovModel(File.ReadAllText(MarkovTextGenerator.DefaultCorpusPath));
    }

    [Benchmark]
    public string BenchmarkGenerator()
    {
        return Generator.GenerateSentence(Seed);
    }

    public static IEnumerable<IGenerator> Generators()
    {
        yield return new MarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator();
    }
}