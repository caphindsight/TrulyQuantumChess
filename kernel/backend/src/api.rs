use rustless;
use rustless::Nesting;

use engine;
use engine::chess::*;
use engine::quantize::*;
use engine::engine::*;

use chess_json;

pub fn api_root() -> rustless::Api {
    rustless::Api::build(|api_root| {
        api_root.namespace("api", |api| {
            api.get("ping", |ping| {
                ping.handle(|client, params| {
                    client.text("pong\r\n".to_string())
                })
            });

            api.get("starting_chessboard", |starting_chessboard| {
                starting_chessboard.handle(|client, params| {
                    let qcb = QuantumChessEngine::new().get_quantum_chessboard();
                    let qcb_serialized = chess_json::output::serialize_quantum_chessboard(&qcb);
                    client.json(&qcb_serialized)
                })
            });
        });
    })
}
