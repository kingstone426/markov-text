use crate::random_number_generator::RandomNumberGeneratorImpl;
use crate::string_based_markov_text_generator::markov;
use anyhow::Result;
use std::fs::read_to_string;

mod cyclic_array;
mod random_number_generator;
mod string_based_markov_text_generator;

fn main() -> Result<()> {
    let mut generator = markov::StringBasedMarkovTextGenerator::new();

    let content = read_to_string("../MarkovText/Resources/thecorsetandthecrinoline.txt")?;

    generator.build_markov_model(&content, 10)?;

    let result = generator.generate_sentence(RandomNumberGeneratorImpl::default());

    if let Ok(r) = result {
        println!("{}", r);
    } else {
        println!("No result");
    }

    Ok(())
}
