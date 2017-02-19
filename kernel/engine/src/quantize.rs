use chess::*;
use rand;

pub const EPS: f64 = 1e-10;

/// Compares 64-bit floats with precision given by `EPS`.
pub mod fcmp {
    use super::EPS;

    pub fn eq(a: f64, b: f64) -> bool {
        (a - b).abs() <= EPS
    }

    pub fn ne(a: f64, b: f64) -> bool {
        (a - b).abs() > EPS
    }

    pub fn lt(a: f64, b: f64) -> bool {
        a < b - EPS
    }

    pub fn gt(a: f64, b: f64) -> bool {
        a > b + EPS
    }

    pub fn le(a: f64, b: f64) -> bool {
        a <= b + EPS
    }

    pub fn ge(a: f64, b: f64) -> bool {
        a >= b - EPS
    }
}

// Measurement utils, i.e. random decision making
pub mod measure {
    use super::rand;

    pub fn decide(probability: f64) -> bool {
        let range = rand::distributions::Range::new(0.0, 1.0);
        let mut rng = rand::thread_rng();
        use rand::distributions::IndependentSample;
        let probe = range.ind_sample(&mut rng);
        let res = probe <= probability;
        println!("Measurement with probability {} rendered {}", probability, res);
        res
    }
}

/// Represents a quantum harmonic, dominated by a
/// particular classical configuration (chessboard).
#[derive(Clone)]
pub struct QuantumHarmonic {
    pub board: Chessboard,
    pub degeneracy: i64,  // The number of times the harmonic is present in the superposition
}

/// Represents an arbitrary superposition of harmonics
/// each given an arbitrary complex amplitude.
#[derive(Clone)]
pub struct QuantumChessboard {
    pub harmonics: Vec<QuantumHarmonic>,
}

/// A piece which (probably) occupies the square,
/// along with the probability of occupation.
/// Remember: a square isn't allowed to be in a
/// superposition of different pieces.
pub struct QuantumSquareInfo {
    pub square: Square,
    pub probability: f64,
}


impl QuantumChessboard {
    /// Removes harmonics which don't pass the specified criteria
    pub fn clean<F>(&mut self, f: F)
        where F: Fn(&QuantumHarmonic) -> bool
    {
        let n = self.harmonics.len();
        let mut new_harmonics = Vec::<QuantumHarmonic>::with_capacity(n);
        for i in 0..n {
            let ref harmonic = self.harmonics[i];
            if f(harmonic) {
                new_harmonics.push(harmonic.clone());
            }
        }
        self.harmonics = new_harmonics;
    }

    fn gcd(a: i64, b: i64) -> i64 {
        if a == 0 || b == 0 {
            a + b
        } else if (a == b) {
            a
        } else if (a > b) {
            QuantumChessboard::gcd(b, a % b)
        } else {
            QuantumChessboard::gcd(a, b % a)
        }
    }

    /// Performs measurements for each square in a superposition of pieces
    pub fn perform_measurements(&mut self) {
        let n = self.harmonics.len();
        for i in 0_usize..64_usize {
            let (x, y) = Chessboard::unindex(i);
            let mut square1 = EMPTY_SQUARE;
            let mut degeneracy1 = 0_i64;
            let mut square2 = EMPTY_SQUARE;
            let mut degeneracy2 = 0_i64;
            for harmonic in &self.harmonics {
                let square = harmonic.board.get(x, y);
                if square.is_occupied() {
                    if !square1.is_occupied() {
                        square1 = square;
                        degeneracy1 = 1;
                    } else if square1 == square {
                        degeneracy1 += 1;
                    } else if !square2.is_occupied() {
                        square2 = square;
                        degeneracy2 = 1;
                    } else if square == square {
                        degeneracy2 += 1;
                    } else {
                        panic!("Too much of an inconsistency found on the quantum chessboard: the square ({}, {}) appears to be in a superposition of more than two pieces!", x, y);
                    }
                }
            }
            if square1.is_occupied() && square2.is_occupied() {
                let prob1 = (degeneracy1 as f64) / (n as f64);
                assert!(fcmp::gt(prob1, 0.0) && fcmp::lt(prob1, 1.0), "Probability was calculated inconsistently");

                let prob2 = (degeneracy2 as f64) / (n as f64);
                assert!(fcmp::gt(prob2, 0.0) && fcmp::lt(prob2, 1.0), "Probability was calculated inconsistently");

                let prob0 = 1.0 - prob1 - prob2;
                assert!(fcmp::ge(prob0, 0.0) && fcmp::lt(prob1, 1.0), "Probability was calculated inconsistently");

                let prob_relative = prob1 / (prob1 + prob2);
                if measure::decide(prob_relative) {
                    self.clean(|h| {
                        h.board.get(x, y) != square2
                    });
                } else {
                    self.clean(|h| {
                        h.board.get(x, y) != square1
                    });
                }
            }
        }
    }

    /// Groups together similar harmonics, adding degeneracies.
    pub fn regroup(&mut self) {
        let n = self.harmonics.len();
        let mut new_harmonics = Vec::<QuantumHarmonic>::with_capacity(n);
        self.harmonics.sort_by_key(|qh| qh.board.clone());
        {
            let mut prev_harmonic = &self.harmonics[0];
            let mut degeneracy = prev_harmonic.degeneracy;
            for i in 1..n {
                let current_harmonic = &self.harmonics[i];
                if current_harmonic.board != prev_harmonic.board {
                    new_harmonics.push(QuantumHarmonic{
                        board: prev_harmonic.board.clone(),
                        degeneracy: degeneracy,
                    });
                    prev_harmonic = current_harmonic;
                    degeneracy = current_harmonic.degeneracy;
                } else {
                    degeneracy += current_harmonic.degeneracy;
                }
            }
            new_harmonics.push(QuantumHarmonic{
                board: prev_harmonic.board.clone(),
                degeneracy: degeneracy,
            });
        }
        if self.harmonics.len() != new_harmonics.len() {
            self.harmonics = new_harmonics;
        }
    }

    /// Divides all degeneracies by their GCD and sorts in reversed order.
    pub fn normalize_degeneracy(&mut self) {
        let mut gcd = 0;
        self.harmonics.sort_by_key(|h| -h.degeneracy);
        for harmonic in &self.harmonics {
            gcd = QuantumChessboard::gcd(gcd, harmonic.degeneracy);
        }
        for harmonic in &mut self.harmonics {
            harmonic.degeneracy = harmonic.degeneracy / gcd;
        }
    }

    /// Gets the information about a particular square
    /// of the quantum board. At an instance of time, only
    /// one piece is allowed to occupy the square. This function
    /// returns the piece that maybe occupies square, together with
    /// the current probability of it.
    pub fn get_quantum_square_info(&self, x: isize, y: isize) -> QuantumSquareInfo {
        let mut res: Option<Square> = None;
        let mut numerator: f64 = 0.0;
        let mut denominator: f64 = 0.0;
        for harmonic in &self.harmonics {
            denominator = denominator + harmonic.degeneracy as f64;
            let current_sq = harmonic.board.get(x, y);
            if current_sq.is_occupied() {
                numerator = numerator + harmonic.degeneracy as f64;
                if let Some(expected_sq) = res {
                    if current_sq != expected_sq {
                        panic!("Inconsistent quantum chessboard: square ({}, {}) is in the superposition of pieces!", x, y);
                    }
                }
                res = Some(current_sq.clone());
            }
        }
        match res {
            Some(sq) => QuantumSquareInfo {
                square: sq,
                probability: numerator / denominator,
            },
            None => QuantumSquareInfo {
                square: EMPTY_SQUARE,
                probability: 1.0,
            }
        }
    }
}
