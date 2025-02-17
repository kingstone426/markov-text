using CommandLine;
using MarkovText;

Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options =>
{
    if (string.IsNullOrEmpty(options.Seed))
    {
        options.Seed = Guid.NewGuid().ToString()[..8];
    }

    var markov = new MarkovTextGenerator(File.ReadAllText(options.Corpus), options.Order);

    Console.WriteLine();
    Console.WriteLine(markov.GenerateMarkov(options.Seed));
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine($"Seed: {options.Seed}");
});
