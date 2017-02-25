using System;
using System.Collections.Generic;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Moves;
using TrulyQuantumChess.Kernel.Quantize;
using TrulyQuantumChess.Kernel.Engine;

namespace TrulyQuantumChess.WebApp {
    public sealed class GameId {
        public readonly string Value;

        public GameId(string val) {
            Value = val;
        }

        public static bool operator == (GameId a, GameId b) {
            return a.Value == b.Value;
        }

        public static bool operator != (GameId a, GameId b) {
            return a.Value != b.Value;
        }

        public override bool Equals(object obj) {
            if (obj is GameId) {
                var game_id = obj as GameId;
                return Value == game_id.Value;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public override string ToString() {
            return $"[GameId({Value})]";
        }

        public GameId Clone() {
            return new GameId(Value);
        }
    }

    public class GameNotFoundException : QuantumChessException {
        public GameNotFoundException(string message)
            : base(message)
        {}
    }

    public class Game {
        public Game(QuantumChessEngine engine) {
            Engine_ = engine;
            LastAccess = DateTime.Now;
        }

        private readonly QuantumChessEngine Engine_;
        public DateTime LastAccess;

        public static Game NewGame() {
            var engine = new QuantumChessEngine();
            return new Game(engine);
        }

        public void Submit(Model.MoveRequest request) {
            lock (Engine_) {
                QuantumChessMove move = request.Parse(Engine_);
                Engine_.Submit(move);
            }
        }

        public Model.InfoResponse Info() {
            lock (Engine_) {
                var res = new Model.InfoResponse() {
                    GameState = GameStateUtils.GameStateToString(Engine_.GameState),
                    ActivePlayer = PlayerUtils.PlayerToString(Engine_.ActivePlayer),
                    Squares = new Dictionary<String, Model.InfoResponse.SquareEncoded>()
                };
                for (int i = 0; i < 64; i++) {
                    Position pos = Position.FromIndex(i);
                    QuantumPiece piece = Engine_.Chessboard.GetQuantumPiece(pos);
                    if (piece.Piece.HasValue) {
                        res.Squares[pos.ToString()] = new Model.InfoResponse.SquareEncoded() {
                            Player = PlayerUtils.PlayerToString(piece.Piece.Value.Player),
                            Piece = PieceTypeUtils.PieceTypeToString(piece.Piece.Value.PieceType),
                            Probability = piece.Probability
                        };
                    } else {
                        res.Squares[pos.ToString()] = null;
                    }
                }
                return res;
            }
        }
    }

    public static class Games {
        private static readonly Dictionary<GameId, Game> Games_ = new Dictionary<GameId, Game>();

        public static Game FindGame(GameId game_id) {
            lock (Games_) {
                Game game;
                if (Games_.TryGetValue(game_id, out game)) {
                    game.LastAccess = DateTime.Now;
                    return game;
                } else {
                    throw new GameNotFoundException($"Game not found: {game_id}");
                }
            }
        }

        public static GameId NewGameId() {
            lock (Games_) {
                var id = new GameId(Guid.NewGuid().ToString());
                Games_[id] = Game.NewGame();
                return id;
            }
        }

        public static void Clean() {
            lock (Games_) {
                var keys = new List<GameId>();
                foreach (var key in Games_.Keys)
                    keys.Add(key.Clone());

                foreach (GameId key in keys) {
                    Game game = Games_[key];
                    TimeSpan ts = DateTime.Now - game.LastAccess;
                    if (ts.TotalHours > WebAppConfig.Instance.CleanAfterHours)
                        Games_.Remove(key);
                }
            }
        }
    }
}