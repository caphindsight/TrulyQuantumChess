// This is the main source file,
// where the alghorithm which handles
// ordinary and quantum moves and
// ensures that they follow the rules
// of Quantum Chess is implemented.

use chess::*;
use quantize::*;

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
        if self.player != mv.player {
            return ChessMoveResult::Failure(
                format!("Invalid player; expected {:?}, got {:?}!", self.player, mv.player)
            );
        }

        let (sx, sy) = mv.source_position;
        let source = self.state.get_quantum_square_info(sx, sy);
        let (tx, ty) = mv.target_position;
        let target = self.state.get_quantum_square_info(tx, ty);

        if !source.square.is_occupied() {
            return ChessMoveResult::Failure(
                format!("Source position ({}, {}) of the move isn't occupied!", sx, sy)
            );
        }

        if source.square.piece != mv.piece {
            return ChessMoveResult::Failure(
                format!("Invalid piece; expected {:?}, got {:?}!", source.square.piece, mv.piece)
            );
        }

        if source.square.player != mv.player {
            return ChessMoveResult::Failure(
                format!("Source position ({}, {}) of the move is occupied by an enemy ({:?}) piece!", sx, sy, source.square.player)
            );
        }

        if target.square.piece != mv.target_piece {
            return ChessMoveResult::Failure(
                format!("Invalid target piece; expected {:?}, got {:?}!", target.square.piece, mv.target_piece)
            );
        }

        if (!target.square.is_occupied()) {
            let mut available = false;
            for harmonic in &mut self.state.harmonics {
                if harmonic.board.allowed(mv) {
                    available = true;
                    harmonic.board.apply(mv);
                }
            }
            if available {
                ChessMoveResult::Success
            } else {
                ChessMoveResult::Failure(
                    format!("Move is unavailable on all harmonics of the quantum chessboard!")
                )
            }
        } else if (target.square.player != mv.player) {
            ChessMoveResult::Failure(
                format!("You can't take pieces... yet :)")
            )
        } else {
            ChessMoveResult::Failure(
                format!("Target position ({}, {}) of the move is occupied by a friendly piece ({:?})!", tx, ty, target.square.player)
            )
        }
    }
}

impl QuantumChessEngine for QuantumChessEngineImpl {
    fn submit_quantum_move(&mut self, mv: &ChessMove) -> ChessMoveResult {
        panic!("Not implemented yet");
    }
}
