pub mod markov {

    /// A cyclic array that wraps around when accessing elements beyond its bounds
    pub struct CyclicArray<T> {
        data: Vec<T>,
    }

    impl<T> CyclicArray<T> {
        /// Creates a new CyclicArray with the specified size
        ///
        /// # Arguments
        /// * `size` - The size of the cyclic array
        ///
        /// # Panics
        /// Panics if size is zero or negative
        pub fn new(size: usize) -> Self {
            if size == 0 {
                panic!("Size must be greater than zero");
            }

            // Create vector with capacity but don't initialize elements since we don't have a Default bound
            let mut data = Vec::with_capacity(size);
            // This makes the vector have the right length with uninitialized values
            unsafe {
                data.set_len(size);
            }

            CyclicArray { data }
        }

        /// Calculate the wrapped index
        fn index(&self, index: usize) -> usize {
            let len = self.data.len();
            let mut adjusted_index: usize = index;

            // Handle negative indices by wrapping around
            while adjusted_index < 0 {
                adjusted_index += len;
            }

            adjusted_index % self.data.len()
        }

        /// Creates a new array with elements offset by the given amount
        pub fn create_offset_array(&self, offset: usize) -> Vec<T>
        where
            T: Clone,
        {
            let mut result = Vec::with_capacity(self.data.len());

            for i in 0..self.data.len() {
                let idx = self.index(i + offset);
                result.push(self.data[idx].clone());
            }

            result
        }
    }

    // Implement indexing for CyclicArray using isize for the index
    impl<T> std::ops::Index<usize> for CyclicArray<T> {
        type Output = T;

        fn index(&self, index: usize) -> &Self::Output {
            &self.data[self.index(index)]
        }
    }

    // Implement mutable indexing for CyclicArray
    impl<T> std::ops::IndexMut<usize> for CyclicArray<T> {
        fn index_mut(&mut self, index: usize) -> &mut Self::Output {
            let idx = self.index(index);
            &mut self.data[idx]
        }
    }

    impl<T> From<Vec<T>> for CyclicArray<T> {
        fn from(data: Vec<T>) -> Self {
            if data.is_empty() {
                panic!("Cannot create CyclicArray from empty vector");
            }
            CyclicArray { data }
        }
    }
}
