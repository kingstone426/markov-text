# Markov Text Generator

This is a Markov chain text generator implemented in .NET 9.0. It was inspired by a [talk by Jason Grinblat](https://youtu.be/3AjlsTtrfVY) of Freehold Games.

The program generates garbled and confusing sentences given a piece of text (a "corpus"). The algorithm selects the next word of the sentence based on the previous words in a pseudo-random manner.

## Examples

The book [The corset and the crinoline](https://www.gutenberg.org/ebooks/53267) (originally published in 1868) generates sentences like these:

> The ring held in the form is still worn by ladies and of the richest materials, embroidered in gold fringes. 

> Slender waists have become a second nature to lead us to dress, and a friend of mine may arise from my stays, especially after dinner, not that I could easily have clasped it with a favourite flower. 

> The long double-looped round lace used is, I believe, considered small, I can look upon my vanities as so many ladies that compress themselves into taper forms of the corset worn at an advanced age? 

## Running the code

To generate a random sentence, simply type

```
cd MarkovText
dotnet run
```

## Configuration

You can specify any plaintext file(s) as corpus source for the generator:

```
dotnet run --corpus Resources/dubliners.txt
```

The generator is deterministic and will always produce the same sentence if you provide a seed:

```
dotnet run --seed AnyTextStringCanGoHere
```

By default, the generator looks at the last two words to determine the next word, but it can be configured to take longer word chains into consideration. Higher order Markov chains will generate more sensible - but less varied - sentences.

You can specify the Markov chain order like this:

```
dotnet run --order 3
```
> [!NOTE]
> In my experience, having order larger than 3 requires a very large corpus to produce interesting results. There is a big risk that three words only appear in sequence once throughout an entire book.

## Performance

I ended up writing three separate implementations of the Markov generator. The three implementations are all functionally equivalent but use different internal representations of the Markov model. 

It turns out that the string-based model generates sentences slightly faster than the array-based and span-based models. The 1008 B allocated are just the string returned by the GenerateSentence function.

```
dotnet run -c Release --project ../MarkovText.Benchmark/ --filter *GenerateSentence*
```

| Method           | Generator | Mean     | Error     | StdDev    | Median   | Gen0   | Allocated |
|----------------- |---------- |---------:|----------:|----------:|---------:|-------:|----------:|
| GenerateSentence | Array     | 2.276 us | 0.0053 us | 0.0077 us | 2.274 us | 0.0801 |    1008 B |
| GenerateSentence | Span      | 2.615 us | 0.0092 us | 0.0126 us | 2.622 us | 0.0801 |    1008 B |
| GenerateSentence | String    | 2.104 us | 0.0130 us | 0.0186 us | 2.107 us | 0.0801 |    1008 B |

The string-based model, however, comes with a larger memory footprint.

```
dotnet run -c Release --project ../MarkovText.Benchmark/ --filter *BuildMarkovModel*
```

| Method           | Generator | Mean     | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated |
|----------------- |---------- |---------:|---------:|---------:|---------:|---------:|---------:|----------:|
| BuildMarkovModel | Array     | 13.35 ms | 0.150 ms | 0.225 ms | 890.6250 | 859.3750 | 468.7500 |   7.38 MB |
| BuildMarkovModel | Span      | 15.21 ms | 0.230 ms | 0.330 ms | 875.0000 | 843.7500 | 437.5000 |   8.48 MB |
| BuildMarkovModel | String    | 14.68 ms | 0.178 ms | 0.255 ms | 906.2500 | 671.8750 | 234.3750 |  10.33 MB |