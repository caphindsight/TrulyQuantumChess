use chess::*;
use num::complex::Complex64;

pub type Comp = Complex64;

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
