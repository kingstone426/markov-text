using CommandLine;

namespace MarkovText;

public class CommandLineOptions
{
    [Option('o', "order", Required = false, HelpText = "The order of the Markov chain.", Default = 2)]
    public required int Order { get; set; }

    [Option('c', "corpus", Required = false, HelpText = "Path to the corpus text file.", Default = MarkovTextGenerator.DefaultCorpusPath)]
    public required string Corpus { get; set; }

    [Option('s', "seed", Required = false, HelpText = "Seed used to initialize random number generator.")]
    public required string Seed { get; set; }
}