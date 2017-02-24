using System;
using System.Collections.Generic;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Moves;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Engine;

namespace TrulyQuantumChess.WebApp.Model {
    public class MoveRequest {
        public string GameId;
        public string MoveType;
        public string Source;
        public string Middle;
        public string Target;

        public QuantumChessMove Parse(QuantumChessEngine engine) {
            if (MoveType == "ordinary") {
                Position source = Position.Parse(Source);
                Position target = Position.Parse(Target);
                Piece? piece = engine.Chessboard.GetQuantumPiece(source).Piece;
                if (piece.HasValue)
                    return new OrdinaryMove(piece.Value, source, target);
                else
                    throw new MoveParseException($"No piece found at {source}");
            } else if (MoveType == "quantum") {
                Position source = Position.Parse(Source);
                Position? middle = null;
                if (!String.IsNullOrEmpty(Middle))
                    middle = Position.Parse(Middle);
                Position target = Position.Parse(Target);
                Piece? piece = engine.Chessboard.GetQuantumPiece(source).Piece;
                if (piece.HasValue)
                    return new QuantumMove(piece.Value, source, middle, target);
                else
                    throw new MoveParseException($"No piece found at {source}");
            } else if (MoveType == "capitulate") {
                return new CapitulateMove(engine.ActivePlayer);
            } else if (MoveType == "castle_left") {
                return new CastleMove(engine.ActivePlayer, CastleType.Left);
            } else if (MoveType == "castle_right") {
                return new CastleMove(engine.ActivePlayer, CastleType.Right);
            } else {
                throw new MoveParseException($"Unsupported move type: {MoveType}");
            }
        }
    }

    public class NewGameResponse {
        public string GameId;
    }

    public class InfoResponse {
        public class SquareEncoded {
            public string Player;
            public string Piece;
            public double Probability;
        }

        public string GameState;
        public string ActivePlayer;
        public Dictionary<String, SquareEncoded> Squares;
    }
}
