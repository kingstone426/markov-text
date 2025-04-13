pub mod markov {
    use crate::cyclic_array::markov::CyclicArray;
    use crate::random_number_generator::RandomNumberGenerator;
    use anyhow::anyhow;
    use regex::Regex;
    use std::collections::HashMap;

    const MAX_WORD_COUNT: usize = 1000;

    pub struct StringBasedMarkovTextGenerator {
        order: usize,
        sentences_starter_phrases: Vec<String>,
        phrase_transitions: HashMap<String, Vec<(String, String)>>,
        sentence_delimiters: Vec<char>,
    }

    impl StringBasedMarkovTextGenerator {
        pub(crate) fn generate_sentence(
            &self,
            mut random: impl RandomNumberGenerator,
        ) -> Result<String, anyhow::Error> {
            if self.sentences_starter_phrases.is_empty() {
                return Err(anyhow!(
                    "There is no markov model, call build_markov_model first"
                ));
            }

            let mut string = String::new();
            let mut word_count = self.order;
            let mut phrase = self.sentences_starter_phrases
                [random.next_u32() as usize % self.sentences_starter_phrases.len()]
            .clone();
            string.push_str(&phrase);

            while let Some(phrase_transition) = self.phrase_transitions.get(&phrase) {
                word_count += 1;
                if word_count >= MAX_WORD_COUNT {
                    return Err(anyhow!(
                        "Word limit reached {word_count} reached for sentence: {string}"
                    ));
                }

                let randomu32 = random.next_u32();
                let len = phrase_transition.len();
                let index = randomu32 as usize % len;

                let (s, last_word_in_phrase) = phrase_transition.get(index).unwrap();

                phrase = s.clone();

                string.push(' ');
                string.push_str(last_word_in_phrase);
            }

            Ok(string)
        }
    }

    impl StringBasedMarkovTextGenerator {

        pub fn new() -> StringBasedMarkovTextGenerator {
            StringBasedMarkovTextGenerator {
                phrase_transitions: HashMap::new(),
                order: 0,
                sentences_starter_phrases: vec![],
                sentence_delimiters: vec![',', '.', '!'],
            }
        }

        pub fn build_markov_model(
            &mut self,
            corpus: &str,
            order: usize,
        ) -> Result<(), anyhow::Error> {
            self.order = order;

            self.analyze_corpus(corpus)?;

            if self.phrase_transitions.is_empty() {
                return Err(anyhow!("No phrases found in the corpus"));
            }

            if self.sentences_starter_phrases.is_empty() {
                return Err(anyhow!(
                    r"No phrases of order {} could be generated from the corpus: {}",
                    self.order,
                    corpus
                ));
            }

            Ok(())
        }

        pub fn analyze_corpus(&mut self, corpus: &str) -> Result<(), anyhow::Error> {
            let s = corpus.replace("\n", " ");
            let santize = Regex::new(r#"\[.+?\]|\""|\)|\(|\'|\n|\r|“|”|’|_"#)?;
            let s2 = santize.replace_all(&s, "");
            let cleaned_corpus = Regex::new(r#"\s+"#)?.replace_all(&s2, " ");

            let mut word_count: usize = 0;
            let mut sliding_widow = CyclicArray::new(self.order);
            let mut previous_phrase_string: Option<String> = None;

            for word in cleaned_corpus.split_whitespace() {
                if word.is_empty() {
                    continue;
                }
                sliding_widow[word_count] = word.to_string();

                word_count += 1;
                if word_count < self.order {
                    if word.chars().nth_back(0).is_none() {
                        return Err(anyhow!("Word is empty"));
                    }

                    if self
                        .sentence_delimiters
                        .contains(&word.chars().nth_back(0).unwrap())
                    {
                        previous_phrase_string = None;
                        word_count = 0;
                    }
                    continue;
                }

                let phrase = sliding_widow.create_offset_array(word_count);
                let phrase_string = phrase.join(" ");

                if previous_phrase_string.is_none() {
                    self.sentences_starter_phrases.push(phrase_string.clone());
                } else {
                    self.phrase_transitions
                        .entry(previous_phrase_string.clone().unwrap())
                        .and_modify(|e| e.push((phrase_string.clone(), String::from(word))))
                        .or_insert_with(|| vec![(phrase_string.clone(), String::from(word))]);
                }

                previous_phrase_string = Some(phrase_string);

                if self
                    .sentence_delimiters
                    .contains(&word.chars().last().unwrap())
                {
                    previous_phrase_string = None;
                    word_count = 0;
                }
            }

            Ok(())
        }
    }
}
