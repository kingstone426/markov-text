using BenchmarkDotNet.Attributes;

namespace MarkovText.Benchmark;

[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class BenchmarkProgram
{
    private const string Seed = "f4e98e86"; // Using a constant seed for deterministic benchmarking

    private MarkovTextGenerator Markov = null!;

    [GlobalSetup]
    public void Setup()
    {
        Markov = new MarkovTextGenerator(File.ReadAllText(MarkovTextGenerator.DefaultCorpusPath));
    }

    [Benchmark]
    public string BenchmarkArrayBasedMarkov()
    {
        return Markov.GenerateMarkov(Seed);
    }
}