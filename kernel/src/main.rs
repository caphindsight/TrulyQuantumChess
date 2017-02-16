extern crate num;
extern crate rand;
#[macro_use] extern crate text_io;

mod chess;
mod engine;
mod quantize;
mod console_io;

fn main() {
    let mut eng = engine::QuantumChessEngineImpl::new();

    let mut show_board = true;

    loop {
        let qb = eng.get_quantum_chessboard();
        assert!(qb.harmonics.len() == 1, "Multiple harmonics?!");

        if show_board {
            let board = qb.harmonics[0].board.clone();
            console_io::output::display_chessboard(&board);
        }

        let mv = console_io::input::input_move();
        // println!("{:?}", &mv);

        use engine::ChessEngine;
        let res = eng.submit_move(&mv);
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
