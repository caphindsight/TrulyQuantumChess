use chess::*;
use num::complex::Complex64;

pub type Comp = Complex64;
pub const EPS: f64 = 1e-10;

/// Represents a quantum harmonic, dominated by a
/// particular classical configuration (chessboard).
#[derive(Clone)]
pub struct QuantumHarmonic {
    pub board: Chessboard,
    pub ampl: Comp,
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
    /// Removes harmonics with vanishing amplitudes from the list.
    pub fn clean_vanishing(&mut self) {
        let n = self.harmonics.len();
        let mut new_harmonics = Vec::<QuantumHarmonic>::with_capacity(n);
        let mut exchange_needed = false;
        for i in 0..n {
            let ref harmonic = self.harmonics[i];
            if harmonic.ampl.norm_sqr() > EPS {
                new_harmonics.push(harmonic.clone());
                exchange_needed = true;
            }
        }
        if exchange_needed {
            self.harmonics = new_harmonics;
        }
    }

    /// Gets the information about a particular square
    /// of the quantum board. At an instance of time, only
    /// one piece is allowed to occupy the square. This function
    /// returns the piece that maybe occupies square, together with
    /// the current probability of it.
    pub fn get_quantum_square_info(&self, x: usize, y: usize) -> QuantumSquareInfo {
        let mut res: Option<Square> = None;
        let mut ampl = Comp::new(0.0, 0.0);
        let mut total = Comp::new(0.0, 0.0);
        for harmonic in &self.harmonics {
            total = total + harmonic.ampl;
            let current_sq = harmonic.board.get(x, y);
            if current_sq.is_occupied() {
                ampl = ampl + harmonic.ampl;
                if let Some(expected_sq) = res {
                    if current_sq != expected_sq {
                        panic!("Inconsistent quantum board: square ({}, {}) is in the superposition of pieces!", x, y);
                    }
                }
                res = Some(current_sq.clone());
            }
        }
        match res {
            Some(sq) => QuantumSquareInfo {
                square: sq,
                probability: ampl.norm_sqr() / total.norm_sqr(),
            },
            None => QuantumSquareInfo {
                square: EMPTY_SQUARE,
                probability: 1.0,
            }
        }
    }
}
