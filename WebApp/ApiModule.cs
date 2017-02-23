using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class ApiModule : NancyModule {
        public ApiModule()
            : base("/api")
        {
            Get["/new_game"] = NewGame;
            Get["/game_info"] = GameInfo;
        }

        private dynamic NewGame(dynamic args) {
            var game_id = Games.NewGameId();
            var new_game_response = new Model.NewGameResponse() {
                GameId = game_id.Value
            };
            return Response.AsJson<Model.NewGameResponse>(new_game_response);
        }

        private dynamic GameInfo(dynamic args) {
            var game_id = new GameId(Request.Query["game_id"]);
            Game game = Games.FindGame(game_id);
            return Response.AsJson<Model.InfoResponse>(game.Info());
        }
    }
}