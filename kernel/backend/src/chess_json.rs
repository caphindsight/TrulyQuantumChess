use rustless;
use rustless::json::*;

use std::collections::BTreeMap;

use engine;
use engine::chess::*;
use engine::quantize::*;
use engine::engine::*;

pub mod output {
    use super::*;

    pub fn serialize_quantum_chessboard(qcb: &QuantumChessboard) -> JsonValue {
        let mut squares: Vec<JsonValue> = vec![JsonValue::Null; 64];
        for i in 0_usize..64_usize {
            let (x, y) = Chessboard::unindex(i);
            let inf = qcb.get_quantum_square_info(x, y);
            let mut data = BTreeMap::new();
            data.insert("piece".to_string(), JsonValue::String(inf.square.piece.to_string()));
            if inf.square.is_occupied() {
                data.insert("player".to_string(), JsonValue::String(inf.square.player.to_string()));
            }
            data.insert("prob".to_string(), JsonValue::F64(inf.probability));
            squares[i] = JsonValue::Object(data);
        }
        let mut data = BTreeMap::new();
        data.insert("squares".to_string(), JsonValue::Array(squares));
        JsonValue::Object(data)
    }
}
