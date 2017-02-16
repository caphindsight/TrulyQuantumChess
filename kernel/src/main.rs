extern crate num;
extern crate rand;
extern crate regex;

mod chess;
mod engine;
mod quantize;
mod console_io;

fn main() {
    let mut eng = engine::QuantumChessEngine::new();
    let mut show_board = true;

    loop {
        let qb = eng.get_quantum_chessboard();

        if show_board {
            for harmonic in &qb.harmonics {
                console_io::output::display_chessboard(&harmonic.board);
                println!("-- ampl={} --", harmonic.ampl);
                println!("");
            }
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
                    chess::ChessMoveResult::Success => {
                        eng.player.switch();
                        show_board = true;
                    },
                    chess::ChessMoveResult::Failure(str) => {
                        println!("{}", &str);
                        show_board = false;
                    },
                }

            }
        }
    }
}
