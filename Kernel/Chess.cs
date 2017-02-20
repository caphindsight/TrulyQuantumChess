using System;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Moves;

namespace TrulyQuantumChess.Kernel.Chess {
    public enum Player {
        White,
        Black
    }

    public static class PlayerUtils {
        public static Player InvertPlayer(Player player) {
            switch (player) {
                case Player.White:
                    return Player.Black;
                case Player.Black:
                    return Player.White;
                default:
                    throw new AssertionException($"Unsupported player: {player}");
            }
        }
    }

    public enum PieceType {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public struct Piece {
        public readonly Player Player;
        public readonly PieceType PieceType;

        public Piece(Player player, PieceType pieceType) {
            Player = player;
            PieceType = pieceType;
        }

        public static bool operator == (Piece a, Piece b) {
            return a.Player == b.Player && a.PieceType == b.PieceType;
        }

        public static bool operator != (Piece a, Piece b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (obj is Piece) {
                Piece piece = (Piece) obj;
                return this == piece;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return 1 + PieceType.GetHashCode() * 2 + Player.GetHashCode();
        }
    }

    public struct Position {
        private readonly int Ind_;

        public Position(int index) {
            Ind_ = index;
        }

        public static Position FromIndex(int index) {
            var res = new Position(index);
            AssertionException.Assert(res.Ind_ >= 0 && res.Ind_ < 64, $"Chessboard index is out of bounds: {res.Ind_}");
            return res;
        }

        public static Position FromCoords(int x, int y) {
            var res = new Position(y * 8 + x);
            AssertionException.Assert(res.Ind_ >= 0 && res.Ind_ < 64, $"Chessboard index is out of bounds: {res.Ind_}");
            return res;
        }

        public int Ind {
            get { return Ind_; }
        }

        public int X {
            get { return Ind_ % 8; }
        }

        public int Y {
            get { return Ind_ / 8; }
        }

        public static bool operator == (Position a, Position b) {
            return a.Ind_ == b.Ind_;
        }

        public static bool operator != (Position a, Position b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (obj is Position) {
                Position position = (Position) obj;
                return this == position;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return Ind_.GetHashCode();
        }

        public override string ToString() {
            char c1 = Convert.ToChar(Convert.ToInt32('A') + X);
            char c2 = Convert.ToChar(Convert.ToInt32('1') + Y);
            return $"{c1}{c2}";
        }
    }

    public enum GameState {
        GameStillGoing,
        WhiteVictory,
        BlackVictory,
        Tie
    }

    public class Chessboard {
        private readonly Piece?[] Pieces_ = new Piece?[64];
        private GameState GameState_ = GameState.Tie;

        public static Chessboard EmptyChessboard() {
            return new Chessboard();
        }

        public static Chessboard StartingChessboard() {
            var chessboard = EmptyChessboard();
            chessboard.GameState_ = GameState.GameStillGoing;

            // Setting up white power pieces
            chessboard.Pieces_[0] = new Piece(Player.White, PieceType.Rook);
            chessboard.Pieces_[1] = new Piece(Player.White, PieceType.Knight);
            chessboard.Pieces_[2] = new Piece(Player.White, PieceType.Bishop);
            chessboard.Pieces_[3] = new Piece(Player.White, PieceType.Queen);
            chessboard.Pieces_[4] = new Piece(Player.White, PieceType.King);
            chessboard.Pieces_[5] = new Piece(Player.White, PieceType.Bishop);
            chessboard.Pieces_[6] = new Piece(Player.White, PieceType.Knight);
            chessboard.Pieces_[7] = new Piece(Player.White, PieceType.Rook);

            // Setting up black power pieces
            chessboard.Pieces_[56] = new Piece(Player.Black, PieceType.Rook);
            chessboard.Pieces_[57] = new Piece(Player.Black, PieceType.Knight);
            chessboard.Pieces_[58] = new Piece(Player.Black, PieceType.Bishop);
            chessboard.Pieces_[59] = new Piece(Player.Black, PieceType.Queen);
            chessboard.Pieces_[60] = new Piece(Player.Black, PieceType.King);
            chessboard.Pieces_[61] = new Piece(Player.Black, PieceType.Bishop);
            chessboard.Pieces_[62] = new Piece(Player.Black, PieceType.Knight);
            chessboard.Pieces_[63] = new Piece(Player.Black, PieceType.Rook);

            // Setting up white pawns
            for (int i = 8; i < 16; i++) {
                chessboard.Pieces_[i] = new Piece(Player.White, PieceType.Pawn);
            }

            // Setting up black pawns
            for (int i = 48; i < 56; i++) {
                chessboard.Pieces_[i] = new Piece(Player.Black, PieceType.Pawn);
            }

            return chessboard;
        }

        public Chessboard Clone() {
            var res = new Chessboard();
            res.GameState_ = GameState_;
            for (int i = 0; i < 64; i++) {
                res.Pieces_[i] = Pieces_[i];
            }
            return res;
        }

        public GameState GameState {
            get { return GameState_; }
        }

        public Piece? this[Position pos] {
            get {
                return Pieces_[pos.Ind];
            }
            set {
                Pieces_[pos.Ind] = value;
            }
        }

        public Piece? this[int index] {
            get {
                var pos = Position.FromIndex(index);
                return Pieces_[pos.Ind];
            }
            set {
                var pos = Position.FromIndex(index);
                Pieces_[pos.Ind] = value;
            }
        }

        public Piece? this[int x, int y] {
            get {
                var pos = Position.FromCoords(x, y);
                return Pieces_[pos.Ind];
            }
            set {
                var pos = Position.FromCoords(x, y);
                Pieces_[pos.Ind] = value;
            }
        }

        public static bool operator == (Chessboard a, Chessboard b) {
            for (int i = 0; i < 64; i++) {
                if (a.Pieces_[i] != b.Pieces_[i])
                    return false;
            }
            return a.GameState_ == b.GameState_;
        }

        public static bool operator != (Chessboard a, Chessboard b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (obj is Chessboard) {
                var chessboard = obj as Chessboard;
                return this == chessboard;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            int res = 0, hash_base = 17;
            unchecked {
                for (int i = 0; i < 64; i++) {
                    res *= hash_base;
                    res += Pieces_[i].GetHashCode();
                }
            }
            return res;
        }

        public int GetHashCodeWithGameState() {
            return unchecked(GetHashCode() * 4 + GameState_.GetHashCode());
        }

        private static int Signum(int x) {
            if (x > 0) {
                return 1;
            } else if (x < 0) {
                return -1;
            } else {
                return 0;
            }
        }

        private bool CheckIntermediateSquares(Piece piece, Position source, Position target, bool capture) {
            int dx = target.X - source.X;
            int dx_abs = Math.Abs(dx);
            int dx_sig = Signum(dx);

            int dy = target.Y - source.Y;
            int dy_abs = Math.Abs(dy);
            int dy_sig = Signum(dy);

            switch (piece.PieceType) {
                case PieceType.Pawn:
                    switch (piece.Player) {
                        case Player.White:
                            if (!capture)
                                return source.X == target.X && (source.Y + 1 == target.Y || source.Y == 1 && target.Y == 3);
                            else
                                return (source.X == target.X + 1 || source.X + 1 == target.X) && source.Y + 1 == target.Y;
                        case Player.Black:
                            if (!capture)
                                return source.X == target.X && (source.Y - 1 == target.Y || source.Y == 6 && target.Y == 4);
                            else
                                return (source.X == target.X + 1 || source.X + 1 == target.X) && source.Y - 1 == target.Y;
                        default:
                            throw new AssertionException($"Unsupported player: {piece.Player}");
                    }

                case PieceType.Knight:
                    return dx_abs == 1 && dy_abs == 2 || dx_abs == 2 && dy_abs == 1;

                case PieceType.Bishop:
                    if (dx_abs != dy_abs)
                        return false;
                    for (int i = 1; i < dx_abs; i++) {
                        if (this[source.X + i * dx_sig, source.Y + i * dy_sig] != null)
                            return false;
                    }
                    return true;

                case PieceType.Rook:
                    if (dx != 0 && dy != 0)
                        return false;
                    for (int i = 1; i < dx + dy; i++) {
                        if (this[source.X + i * dx_sig, source.Y + i * dy_sig] != null)
                            return false;
                    }
                    return true;

                case PieceType.Queen:
                    if (dx_abs == dy_abs) {
                        for (int i = 1; i < dx_abs; i++) {
                            if (this[source.X + i * dx_sig, source.Y + i * dy_sig] != null)
                                return false;
                        }
                        return true;
                    } else if (dx_abs == 0 || dy_abs == 0) {
                        for (int i = 1; i < dx + dy; i++) {
                            if (this[source.X + i * dx_sig, source.Y + i * dy_sig] != null)
                                return false;
                        }
                        return true;
                    } else {
                        return false;
                    }

                case PieceType.King:
                    return (dx_abs == 0 || dx_abs == 1) && (dy_abs == 0 || dy_abs == 1);

                default:
                    throw new AssertionException($"Unsupported piece type: {piece.PieceType}");
            }
        }

        public bool CheckOrdinaryMoveApplicable(OrdinaryMove move) {
            if (move.Source == move.Target) {
                // Dummy moves aren't allowed by the rules of the game
                return false;
            }

            Piece? source = this[move.Source];
            if (source != move.ActorPiece) {
                // The chessboard doesn't have an actor piece at the source position
                // Therefore the move is inapplicable
                return false;
            }

            Piece? target = this[move.Target];
            bool capture = target != null;
            return CheckIntermediateSquares(move.ActorPiece, move.Source, move.Target, capture);
        }

        public void ApplyOrdinaryMove(OrdinaryMove move) {
            AssertionException.Assert(CheckOrdinaryMoveApplicable(move), $"Attempted applying inapplicable ordinary move");
            this[move.Target] = this[move.Source];
            this[move.Source] = null;
            if (GameState_ == GameState.GameStillGoing) {
                bool white_king_present = false;
                bool black_king_present = false;
                for (int i = 0; i < 64; i++) {
                    if (!Pieces_[i].HasValue)
                        continue;
                    if (Pieces_[i].Value.PieceType != PieceType.King)
                        continue;
                    switch (Pieces_[i].Value.Player) {
                        case Player.White:
                            white_king_present = true;
                            break;
                        case Player.Black:
                            black_king_present = true;
                            break;
                    }
                }
                if (!white_king_present && !black_king_present) {
                    GameState_ = GameState.Tie;
                } else if (!white_king_present) {
                    GameState_ = GameState.BlackVictory;
                } else if (!black_king_present) {
                    GameState_ = GameState.WhiteVictory;
                }
            }
        }

        public bool CheckQuantumMoveApplicable(QuantumMove move) {
            if (move.Source == move.Target) {
                // Dummy moves aren't allowed by the rules of the game
                return false;
            }

            Piece? source = this[move.Source];
            if (source != move.ActorPiece) {
                // The chessboard doesn't have an actor piece at the source position
                // Therefore the move is inapplicable
                return false;
            }

            Piece? target = this[move.Target];
            if (target != null) {
                // You can't quantum-capture!
                return false;
            }

            if (move.Middle.HasValue) {
                Piece? middle = this[move.Middle.Value];
                if (middle != null) {
                    // Move is inapplicable
                    return false;
                }

                return CheckIntermediateSquares(move.ActorPiece, move.Source, move.Middle.Value, false) &&
                    CheckIntermediateSquares(move.ActorPiece, move.Middle.Value, move.Target, false);
            } else {
                return CheckIntermediateSquares(move.ActorPiece, move.Source, move.Target, false);
            }
        }

        public void ApplyQuantumMove(QuantumMove move) {
            AssertionException.Assert(CheckQuantumMoveApplicable(move), $"Attempted applying inapplicable quantum move");
            this[move.Target] = this[move.Source];
            this[move.Source] = null;
        }

        public void RegisterVictory(Player player) {
            if (GameState_ == GameState.GameStillGoing) {
                switch (player) {
                    case Player.White:
                        GameState_ = GameState.WhiteVictory;
                    break;

                    case Player.Black:
                        GameState_ = GameState.BlackVictory;
                    break;
                }
            }
        }
    }
}
