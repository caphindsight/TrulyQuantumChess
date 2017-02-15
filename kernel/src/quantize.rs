use chess::*;
use num::complex::Complex64;

type Comp = Complex64;

/// Represents a quantum harmonic, dominated by a
/// particular classical configuration (chessboard).
struct QuantumHarmonic {
    board: Chessboard,
    ampl: Comp,
}

/// Represents an arbitrary superposition of harmonics
/// each given an arbitrary complex amplitude.
struct QuantumState {
    harmonics: Vec<QuantumHarmonic>,
}
