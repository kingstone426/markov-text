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

#### Corpus

You can specify any plaintext file(s) as corpus source for the generator:

```
dotnet run --corpus Resources/dubliners.txt
```

#### Seed

The generator is deterministic and will always produce the same sentence if you provide a seed:

```
dotnet run --seed AnyTextStringCanGoHere
```

#### Order

By default, the generator looks at the last two words to determine the next word, but it can be configured to take longer word chains into consideration. Higher order Markov chains will generate more sensible - but less varied - sentences.

You can specify the Markov chain order like this:

```
dotnet run --order 3
```
> [!NOTE]
> In my experience, having order larger than 3 requires a very large corpus to produce interesting results. There is a big risk that three words only appear in sequence once throughout an entire book.
