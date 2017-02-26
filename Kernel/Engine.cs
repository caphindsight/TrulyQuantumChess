using System;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Moves;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Quantize;

namespace TrulyQuantumChess.Kernel.Engine {
    public class QuantumChessEngine {
        public QuantumChessEngine() {
            QuantumChessboard_ = QuantumChessboard.StartingQuantumChessboard();
            ActivePlayer_ = Player.White;
            CreationTime_ = DateTime.Now;
        }

        public QuantumChessEngine(QuantumChessboard quantum_chessboard, Player active_player, DateTime creation_time) {
            QuantumChessboard_ = quantum_chessboard;
            ActivePlayer_ = active_player;
            CreationTime_ = creation_time;
        }

        private readonly QuantumChessboard QuantumChessboard_;
        private Player ActivePlayer_;
        private DateTime CreationTime_;

        public QuantumChessboard QuantumChessboard {
            get { return QuantumChessboard_; }
        }

        public Player ActivePlayer {
            get { return ActivePlayer_; }
        }

        public DateTime CreationTime {
            get { return CreationTime_; }
        }

        public GameState GameState {
            get { return QuantumChessboard_.GameState; }
        }

        public void Submit(QuantumChessMove move) {
            if (move.ActorPlayer != ActivePlayer_)
                MoveProcessException.Throw("Waiting for another player's move");

            if (move is CapitulateMove) {
                QuantumChessboard_.RegisterVictory(PlayerUtils.InvertPlayer(ActivePlayer_));
            } else if (move is AgreeToTieMove) {
                QuantumChessboard_.RegisterTie();
            } else if (move is OrdinaryMove) {
                var omove = move as OrdinaryMove;
                if (QuantumChessboard_.CheckOrdinaryMoveApplicable(omove)) {
                    QuantumChessboard_.ApplyOrdinaryMove(omove);
                    ActivePlayer_ = PlayerUtils.InvertPlayer(ActivePlayer_);
                } else {
                    MoveProcessException.Throw("Move is inapplicable on all harmonics");
                }
            } else if (move is QuantumMove) {
                var qmove = move as QuantumMove;
                if (QuantumChessboard_.CheckQuantumMoveApplicable(qmove)) {
                    QuantumChessboard_.ApplyQuantumMove(qmove);
                    ActivePlayer_ = PlayerUtils.InvertPlayer(ActivePlayer_);
                } else {
                    MoveProcessException.Throw("Quantum move is inapplicable on all harmonics");
                }
            } else if (move is CastleMove) {
                var cmove = move as CastleMove;
                if (QuantumChessboard_.CheckCastleMoveApplicable(cmove)) {
                    QuantumChessboard_.ApplyCastleMove(cmove);
                    ActivePlayer_ = PlayerUtils.InvertPlayer(ActivePlayer_);
                } else {
                    MoveProcessException.Throw("Castle is inapplicable on all harmonics");
                }
            } else {
                AssertionException.Assert(false, $"Unsupported move type: {move.GetType().Name}");
            }
        }
    }
}