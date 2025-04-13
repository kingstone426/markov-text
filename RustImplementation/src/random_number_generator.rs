use rand::RngCore;
use rand::rngs::ThreadRng;

pub trait RandomNumberGenerator {
    fn next_u32(&mut self) -> u32;
}

#[derive(Default)]
pub struct RandomNumberGeneratorImpl {
    nrg: ThreadRng,
}

impl RandomNumberGenerator for RandomNumberGeneratorImpl {
    fn next_u32(&mut self) -> u32 {
        self.nrg.next_u32()
    }
}
