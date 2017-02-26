using System;
using System.Collections.Generic;
using System.Linq;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Moves;

namespace TrulyQuantumChess.Kernel.Quantize {
    public static class MeasurementUtils {
        private static readonly Random Random_ = new Random();

        public static double Probability(ulong degeneracy, ulong total) =>
            Convert.ToDouble(degeneracy) / Convert.ToDouble(total);

        public static bool Decide(double probability) {
            double rand = Random_.NextDouble();
            bool res = rand < probability;
            // Console.WriteLine($"Measurement with probability {probability} rendered {res}");
            return res;
        }

        public static bool Decide(ulong degeneracy, ulong total) {
            return Decide(Probability(degeneracy, total));
        }
    }

    public class QuantumHarmonic {
        public readonly Chessboard Board;
        public ulong Degeneracy;

        public QuantumHarmonic(Chessboard board, ulong degeneracy) {
            Board = board;
            Degeneracy = degeneracy;
        }

        public QuantumHarmonic Clone() {
            return new QuantumHarmonic(Board.Clone(), Degeneracy);
        }
    }

    public struct QuantumPiece {
        public readonly Piece? Piece;
        public readonly double Probability;

        public QuantumPiece(Piece? piece, double probability) {
            Piece = piece;
            Probability = probability;
        }
    }

    public class QuantumChessboard {
        private List<QuantumHarmonic> Harmonics_ = new List<QuantumHarmonic>();
        private GameState GameState_ = GameState.Tie;

        public QuantumChessboard() {
        }

        public QuantumChessboard(List<QuantumHarmonic> harmonics, GameState gameState) {
            Harmonics_ = harmonics;
            GameState_ = gameState;
        }

        public static QuantumChessboard StartingQuantumChessboard() {
            var res = new QuantumChessboard();
            res.GameState_ = GameState.GameStillGoing;
            res.Harmonics_.Add(new QuantumHarmonic(Chessboard.StartingChessboard(), 1));
            return res;
        }

        private ulong DegeneracyNormalization() {
            ulong res = 0;
            foreach (QuantumHarmonic harmonic in Harmonics_)
                res += harmonic.Degeneracy;
            return res;
        }

        private static ulong Gcd(ulong a, ulong b) {
            while (a != 0 && b != 0) {
                if (a > b)
                    a %= b;
                else if (b > a)
                    b %= a;
                else
                    return a;
            }
            return a + b;
        }

        private void RenormalizeDegeneracies() {
            ulong gcd = 0;
            foreach (QuantumHarmonic harmonic in Harmonics_)
                gcd = Gcd(gcd, harmonic.Degeneracy);
            foreach (QuantumHarmonic harmonic in Harmonics_)
                harmonic.Degeneracy /= gcd;
        }

        private void RegroupHarmonics() {
            RemoveVanishing();
            AssertionException.Assert(Harmonics_.Count > 0, "Empty quantum superposition found");

            Harmonics_.Sort((a, b) => a.Board.GetHashCodeWithGameState().CompareTo(b.Board.GetHashCodeWithGameState()));
            var new_harmonics = new List<QuantumHarmonic>();

            QuantumHarmonic prev_harmonic = Harmonics_[0];
            for (int i = 1; i < Harmonics_.Count; i++) {
                if (Harmonics_[i].Board == prev_harmonic.Board) {
                    prev_harmonic.Degeneracy += Harmonics_[i].Degeneracy;
                } else {
                    new_harmonics.Add(prev_harmonic);
                    prev_harmonic = Harmonics_[i];
                }
            }
            new_harmonics.Add(prev_harmonic);

            Harmonics_ = new_harmonics;
            Harmonics_.Sort((a, b) => b.Degeneracy.CompareTo(a.Degeneracy));
        }

        private void RemoveVanishing() {
            FilterBy((h) => h.Degeneracy > 0);
        }


        private void FilterBy(Predicate<QuantumHarmonic> pred) {
            var new_harmonics = new List<QuantumHarmonic>();
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (pred(harmonic))
                    new_harmonics.Add(harmonic);
            }
            AssertionException.Assert(new_harmonics.Count > 0, "Filtered into empty quantum superposition");
            Harmonics_ = new_harmonics;
            RenormalizeDegeneracies();
        }

        private void PerformMeasurement(Position pos) {
            var piece_degeneracies = new Dictionary<Piece, ulong>();
            ulong overall_degeneracy = 0;

            foreach (QuantumHarmonic harmonic in Harmonics_) {
                Piece? square = harmonic.Board[pos];
                if (square.HasValue) {
                    if (!piece_degeneracies.ContainsKey(square.Value))
                        piece_degeneracies[square.Value] = 0;
                    piece_degeneracies[square.Value] += harmonic.Degeneracy;
                    overall_degeneracy += harmonic.Degeneracy;
                }
            }

            if (piece_degeneracies.Count <= 1) {
                // No measurement is needed
                return;
            }

            foreach (Piece piece in piece_degeneracies.Keys) {
                ulong degeneracy = piece_degeneracies[piece];
                if (MeasurementUtils.Decide(degeneracy, overall_degeneracy)) {
                    // Removing all the harmonics with another piece
                    FilterBy((h) => h.Board[pos] == null || h.Board[pos] == piece);
                    return;
                } else {
                    overall_degeneracy -= degeneracy;
                }
            }
            AssertionException.Assert(false, "One of the pieces has to be chosen");
        }

        private void PerformMeasurements() {
            RemoveVanishing();
            for (int i = 0; i < 64; i++) {
                PerformMeasurement(Position.FromIndex(i));
            }
        }

        private void PerformSpontaneousMeasurement() {
            ulong total_degeneracy = 0;
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                total_degeneracy += harmonic.Degeneracy;
            }

            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (MeasurementUtils.Decide(harmonic.Degeneracy, total_degeneracy)) {
                    var new_harmonics = new List<QuantumHarmonic>();
                    new_harmonics.Add(harmonic);
                    Harmonics_ = new_harmonics;
                    return;
                }
                total_degeneracy -= harmonic.Degeneracy;
            }
        }

        private void UpdateGameState() {
            if (GameState_ != GameState.GameStillGoing)
                return;

            if (Harmonics_.All((h) => h.Board.GameState != GameState.GameStillGoing)) {
                ulong white_victory_degeneracy = Harmonics_
                    .Where((h) => h.Board.GameState == GameState.WhiteVictory)
                    .Select((h) => h.Degeneracy)
                    .Aggregate(0ul, (a, b) => a + b);

                ulong black_victory_degeneracy = Harmonics_
                    .Where((h) => h.Board.GameState == GameState.BlackVictory)
                    .Select((h) => h.Degeneracy)
                    .Aggregate(0ul, (a, b) => a + b);

                ulong tie_degeneracy = Harmonics_
                    .Where((h) => h.Board.GameState == GameState.Tie)
                    .Select((h) => h.Degeneracy)
                    .Aggregate(0ul, (a, b) => a + b);

                ulong total_degeneracy = white_victory_degeneracy + black_victory_degeneracy + tie_degeneracy;
                if (!MeasurementUtils.Decide(tie_degeneracy, total_degeneracy)) {
                    total_degeneracy -= tie_degeneracy;
                    if (MeasurementUtils.Decide(white_victory_degeneracy, total_degeneracy)) {
                        GameState_ = GameState.WhiteVictory;
                    } else {
                        GameState_ = GameState.BlackVictory;
                    }
                } else {
                    GameState_ = GameState.Tie;
                }
            }
        }

        private void UpdateQuantumCheckboard() {
            PerformMeasurements();
            if (Harmonics_.Count >= 1024)
                PerformSpontaneousMeasurement();
            RegroupHarmonics();
            RenormalizeDegeneracies();
            UpdateGameState();
        }

        public List<QuantumHarmonic> Harmonics {
            get { return Harmonics_; }
        }

        public GameState GameState {
            get { return GameState_; }
        }

        public bool CheckOrdinaryMoveApplicable(OrdinaryMove move) {
            return Harmonics_.Any((h) => h.Board.GameState == GameState.GameStillGoing &&
                                 h.Board.CheckOrdinaryMoveApplicable(move));
        }

        public void ApplyOrdinaryMove(OrdinaryMove move) {
            bool applied = false;
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (harmonic.Board.CheckOrdinaryMoveApplicable(move)) {
                    harmonic.Board.ApplyOrdinaryMove(move);
                    applied = true;
                }
            }
            UpdateQuantumCheckboard();
            AssertionException.Assert(applied, "Ordinary move couldn't be applied on any harmonic");
        }

        public bool CheckQuantumMoveApplicable(QuantumMove move) {
            return Harmonics_.Any((h) => h.Board.GameState == GameState.GameStillGoing &&
                                 h.Board.CheckQuantumMoveApplicable(move));
        }

        public void ApplyQuantumMove(QuantumMove move) {
            bool applied = false;
            var new_harmonics = new List<QuantumHarmonic>();
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (harmonic.Board.CheckQuantumMoveApplicable(move)) {
                    // Passing to the superposition of the original and new harmonics
                    QuantumHarmonic new_harmonic = harmonic.Clone();
                    new_harmonic.Board.ApplyQuantumMove(move);
                    new_harmonics.Add(harmonic);
                    new_harmonics.Add(new_harmonic);
                    applied = true;
                } else {
                    // Keeping the original harmonic with degeneracy doubled
                    harmonic.Degeneracy *= 2;
                    new_harmonics.Add(harmonic);
                }
            }
            Harmonics_ = new_harmonics;
            UpdateQuantumCheckboard();
            AssertionException.Assert(applied, "Quantum move couldn't be applied on any harmonic");
        }

        public bool CheckCastleMoveApplicable(CastleMove move) {
            return Harmonics_.Any((h) => h.Board.GameState == GameState.GameStillGoing &&
                                  h.Board.CheckCastleMoveApplicable(move));
        }

        public void ApplyCastleMove(CastleMove move) {
            bool applied = false;
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (harmonic.Board.CheckCastleMoveApplicable(move)) {
                    harmonic.Board.ApplyCastleMove(move);
                    applied = true;
                }
            }
            UpdateQuantumCheckboard();
            AssertionException.Assert(applied, "Ordinary move couldn't be applied on any harmonic");
        }

        public void RegisterVictory(Player player) {
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (harmonic.Board.GameState == GameState.GameStillGoing)
                    harmonic.Board.RegisterVictory(player);
            }
            UpdateGameState();
        }

        public void RegisterTie() {
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                if (harmonic.Board.GameState == GameState.GameStillGoing) {
                    harmonic.Board.RegisterTie();
                }
            }
            UpdateGameState();
        }

        public QuantumPiece GetQuantumPiece(Position pos) {
            Piece? piece = null;
            ulong filled = 0, empty = 0;
            foreach (QuantumHarmonic harmonic in Harmonics_) {
                Piece? classical = harmonic.Board[pos];
                if (classical.HasValue) {
                    AssertionException.Assert(piece == null || piece == classical,
                                              $"The square {pos} appears in a superposition of two pieces");
                    piece = classical;
                    filled += harmonic.Degeneracy;
                } else {
                    empty += harmonic.Degeneracy;
                }
            }
            if (piece.HasValue) {
                return new QuantumPiece(piece, MeasurementUtils.Probability(filled, filled + empty));
            } else {
                return new QuantumPiece(null, 1.0);
            }
        }
    }
}