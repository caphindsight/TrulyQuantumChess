// This is the main source file,
// where the alghorithm which handles
// ordinary and quantum moves and
// ensures that they follow the rules
// of Quantum Chess is implemented.

use chess::*;
use quantize::*;

pub struct QuantumChessEngine {
    pub player: Player,
    state: QuantumChessboard,
}

impl QuantumChessEngine {
    pub fn new() -> QuantumChessEngine {
        QuantumChessEngine {
            player: Player::WHITE,
            state: QuantumChessboard {
                harmonics: vec![
                    QuantumHarmonic {
                        board: Chessboard::new_starting(),
                        degeneracy: 1,
                    }
                ]
            }
        }
    }

    pub fn get_quantum_chessboard(&self) -> QuantumChessboard {
        self.state.clone()
    }

    pub fn submit(&mut self, mv: &QuantumChessMove) -> ChessMoveResult {
        match *mv {
            QuantumChessMove::Ordinary(ref omv) => self.submit_move(omv),
            QuantumChessMove::Quantum(ref qmv) => self.submit_quantum_move(qmv),
        }
    }

    pub fn submit_move(&mut self, mv: &ChessMove) -> ChessMoveResult {
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
                if harmonic.board.get(sx, sy) == source.square && harmonic.board.allowed(mv) {
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
            if fcmp::eq(source.probability, 1.0) {
                // The player's piece is present at the source position of the move
                // on all chessboard harmonics. The measurement doesn't occur.
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
                        format!("Capture is unavailable on all harmonics of the quantum chessboard!")
                    )
                }
            } else if fcmp::ne(source.probability, 0.0) {
                // The player's piece is in a superposition. The measurement will occur
                // in order to decide whether the opponent's piece is captured or not.
                // This measurement can produce two distinctive outcomes:
                // 1. A player's piece might be found absent on the source position of the move.
                // In this case a capture doesn't occur, and all the harmonics with the player's
                // piece at the source position are removed from the superposition.
                // 2. A player's piece might be found present on the soure position. In this case
                // the capture occurs. All the chessboard harmonics with the player's piece on
                // other positions are removed. On all haronics a move/capture is played, meaning
                // that the opponent's piece is now in superposition of being captured and still
                // alive on another square.

                let mut available = false;
                for harmonic in &mut self.state.harmonics {
                    if harmonic.board.allowed(mv) {
                        available = true;
                    }
                }

                if !available {
                    return ChessMoveResult::Failure(
                        format!("Capture is unavailable on all harmonics of the quantum chessboard!")
                    );
                }

                let capture_occurs = measure::decide(source.probability);
                if capture_occurs {
                    // Case #2 from the comment above.
                    // Removing all harmonics without the player's piece on the source position.
                    self.state.clean(|h: &QuantumHarmonic| {
                        let sq = h.board.get(sx, sy);
                        h.board.get(sx, sy).is_occupied()
                    });

                    // On all remanining harmonics, we play the move. It can be a capture or
                    // a simple move, depending on the harmonic.
                    for harmonic in &mut self.state.harmonics {
                        if harmonic.board.allowed(mv) {
                            harmonic.board.apply(mv);
                        }
                    }
                } else {
                    // Case #1 from the comment above.
                    // Removing all harmonics with the player's piece on the source position.
                    self.state.clean(|h: &QuantumHarmonic| {
                        !h.board.get(sx, sy).is_occupied()
                    });
                    // That's all! Capture has failed, the player has lost his turn to act.
                }

                ChessMoveResult::Success
            } else {
                panic!("Inconsistent quantum chessboard: probability of player's piece is zero!");
            }
        } else {
            ChessMoveResult::Failure(
                format!("Target position ({}, {}) of the move is occupied by a friendly piece ({:?})!", tx, ty, target.square.player)
            )
        }
    }

    pub fn submit_quantum_move(&mut self, qmv: &QuantumMove) -> ChessMoveResult {
        if self.player != qmv.player {
            return ChessMoveResult::Failure(
                format!("Invalid player; expected {:?}, got {:?}!", self.player, qmv.player)
            );
        }

        let (sx, sy) = qmv.source_position;
        let source = self.state.get_quantum_square_info(sx, sy);
        let (mx, my) = qmv.middle_position;
        let middle = self.state.get_quantum_square_info(mx, my);
        let (tx, ty) = qmv.target_position;
        let target = self.state.get_quantum_square_info(tx, ty);

        if !source.square.is_occupied() {
            return ChessMoveResult::Failure(
                format!("Source position ({}, {}) of the move isn't occupied!", sx, sy)
            );
        }

        if source.square.piece != qmv.piece {
            return ChessMoveResult::Failure(
                format!("Invalid piece; expected {:?}, got {:?}!", source.square.piece, qmv.piece)
            );
        }

        if source.square.player != qmv.player {
            return ChessMoveResult::Failure(
                format!("Source position ({}, {}) of the move is occupied by an enemy ({:?}) piece!", sx, sy, source.square.player)
            );
        }

        if target.square.piece != Piece::EMPTY {
            return ChessMoveResult::Failure(
                format!("Invalid target piece; expected {:?}, got {:?}!", target.square.piece, Piece::EMPTY)
            );
        }

        let first_move = ChessMove {
            player: qmv.player,
            source_position: qmv.source_position,
            target_position: qmv.middle_position,
            piece: qmv.piece,
            target_piece: Piece::EMPTY,
        };

        let second_move = ChessMove {
            player: qmv.player,
            source_position: qmv.middle_position,
            target_position: qmv.target_position,
            piece: qmv.piece,
            target_piece: Piece::EMPTY,
        };

        if qmv.source_position == qmv.target_position {
            return ChessMoveResult::Failure(
                format!("Trivial quantum moves are forbidden by the rules of the game")
            );
        }

        if (!target.square.is_occupied()) {
            let mut available = false;
            let mut new_harmonics = Vec::<QuantumHarmonic>::with_capacity(self.state.harmonics.len() * 2);
            for harmonic in &mut self.state.harmonics {
                if first_move.is_trivial() || harmonic.board.allowed(&first_move) {
                    if second_move.is_trivial() || harmonic.board.allowed(&second_move) {
                        let middle_occupied = harmonic.board.get(mx, my).is_occupied();
                        if first_move.is_trivial() || second_move.is_trivial() || !middle_occupied {
                            available = true;
                            let mut new_harmonic = harmonic.clone();
                            new_harmonic.board.apply(&first_move);
                            new_harmonic.board.apply(&second_move);
                            new_harmonics.push(harmonic.clone());
                            new_harmonics.push(new_harmonic);
                            continue;
                        }
                    }
                }
                let mut new_harmonic = harmonic.clone();
                new_harmonic.degeneracy = new_harmonic.degeneracy * 2;
                new_harmonics.push(new_harmonic);
            }
            if available {
                self.state.harmonics = new_harmonics;
                ChessMoveResult::Success
            } else {
                ChessMoveResult::Failure(
                    format!("Quantum move is unavailable on all harmonics of the quantum chessboard!")
                )
            }
        } else if (target.square.player != qmv.player) {
            ChessMoveResult::Failure(
                format!("You can't quantum-capture!")
            )
        } else {
            ChessMoveResult::Failure(
                format!("Target position ({}, {}) of the move is occupied by a friendly piece ({:?})!", tx, ty, target.square.player)
            )
        }
    }
}
