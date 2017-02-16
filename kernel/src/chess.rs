use std;

#[derive(Copy, Clone, Debug, Eq, PartialEq)]
pub enum Player {
    WHITE,
    BLACK,
}

impl Player {
    pub fn switch(&mut self) {
        if self == &Player::WHITE {
            *self = Player::BLACK;
        } else {
            *self = Player::WHITE;
        }
    }
}

#[derive(Copy, Clone, Debug, Eq, PartialEq)]
pub enum Piece {
    EMPTY,
    PAWN,
    KNIGHT,
    BISHOP,
    ROOK,
    QUEEN,
    KING,
}

#[derive(Copy, Clone, Eq, PartialEq)]
pub struct Square {
    pub player: Player,
    pub piece: Piece,
}

impl Square {
    pub fn is_occupied(&self) -> bool {
        self.piece != Piece::EMPTY
    }
}

pub const EMPTY_SQUARE: Square = Square {player: Player::WHITE, piece: Piece::EMPTY};

/// Represents the state of the chessboard
pub struct Chessboard {
    squares: [Square; 64],
}

impl Clone for Chessboard {
    fn clone(&self) -> Chessboard {
        let mut new_squares = [EMPTY_SQUARE; 64];
        for i in 0..64 {
            new_squares[i] = self.squares[i];
        }
        Chessboard {squares: new_squares}
    }
}

/// Represents a chess move
#[derive(Debug)]
pub struct ChessMove {
    pub player: Player,
    pub source_position: (isize, isize),
    pub target_position: (isize, isize),
    pub piece: Piece,
    pub target_piece: Piece,  // Piece::EMPTY for non-take moves
}

impl ChessMove {
    pub fn is_trivial(&self) -> bool {
        self.source_position == self.target_position
    }
}

/// Represents a quantum move
#[derive(Debug)]
pub struct QuantumMove {
    pub player: Player,
    pub source_position: (isize, isize),
    pub middle_position: (isize, isize),
    pub target_position: (isize, isize),
    pub piece: Piece,
}

/// Represents any move player can make in quantum chess
#[derive(Debug)]
pub enum QuantumChessMove {
    Ordinary(ChessMove),
    Quantum(QuantumMove),
}

/// Represents a result of the chess move
pub enum ChessMoveResult {
    Success,
    Failure(String),
}


impl Chessboard {
    pub fn new_empty() -> Chessboard {
        Chessboard {squares: [EMPTY_SQUARE; 64]}
    }

    pub fn new_starting() -> Chessboard {
        let mut board = Chessboard::new_empty();

        // Setting up pawns
        for i in 0..8 {
            board.set(i, 1, Square {player: Player::WHITE, piece: Piece::PAWN});
            board.set(i, 6, Square {player: Player::BLACK, piece: Piece::PAWN});
        }

        // Setting up power pieces
        board.set(0, 0, Square {player: Player::WHITE, piece: Piece::ROOK});
        board.set(0, 7, Square {player: Player::BLACK, piece: Piece::ROOK});
        board.set(1, 0, Square {player: Player::WHITE, piece: Piece::KNIGHT});
        board.set(1, 7, Square {player: Player::BLACK, piece: Piece::KNIGHT});
        board.set(2, 0, Square {player: Player::WHITE, piece: Piece::BISHOP});
        board.set(2, 7, Square {player: Player::BLACK, piece: Piece::BISHOP});
        board.set(3, 0, Square {player: Player::WHITE, piece: Piece::QUEEN});
        board.set(3, 7, Square {player: Player::BLACK, piece: Piece::QUEEN});
        board.set(4, 0, Square {player: Player::WHITE, piece: Piece::KING});
        board.set(4, 7, Square {player: Player::BLACK, piece: Piece::KING});
        board.set(5, 0, Square {player: Player::WHITE, piece: Piece::BISHOP});
        board.set(5, 7, Square {player: Player::BLACK, piece: Piece::BISHOP});
        board.set(6, 0, Square {player: Player::WHITE, piece: Piece::KNIGHT});
        board.set(6, 7, Square {player: Player::BLACK, piece: Piece::KNIGHT});
        board.set(7, 0, Square {player: Player::WHITE, piece: Piece::ROOK});
        board.set(7, 7, Square {player: Player::BLACK, piece: Piece::ROOK});

        // Chessboard is now set up, returning
        board
    }

    fn validate_coord(coord: isize) {
        if coord < 0 || coord >= 8 {
            panic!("Chessboard coordinate out of range: {}", &coord);
        }
    }

    pub fn index(x: isize, y: isize) -> usize {
        Chessboard::validate_coord(x);
        Chessboard::validate_coord(y);
        (y * 8 + x) as usize
    }

    pub fn unindex(ind: usize) -> (isize, isize) {
        if ind >= 64 {
            panic!("Chessboard index out of range: {}", &ind);
        }
        ((ind % 8) as isize, (ind / 8) as isize)
    }

    pub fn get(&self, x: isize, y: isize) -> Square {
        let ind = Chessboard::index(x, y);
        self.squares[ind]
    }

    pub fn set(&mut self, x: isize, y: isize, square: Square) {
        let ind = Chessboard::index(x, y);
        self.squares[ind] = square;
    }

    /// Checks whether a specified move is allowed by rules of ordinary chess.
    pub fn allowed(&self, mv: &ChessMove) -> bool {
        let (sx, sy) = mv.source_position;
        let (tx, ty) = mv.target_position;

        if sx == tx && sy == ty {
            return false;
        }

        let dx = tx - sx;
        let dx_abs = dx.abs();
        let dx_sig = dx.signum();
        let dy = ty - sy;
        let dy_abs = dy.abs();
        let dy_sig = dy.signum();

        match mv.piece {
            Piece::EMPTY => { panic!("You can't make a move with an empty piece!"); },
            Piece::PAWN => {
                match mv.player {
                    Player::WHITE => {
                        if mv.target_piece == Piece::EMPTY {
                            sx == tx && (sy + 1 == ty || sy == 1 && ty == 3 && !self.get(sx, 2).is_occupied())
                        } else {
                            (sx == tx + 1 || sx + 1 == tx) && (sy + 1 == ty)
                        }
                    },
                    Player::BLACK => {
                        if mv.target_piece == Piece::EMPTY {
                            sx == tx && (sy == ty + 1 || sy == 6 && ty == 4 && !self.get(sx, 5).is_occupied())
                        } else {
                            (sx == tx + 1 || sx + 1 == tx) && (sy == ty + 1)
                        }
                    }
                }
            },
            Piece::KNIGHT => {
                dx_abs == 1 && dy_abs == 2 || dx_abs == 2 && dy_abs == 1
            },
            Piece::BISHOP => {
                if dx_abs == dy_abs {
                    for i in 1..dx_abs {
                        if self.get(sx + i * dx_sig, sy + i * dy_sig).is_occupied() {
                            return false;
                        }
                    }
                    true
                } else {
                    false
                }
            },
            Piece::ROOK => {
                if dx == 0 || dy == 0 {
                    let dd = std::cmp::max(dx_abs, dy_abs);
                    for i in 1..dd {
                        if self.get(sx + i * dx_sig, sy + i * dy_sig).is_occupied() {
                            return false;
                        }
                    }
                    true
                } else {
                    false
                }
            },
            Piece::QUEEN => {
                if dx == 0 || dy == 0 {
                    let dd = std::cmp::max(dx_abs, dy_abs);
                    for i in 1..dd {
                        if self.get(sx + i * dx_sig, sy + i * dy_sig).is_occupied() {
                            return false;
                        }
                    }
                    true
                } else if dx_abs == dy_abs {
                    for i in 1..dx_abs {
                        if self.get(sx + i * dx_sig, sy + i * dy_sig).is_occupied() {
                            return false;
                        }
                    }
                    true
                } else {
                    false
                }
            },
            Piece::KING => {
                (dx_abs == 0 || dx_abs == 1) && (dy_abs == 0 || dy_abs == 1)
            }
        }
    }

    /// Will happily apply a move disallowed by the rules of the game. Be careful!
    /// Ideally one should only call this after checking that `self.allowed(mv) == true`.
    pub fn apply(&mut self, mv: &ChessMove) {
        let (sx, sy) = mv.source_position;
        let (tx, ty) = mv.target_position;
        self.set(tx, ty, Square {player: mv.player, piece: mv.piece});
        self.set(sx, sy, EMPTY_SQUARE);
    }
}
