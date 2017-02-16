use std;
use chess::*;

pub mod output {
    use super::*;

    fn square_char(square: Square) -> char {
        match square.player {
            Player::WHITE => {
                match square.piece {
                    Piece::EMPTY => {' '},
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
                    Piece::EMPTY => {' '},
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
            for x in 0_isize..8_isize {
                let square = board.get(x, y);
                print!("{} ", square_char(square));
            }
            println!("");
        }
    }
}

pub mod input {
    use super::*;

    fn parse_coords(a: &String) -> (isize, isize) {
        assert!(a.len() == 2, "Invalid coord format!");
        let x = (a.chars().nth(0).unwrap() as isize) - ('a' as isize);
        let y = (a.chars().nth(1).unwrap() as isize) - ('1' as isize);
        (x, y)
    }

    pub fn input_move() -> ChessMove {
        let player_str: String;
        let piece_str: String;
        let source_str: String;
        let target_str: String;
        let takes_str: String;

        // print!("your move: ");
        // use std::io::Write;
        // std::io::stdout().flush().unwrap();
        scan!("{} {} {} -> {} takes {}\n", player_str, piece_str, source_str, target_str, takes_str);

        let player: Player;
        if player_str == "white" {
            player = Player::WHITE;
        } else if player_str == "black" {
            player = Player::BLACK;
        } else {
            panic!("Invalid player: {}!", &player_str);
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
            panic!("Inalid piece: {}!", &piece_str);
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
            panic!("Invalid target piece: {}!", &takes_str);
        }

        ChessMove {
            player: player,
            source_position: source_position,
            target_position: target_position,
            piece: piece,
            target_piece: target_piece,
        }
    }
}
