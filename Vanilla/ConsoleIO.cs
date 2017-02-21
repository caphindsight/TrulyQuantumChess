using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Moves;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Quantize;
using TrulyQuantumChess.Kernel.Engine;

namespace TrulyQuantumChess.Vanilla.ConsoleIO {
    public static class Input {
        public static QuantumChessMove ReadMove(QuantumChessEngine engine) {
            switch (engine.ActivePlayer) {
                case Player.White:
                    Console.Write("white> ");
                break;
                case Player.Black:
                    Console.Write("black> ");
                break;
            }

            string move_str = Console.ReadLine();

            string capitulate_move_regex = @"^(quit|exit|capitulate)$";
            string agree_to_tie_move_regex = @"^tie$";
            string ordinary_move_regex = @"^([A-Za-z][1-8])\s*([A-Za-z][1-8])$";
            string quantum_move_regex = @"^(?:q|Q|quantum)\s+([A-Za-z][1-8])\s*((?:[A-Za-z][1-8])?)\s*([A-Za-z][1-8])$";
            string castle_move_regex = @"^castle (left|right)$";

            Match ordinary_match = Regex.Match(move_str, ordinary_move_regex);
            Match quantum_match = Regex.Match(move_str, quantum_move_regex);
            Match castle_match = Regex.Match(move_str, castle_move_regex);

            if (Regex.IsMatch(move_str, capitulate_move_regex)) {
                return new CapitulateMove(engine.ActivePlayer);
            } else if (Regex.IsMatch(move_str, agree_to_tie_move_regex)) {
                return new AgreeToTieMove(engine.ActivePlayer);
            } else if (ordinary_match.Success) {
                Position source = Position.Parse(ordinary_match.Groups[1].Captures[0].Value);
                Position target = Position.Parse(ordinary_match.Groups[2].Captures[0].Value);
                QuantumPiece qpiece = engine.Chessboard.GetQuantumPiece(source);
                if (qpiece.Piece.HasValue)
                    return new OrdinaryMove(qpiece.Piece.Value, source, target);
                else
                    throw new MoveParseException($"No piece found at {source}");
            } else if (quantum_match.Success) {
                Position source = Position.Parse(quantum_match.Groups[1].Captures[0].Value);
                Position? middle = null;
                if (quantum_match.Groups[2].Captures[0].Length > 0)
                    middle = Position.Parse(quantum_match.Groups[2].Captures[0].Value);
                Position target = Position.Parse(quantum_match.Groups[3].Captures[0].Value);
                QuantumPiece qpiece = engine.Chessboard.GetQuantumPiece(source);
                if (qpiece.Piece.HasValue)
                    return new QuantumMove(qpiece.Piece.Value, source, middle, target);
                else
                    throw new MoveParseException($"No piece found at {source}");
            } else if (castle_match.Success) {
                string castle_type_str = castle_match.Groups[1].Captures[0].Value;
                CastleType castle_type;
                if (castle_type_str == "left") {
                    castle_type = CastleType.Left;
                } else if (castle_type_str == "right") {
                    castle_type = CastleType.Right;
                } else {
                    throw new MoveParseException($"Unsupported castle type: {castle_type_str}");
                }
                return new CastleMove(engine.ActivePlayer, castle_type);
            } else {
                throw new MoveParseException("Unable to parse move");
            }
        }

        public static QuantumChessMove ReadMoveRepeated(QuantumChessEngine engine) {
            for (;;) {
                try {
                    return ReadMove(engine);
                } catch (MoveParseException e) {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    public static class Output {
        private static char GetUnicodeChar(Piece? piece) {
            if (piece.HasValue) {
                switch (piece.Value.Player) {
                    case Player.White:
                        switch (piece.Value.PieceType) {
                            case PieceType.Pawn: return '\u2659';
                            case PieceType.Knight: return '\u2658';
                            case PieceType.Bishop: return '\u2657';
                            case PieceType.Rook: return '\u2656';
                            case PieceType.Queen: return '\u2655';
                            case PieceType.King: return '\u2654';
                            default: throw new AssertionException($"Unsupported piece type: {piece.Value.PieceType}");
                        }

                    case Player.Black:
                        switch (piece.Value.PieceType) {
                            case PieceType.Pawn: return '\u265F';
                            case PieceType.Knight: return '\u265E';
                            case PieceType.Bishop: return '\u265D';
                            case PieceType.Rook: return '\u265C';
                            case PieceType.Queen: return '\u265B';
                            case PieceType.King: return '\u265A';
                            default: throw new AssertionException($"Unsupported piece type: {piece.Value.PieceType}");
                        }

                    default:
                        throw new AssertionException($"Unsupported player: {piece.Value.Player}");
                }
            } else {
                return '.';
            }
        }

        private static void DisplayHarmonics(List<QuantumHarmonic> harmonics) {
            for (int y = 7; y >= 0; y--) {
                foreach (QuantumHarmonic harmonic in harmonics) {
                    Console.Write($"{y+1} ");
                    for (int x = 0; x < 8; x++) {
                        Console.Write(GetUnicodeChar(harmonic.Board[x, y]));
                        Console.Write(" ");
                    }
                    Console.Write("    ");
                }
                Console.WriteLine();
            }
            foreach (QuantumHarmonic harmonic in harmonics) {
                Console.Write("  a b c d e f g h     ");
            }
            Console.WriteLine();
            foreach (QuantumHarmonic harmonic in harmonics) {
                string win_str;
                switch (harmonic.Board.GameState) {
                    case GameState.GameStillGoing:
                        win_str = "   ";
                    break;
                    case GameState.WhiteVictory:
                        win_str = "[w]";
                    break;
                    case GameState.BlackVictory:
                        win_str = "[b]";
                    break;
                    case GameState.Tie:
                        win_str = "[=]";
                    break;
                    default:
                        throw new AssertionException($"Unsupported game state: {harmonic.Board.GameState}");
                }
                string deg = harmonic.Degeneracy == 1 ? "  " : $"x{harmonic.Degeneracy}";
                Console.Write($"   {win_str} {deg}");
                int expected_ln = 8;
                int actual_ln = harmonic.Degeneracy.ToString().Length;
                for (int i = actual_ln; i != expected_ln; i++)
                    Console.Write(" ");
                Console.Write("      ");
            }
            Console.WriteLine();
        }

        public static void DisplayQuantumChessboard(QuantumChessboard qboard, int cols) {
            int n = qboard.Harmonics.Count;
            int rows = (n + cols - 1) / cols;
            for (int row = 0; row < rows; row++) {
                var harmonics = new List<QuantumHarmonic>();
                for (int col = 0; col < cols; col++) {
                    int ind = row * cols + col;
                    if (ind < n)
                        harmonics.Add(qboard.Harmonics[ind]);
                }
                DisplayHarmonics(harmonics);
                if (row != rows - 1)
                    Console.WriteLine();
            }
        }
    }
}