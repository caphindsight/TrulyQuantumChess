using System;

using TrulyQuantumChess.Kernel.Errors;

using Nancy;
using Nancy.ModelBinding;

namespace TrulyQuantumChess.WebApp {
    public class ApiModule : NancyModule {
        public ApiModule()
            : base("/api")
        {
            Get["/new_game"] = NewGame;
            Get["/game_info"] = GameInfo;
            Post["/submit_move"] = SubmitMove;
        }

        private dynamic NewGame(dynamic args) {
            var game_id = Games.NewGameId();
            var new_game_response = new Model.NewGameResponse() {
                GameId = game_id.Value
            };
            return Response.AsJson<Model.NewGameResponse>(new_game_response);
        }

        private dynamic GameInfo(dynamic args) {
            var game_id = new GameId(Request.Query["gameId"]);
            Game game = Games.FindGame(game_id);
            return Response.AsJson<Model.InfoResponse>(game.Info());
        }

        private dynamic SubmitMove(dynamic args) {
            Model.MoveRequest request = this.Bind<Model.MoveRequest>();
            try {
                var game_id = new GameId(request.GameId);
                Games.FindGame(game_id).Submit(request);
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