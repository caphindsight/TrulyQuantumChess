// This is the main source file,
// where the alghorithm which handles
// ordinary and quantum moves and
// ensures that they follow the rules
// of Quantum Chess is implemented.

use chess::*;
use quantize::*;

/// Represents a chess move
pub struct ChessMove {
    pub player: Player,
    pub source_position: (usize, usize),
    pub target_position: (usize, usize),
    pub piece: Piece,
    pub target_piece: Piece,  // Piece::EMPTY for non-take moves
}

/// Represents a result of the chess move
pub enum ChessMoveResult {
    Success,
    Failure(String),
}

/// This simple trait provides basic interface into
/// an object capable of handling chess moves
pub trait ChessEngine {
    fn submit_move(&mut self, mv: &ChessMove) -> ChessMoveResult;
}

/// Interface into an object capable of handling
/// quantum chess moves
pub trait QuantumChessEngine: ChessEngine {
    fn submit_quantum_move(&mut self, mv: &ChessMove) -> ChessMoveResult;
}

pub struct QuantumChessEngineImpl {
    player: Player,
    state: QuantumChessboard,
}

impl QuantumChessEngineImpl {
    pub fn new() -> QuantumChessEngineImpl {
        QuantumChessEngineImpl {
            player: Player::WHITE,
            state: QuantumChessboard {
                harmonics: vec![
                    QuantumHarmonic {
                        board: Chessboard::new_starting(),
                        ampl: Comp::new(1.0, 0.0),
                    }
                ]
            }
        }
    }

    pub fn get_quantum_chessboard(&self) -> QuantumChessboard {
        self.state.clone()
    }
}

impl ChessEngine for QuantumChessEngineImpl {
    fn submit_move(&mut self, mv: &ChessMove) -> ChessMoveResult {
        panic!("Not implemented yet");
    }
}

impl QuantumChessEngine for QuantumChessEngineImpl {
    fn submit_quantum_move(&mut self, mv: &ChessMove) -> ChessMoveResult {
        panic!("Not implemented yet");
    }
}
