use std::collections::HashMap;

struct StringBasedMarkovTextGenerator {
    order: usize,
    sentences_starter_phrases: Vec<String>,
    PhraseTransitions: HashMap<String, Vec<(String, String)>
}

impl StringBasedMarkovTextGenerator {
    pub const MaxWordCount: usize = 1000;

    pub fn build_markov_model(mut self, corpus: &str, order: usize)     {
        self.order = order

    }
}
