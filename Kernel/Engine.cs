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
            LastMovePositions_ = new Position[0];
        }

        public QuantumChessEngine(QuantumChessboard quantum_chessboard, Player active_player, DateTime creation_time,
                                 Position[] last_move_positions)
        {
            QuantumChessboard_ = quantum_chessboard;
            ActivePlayer_ = active_player;
            CreationTime_ = creation_time;
            LastMovePositions_ = last_move_positions;
        }

        private readonly QuantumChessboard QuantumChessboard_;
        private Player ActivePlayer_;
        private DateTime CreationTime_;
        private Position[] LastMovePositions_;

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

        public Position[] LastMovePositions {
            get { return LastMovePositions_; }
        }

        public void Submit(QuantumChessMove move) {
            if (move.ActorPlayer != ActivePlayer_)
                MoveProcessException.Throw("Waiting for another player's move");

            if (move is CapitulateMove) {
                QuantumChessboard_.RegisterVictory(PlayerUtils.InvertPlayer(ActivePlayer_));
                LastMovePositions_ = new Position[0];
            } else if (move is AgreeToTieMove) {
                QuantumChessboard_.RegisterTie();
                LastMovePositions_ = new Position[0];
            } else if (move is OrdinaryMove) {
                var omove = move as OrdinaryMove;
                if (QuantumChessboard_.CheckOrdinaryMoveApplicable(omove)) {
                    QuantumChessboard_.ApplyOrdinaryMove(omove);
                    ActivePlayer_ = PlayerUtils.InvertPlayer(ActivePlayer_);
                    LastMovePositions_ = new Position[]{omove.Source, omove.Target};
                } else {
                    MoveProcessException.Throw("Move is inapplicable on all harmonics");
                }
            } else if (move is QuantumMove) {
                var qmove = move as QuantumMove;
                if (QuantumChessboard_.CheckQuantumMoveApplicable(qmove)) {
                    QuantumChessboard_.ApplyQuantumMove(qmove);
                    ActivePlayer_ = PlayerUtils.InvertPlayer(ActivePlayer_);
                    if (qmove.Middle.HasValue)
                        LastMovePositions_ = new Position[]{qmove.Source, qmove.Middle.Value, qmove.Target};
                    else
                        LastMovePositions_ = new Position[]{qmove.Source, qmove.Target};
                } else {
                    MoveProcessException.Throw("Quantum move is inapplicable on all harmonics");
                }
            } else if (move is CastleMove) {
                var cmove = move as CastleMove;
                if (QuantumChessboard_.CheckCastleMoveApplicable(cmove)) {
                    QuantumChessboard_.ApplyCastleMove(cmove);
                    ActivePlayer_ = PlayerUtils.InvertPlayer(ActivePlayer_);
                    int c = cmove.ActorPlayer == Player.White ? 0 : 7;
                    LastMovePositions_ = new Position[2];
                    LastMovePositions_[0] = Position.FromCoords(4, c);
                    if (cmove.CastleType == CastleType.Left)
                        LastMovePositions_[1] = Position.FromCoords(0, c);
                    else
                        LastMovePositions_[1] = Position.FromCoords(7, c);
                } else {
                    MoveProcessException.Throw("Castle is inapplicable on all harmonics");
                }
            } else {
                AssertionException.Assert(false, $"Unsupported move type: {move.GetType().Name}");
            }
        }
    }
}