use regex;
use std;
use chess::*;

pub mod output {
    use super::*;

    fn square_char(square: Square) -> char {
        match square.player {
            Player::WHITE => {
                match square.piece {
                    Piece::EMPTY => {'.'},
                    Piece::PAWN => {'\u{2659}'},
                    Piece::KNIGHT => {'\u{2658}'},
                    Piece::BISHOP => {'\u{2657}'},
                    Piece::ROOK => {'\u{2656}'},
                    Piece::QUEEN => {'\u{2655}'},
                    Piece::KING => {'\u{2654}'},
                }
            },
            Player::BLACK => {
                match square.piece {
                    Piece::EMPTY => {'.'},
                    Piece::PAWN => {'\u{265F}'},
                    Piece::KNIGHT => {'\u{265E}'},
                    Piece::BISHOP => {'\u{265D}'},
                    Piece::ROOK => {'\u{265C}'},
                    Piece::QUEEN => {'\u{265B}'},
                    Piece::KING => {'\u{265A}'},
                }
            }
        }
    }

    pub fn display_chessboard(board: &Chessboard) {
        for y in (0_isize..8_isize).rev() {
            print!("{} ", y + 1);
            for x in 0_isize..8_isize {
                let square = board.get(x, y);
                print!("{} ", square_char(square));
            }
            println!("");
        }
        println!("  a b c d e f g h");
    }
}

pub mod input {
    use super::*;

    fn parse_coords(a_str: &str) -> (isize, isize) {
        let a = a_str.to_string();
        assert!(a.len() == 2, "Invalid coord format!");
        let x = (a.chars().nth(0).unwrap() as isize) - ('a' as isize);
        let y = (a.chars().nth(1).unwrap() as isize) - ('1' as isize);
        (x, y)
    }

    pub fn read_line() -> String {
        use std::io::BufRead;
        let res: String;
        {
            let stdin = std::io::stdin();
            res = stdin.lock().lines().next().unwrap().unwrap().to_string();
        }
        res
    }

    pub fn input_move() -> Option<QuantumChessMove> {
        let ordinary_re = regex::Regex::new(r"^(white|black) (pawn|knight|bishop|rook|queen|king) ([a-h][1-8]) -> ([a-h][1-8]) takes (nothing|pawn|knight|bishop|rook|queen|king)$").unwrap();
        let quantum_re = regex::Regex::new(r"^(white|black) (pawn|knight|bishop|rook|queen|king) ([a-h][1-8]) -> ([a-h][1-8]) -> ([a-h][1-8])$").unwrap();

        let line = read_line();
        if ordinary_re.is_match(&line) {
            let mut iter = ordinary_re.captures_iter(&line);
            let cap = iter.next().unwrap();
            match parse_ordinary_move(&cap[1], &cap[2], &cap[3], &cap[4], &cap[5]) {
                None => None,
                Some(m) => Some(QuantumChessMove::Ordinary(m)),
            }
        } else if quantum_re.is_match(&line) {
            let mut iter = quantum_re.captures_iter(&line);
            let cap = iter.next().unwrap();
            match parse_quantum_move(&cap[1], &cap[2], &cap[3], &cap[4], &cap[5]) {
                None => None,
                Some(m) => Some(QuantumChessMove::Quantum(m)),
            }
        } else {
            None
        }
    }

    pub fn parse_ordinary_move(player_str: &str, piece_str: &str, source_str: &str,
        target_str: &str, takes_str: &str) -> Option<ChessMove>
    {
        let player: Player;
        if player_str == "white" {
            player = Player::WHITE;
        } else if player_str == "black" {
            player = Player::BLACK;
        } else {
            return None;
        }

        let piece: Piece;
        if piece_str == "pawn" {
            piece = Piece::PAWN;
        } else if (piece_str == "knight") {
            piece = Piece::KNIGHT;
        } else if (piece_str == "bishop") {
            piece = Piece::BISHOP;
        } else if (piece_str == "rook") {
            piece = Piece::ROOK;
        } else if (piece_str == "queen") {
            piece = Piece::QUEEN;
        } else if (piece_str == "king") {
            piece = Piece::KING;
        } else {
            return None;
        }

        let source_position = parse_coords(&source_str);
        let target_position = parse_coords(&target_str);

        let target_piece: Piece;
        if (takes_str == "nothing") {
            target_piece = Piece::EMPTY;
        } else if (takes_str == "pawn") {
            target_piece = Piece::PAWN;
        } else if (takes_str == "knight") {
            target_piece = Piece::KNIGHT;
        } else if (takes_str == "bishop") {
            target_piece = Piece::BISHOP;
        } else if (takes_str == "rook") {
            target_piece = Piece::ROOK;
        } else if (takes_str == "queen") {
            target_piece = Piece::QUEEN;
        } else if (takes_str == "king") {
            target_piece = Piece::KING;
        } else {
            return None;
        }

        Some(ChessMove {
            player: player,
            source_position: source_position,
            target_position: target_position,
            piece: piece,
            target_piece: target_piece,
        })
    }

    pub fn parse_quantum_move(player_str: &str, piece_str: &str, source_str: &str,
        middle_str: &str, target_str: &str) -> Option<QuantumMove>
    {
        let player: Player;
        if player_str == "white" {
            player = Player::WHITE;
        } else if player_str == "black" {
            player = Player::BLACK;
        } else {
            return None;
        }

        let piece: Piece;
        if piece_str == "pawn" {
            piece = Piece::PAWN;
        } else if (piece_str == "knight") {
            piece = Piece::KNIGHT;
        } else if (piece_str == "bishop") {
            piece = Piece::BISHOP;
        } else if (piece_str == "rook") {
            piece = Piece::ROOK;
        } else if (piece_str == "queen") {
            piece = Piece::QUEEN;
        } else if (piece_str == "king") {
            piece = Piece::KING;
        } else {
            return None;
        }

        let source_position = parse_coords(&source_str);
        let middle_position = parse_coords(&middle_str);
        let target_position = parse_coords(&target_str);

        Some(QuantumMove {
            player: player,
            piece: piece,
            source_position: source_position,
            middle_position: middle_position,
            target_position: target_position,
        })
    }
}
