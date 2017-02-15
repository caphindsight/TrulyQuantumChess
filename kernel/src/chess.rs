#[derive(Copy, Clone, Eq, PartialEq)]
pub enum Player {
    WHITE,
    BLACK,
}

#[derive(Copy, Clone, Eq, PartialEq)]
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
    player: Player,
    piece: Piece,
}

pub const EMPTY_SQUARE: Square = Square {player: Player::WHITE, piece: Piece::EMPTY};

/// Represents the state of the chessboard
pub struct Chessboard {
    squares: [Square; 64],
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

    fn validate_coord(coord: usize) {
        if coord >= 8 {
            panic!("Chessboard coordinate out of range: {}", &coord);
        }
    }

    pub fn index(x: usize, y: usize) -> usize {
        Chessboard::validate_coord(x);
        Chessboard::validate_coord(y);
        y * 8 + x
    }

    pub fn unindex(ind: usize) -> (usize, usize) {
        if ind >= 64 {
            panic!("Chessboard index out of range: {}", &ind);
        }
        (ind % 8, ind / 8)
    }

    pub fn get(&self, x: usize, y: usize) -> Square {
        let ind = Chessboard::index(x, y);
        self.squares[ind]
    }

    pub fn set(&mut self, x: usize, y: usize, square: Square) {
        let ind = Chessboard::index(x, y);
        self.squares[ind] = square;
    }
}
