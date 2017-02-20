using System;
using System.Collections.Generic;
using System.Linq;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Moves;

namespace TrulyQuantumChess.Kernel.Quantize {
    public static class MeasurementUtils {
        private static readonly Random Random_ = new Random();

        public static bool Decide(double probability) {
            double rand = Random_.NextDouble();
            bool res = rand < probability;
            Console.WriteLine($"Measurement with probability {probability} rendered {res}");
            return res;
        }

        public static bool Decide(ulong degeneracy, ulong total) {
            return Decide(Convert.ToDouble(degeneracy) / Convert.ToDouble(total));
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

    public class QuantumChessboard {
        private List<QuantumHarmonic> Harmonics = new List<QuantumHarmonic>();
        private GameState GameState_ = GameState.Tie;

        public static QuantumChessboard StartingQuantumChessboard() {
            var res = new QuantumChessboard();
            res.GameState_ = GameState.GameStillGoing;
            res.Harmonics.Add(new QuantumHarmonic(Chessboard.StartingChessboard(), 1));
            return res;
        }

        private ulong DegeneracyNormalization() {
            ulong res = 0;
            foreach (QuantumHarmonic harmonic in Harmonics)
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
            foreach (QuantumHarmonic harmonic in Harmonics)
                gcd = Gcd(gcd, harmonic.Degeneracy);
            foreach (QuantumHarmonic harmonic in Harmonics)
                harmonic.Degeneracy /= gcd;
        }

        private void RegroupHarmonics() {
            RemoveVanishing();
            AssertionException.Assert(Harmonics.Count > 0, "Empty quantum superposition found");

            Harmonics.Sort((a, b) => a.Board.GetHashCodeWithGameState().CompareTo(b.Board.GetHashCodeWithGameState()));
            var new_harmonics = new List<QuantumHarmonic>();

            QuantumHarmonic prev_harmonic = Harmonics[0];
            for (int i = 1; i < Harmonics.Count; i++) {
                if (Harmonics[i].Board == prev_harmonic.Board) {
                    prev_harmonic.Degeneracy += Harmonics[i].Degeneracy;
                } else {
                    new_harmonics.Add(prev_harmonic);
                    prev_harmonic = Harmonics[i];
                }
            }
            new_harmonics.Add(prev_harmonic);

            Harmonics = new_harmonics;
            Harmonics.Sort((a, b) => b.Degeneracy.CompareTo(a.Degeneracy));
        }

        private void RemoveVanishing() {
            FilterBy((h) => h.Degeneracy > 0);
        }


        private void FilterBy(Predicate<QuantumHarmonic> pred) {
            var new_harmonics = new List<QuantumHarmonic>();
            foreach (QuantumHarmonic harmonic in Harmonics) {
                if (pred(harmonic))
                    new_harmonics.Add(harmonic);
            }
            AssertionException.Assert(new_harmonics.Count > 0, "Filtered into empty quantum superposition");
            Harmonics = new_harmonics;
            RenormalizeDegeneracies();
        }

        private void PerformMeasurement(Position pos) {
            var piece_degeneracies = new Dictionary<Piece, ulong>();
            ulong overall_degeneracy = 0;

            foreach (QuantumHarmonic harmonic in Harmonics) {
                Piece? square = harmonic.Board[pos];
                if (square.HasValue) {
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

        private void UpdateGameState() {
            if (GameState_ != GameState.GameStillGoing)
                return;

            if (Harmonics.All((h) => h.Board.GameState != GameState.GameStillGoing)) {
                ulong white_victory_degeneracy = Harmonics
                    .Where((h) => h.Board.GameState == GameState.WhiteVictory)
                    .Select((h) => h.Degeneracy)
                    .Aggregate((a, b) => a + b);

                ulong black_victory_degeneracy = Harmonics
                    .Where((h) => h.Board.GameState == GameState.BlackVictory)
                    .Select((h) => h.Degeneracy)
                    .Aggregate((a, b) => a + b);

                ulong tie_degeneracy = Harmonics
                    .Where((h) => h.Board.GameState == GameState.Tie)
                    .Select((h) => h.Degeneracy)
                    .Aggregate((a, b) => a + b);

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
            RegroupHarmonics();
            RenormalizeDegeneracies();
            UpdateGameState();
        }

        public GameState GameState {
            get { return GameState_; }
        }

        public bool CheckOrdinaryMoveApplicable(OrdinaryMove move) {
            return Harmonics.Any((h) => h.Board.GameState == GameState.GameStillGoing &&
                                 h.Board.CheckOrdinaryMoveApplicable(move));
        }

        public void ApplyOrdinaryMove(OrdinaryMove move) {
            bool applied = false;
            foreach (QuantumHarmonic harmonic in Harmonics) {
                if (harmonic.Board.CheckOrdinaryMoveApplicable(move)) {
                    harmonic.Board.ApplyOrdinaryMove(move);
                    applied = true;
                }
            }
            UpdateQuantumCheckboard();
            AssertionException.Assert(applied, "Ordinary move couldn't be applied on any harmonic");
        }

        public bool CheckQuantumMoveApplicable(QuantumMove move) {
            return Harmonics.Any((h) => h.Board.GameState == GameState.GameStillGoing &&
                                 h.Board.CheckQuantumMoveApplicable(move));
        }

        public void ApplyQuantumMove(QuantumMove move) {
            bool applied = false;
            var new_harmonics = new List<QuantumHarmonic>();
            foreach (QuantumHarmonic harmonic in Harmonics) {
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
            Harmonics = new_harmonics;
            UpdateQuantumCheckboard();
            AssertionException.Assert(applied, "Quantum move couldn't be applied on any harmonic");
        }

        public void RegisterVictory(Player player) {
            foreach (QuantumHarmonic harmonic in Harmonics) {
                if (harmonic.Board.GameState == GameState.GameStillGoing) {
                    harmonic.Board.RegisterVictory(player);
                }
            }
            UpdateGameState();
        }
    }
}