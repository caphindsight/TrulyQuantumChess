using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Moves;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Quantize;
using TrulyQuantumChess.Kernel.Engine;

using Nancy;
using Nancy.ModelBinding;

namespace TrulyQuantumChess.WebApp {
    public class ApiModule : NancyModule {
        public ApiModule()
            : base(WebAppConfig.Instance.Prefix + "/api")
        {
            Get["/new_game"] = (args) => NewGame(args, new CancellationToken()).Result;
            Get["/game_info"] = (args) => GameInfo(args, new CancellationToken()).Result;
            Post["/submit_move"] = (args) => SubmitMove(args, new CancellationToken()).Result;

            // This doesn't work yet, because we would have to obtain exclusive locks for each chessboard
            // Although, we probably have to obtain them anyway... Future will tell.
            // Get["/new_game", true] = NewGame;
            // Get["/game_info", true] = GameInfo;
            // Post["/submit_move", true] = SubmitMove;
        }

        private async Task<dynamic> NewGame(dynamic args, CancellationToken cancellation_token) {
            var engine = new QuantumChessEngine();
            string game_id = await WebAppManagers.DatabaseManager.InsertEngine(engine);
            var new_game_response = new Model.NewGameResponse() {
                GameId = game_id
            };
            return Response.AsJson(new_game_response);
        }

        private async Task<dynamic> GameInfo(dynamic args, CancellationToken cancellation_token) {
            string game_id = Request.Query["gameId"];
            QuantumChessEngine engine = await WebAppManagers.DatabaseManager.RequestEngine(game_id);

            var response = new Model.InfoResponse();
            response.ActivePlayer = PlayerUtils.ToString(engine.ActivePlayer);
            response.GameState = GameStateUtils.ToString(engine.GameState);
            response.Squares = new Dictionary<String, Model.InfoResponse.SquareEncoded>();
            for (int i = 0; i < 64; i++) {
                Position pos = Position.FromIndex(i);
                QuantumPiece qpiece = engine.QuantumChessboard.GetQuantumPiece(pos);
                if (qpiece.Piece.HasValue) {
                    response.Squares[pos.ToString()] = new Model.InfoResponse.SquareEncoded() {
                        Player = PlayerUtils.ToString(qpiece.Piece.Value.Player),
                        Piece = PieceTypeUtils.ToString(qpiece.Piece.Value.PieceType),
                        Probability = qpiece.Probability
                    };
                } else {
                    response.Squares[pos.ToString()] = null;
                }
            }

            return Response.AsJson(response);
        }

        private async Task<dynamic> SubmitMove(dynamic args, CancellationToken cancellation_token) {
            Model.MoveRequest request = this.Bind<Model.MoveRequest>();
            try {
                QuantumChessEngine engine = await WebAppManagers.DatabaseManager.RequestEngine(request.GameId);
                QuantumChessMove move = request.Parse(engine);
                engine.Submit(move);
                await WebAppManagers.DatabaseManager.UpdateEngine(request.GameId, engine);
                return new {
                    Success = true
                };
            } catch (QuantumChessException e) {
                return new {
                    Success = false,
                    Message = e.Message
                };
            }
        }
    }
}