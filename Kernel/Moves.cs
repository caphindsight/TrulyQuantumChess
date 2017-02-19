using System;

using TrulyQuantumChess.Kernel.Chess;

namespace TrulyQuantumChess.Kernel.Moves {
    public abstract class QuantumChessMove {
        public abstract Player ActorPlayer { get; }
    }

    public sealed class CapitulateMove : QuantumChessMove {
        public CapitulateMove(Player actorPlayer) {
            ActorPlayer_ = actorPlayer;
        }

        private Player ActorPlayer_;

        public override Player ActorPlayer {
            get { return ActorPlayer_; }
        }
    }

    public sealed class OrdinaryMove : QuantumChessMove {
        public OrdinaryMove(Piece actorPiece, Position source, Position target) {
            ActorPiece_ = actorPiece;
            Source_ = source;
            Target_ = target;
        }

        private readonly Piece ActorPiece_;
        private readonly Position Source_;
        private readonly Position Target_;

        public Piece ActorPiece {
            get { return ActorPiece_; }
        }

        public Position Source {
            get { return Source_; }
        }

        public Position Target {
            get { return Target_; }
        }

        public override Player ActorPlayer {
            get { return ActorPiece_.Player; }
        }
    }

    public sealed class SingleQuantumMove : QuantumChessMove {
        public SingleQuantumMove(Piece actorPiece, Position source, Position target) {
            ActorPiece_ = actorPiece;
            Source_ = source;
            Target_ = target;
        }

        private readonly Piece ActorPiece_;
        private readonly Position Source_;
        private readonly Position Target_;

        public Piece ActorPiece {
            get { return ActorPiece_; }
        }

        public Position Source {
            get { return Source_; }
        }

        public Position Target {
            get { return Target_; }
        }

        public override Player ActorPlayer {
            get { return ActorPiece_.Player; }
        }
    }

    public sealed class DoubleQuantumMove : QuantumChessMove {
        public DoubleQuantumMove(Piece actorPiece, Position source, Position middle, Position target) {
            ActorPiece_ = actorPiece;
            Source_ = source;
            Middle_ = middle;
            Target_ = target;
        }

        private readonly Piece ActorPiece_;
        private readonly Position Source_;
        private readonly Position Middle_;
        private readonly Position Target_;

        public Piece ActorPiece {
            get { return ActorPiece_; }
        }

        public Position Source {
            get { return Source_; }
        }

        public Position Middle {
            get { return Middle_; }
        }

        public Position Target {
            get { return Target_; }
        }

        public override Player ActorPlayer {
            get { return ActorPiece_.Player; }
        }
    }
}
