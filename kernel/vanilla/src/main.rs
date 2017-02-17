extern crate regex;

extern crate engine;
use engine::chess::*;
use engine::quantize::*;
use engine::engine::*;

mod console_io;

fn main() {
    let mut eng = QuantumChessEngine::new();
    let mut show_board = true;

    loop {
        let qb = eng.get_quantum_chessboard();

        if show_board {
            console_io::output::display_harmonics(&qb.harmonics, 8);
        }

        println!("\n-------------------------\n");
        match console_io::input::input_move() {
            None => {
                println!("Unable to parse your move, try again");
                show_board = false;
            },
            Some(m) => {
                let res = eng.submit(&m);
                match res {
                    ChessMoveResult::Success => {
                        eng.player.switch();
                        show_board = true;
                    },
                    ChessMoveResult::Failure(str) => {
                        println!("{}", &str);
                        show_board = false;
                    },
                }
            }
        }
    }
}