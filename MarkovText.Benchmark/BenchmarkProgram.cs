using BenchmarkDotNet.Attributes;

namespace MarkovText.Benchmark;

[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class BenchmarkGenerateSentence
{
    private const string Seed = "6a4156e2"; // Using a constant seed for deterministic benchmarking

    [ParamsSource(nameof(Generators))]
    public IGenerator Generator = null!;

    private string corpus = null!;

    [GlobalSetup]
    public void Setup()
    {
        corpus = File.ReadAllText(ArrayBasedMarkovTextGenerator.DefaultCorpusPath);
        Generator.BuildMarkovModel(corpus);
    }

    [Benchmark]
    public string GenerateSentence()
    {
        return Generator.GenerateSentence(Seed);
    }

    public static IEnumerable<IGenerator> Generators()
    {
        yield return new StringBasedMarkovTextGenerator();
        yield return new ArrayBasedMarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator_CreateString();
    }
}

[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class BenchmarkBuildMarkovModel
{
    [ParamsSource(nameof(Generators))]
    public IGenerator Generator = null!;

    private string corpus = null!;

    [GlobalSetup]
    public void Setup()
    {
        corpus = File.ReadAllText(ArrayBasedMarkovTextGenerator.DefaultCorpusPath);
    }

    [Benchmark]
    public void BuildMarkovModel()
    {
        Generator.BuildMarkovModel(corpus);
    }

    public static IEnumerable<IGenerator> Generators()
    {
        yield return new StringBasedMarkovTextGenerator();
        yield return new ArrayBasedMarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator();
        yield return new SpanBasedMarkovTextGenerator_CreateString();
    }
}