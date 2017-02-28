using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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

        private struct RecaptchaResponseModel {
            public bool success;
            public string challenge_ts;
            public string hostname;
            public string[] error_codes;
        }

        private static readonly JavaScriptSerializer Serializer_ =
            new JavaScriptSerializer();

        private async Task<bool> ValidateCaptchaResponse(string captcha_response) {
            using (var client = new HttpClient()) {
                var values = new Dictionary<String, String> {
                    { "secret", WebAppConfig.Instance.Captcha.Secret },
                    { "response", captcha_response }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var response_string = await response.Content.ReadAsStringAsync();
                response_string = response_string.Replace("error-codes", "error_codes"); // I know this is a hack. I don't care.
                RecaptchaResponseModel model = Serializer_.Deserialize<RecaptchaResponseModel>(response_string);
                return model.success;
            }
        }

        private async Task<dynamic> NewGame(dynamic args, CancellationToken cancellation_token) {
            if (WebAppConfig.Instance.Captcha.Enabled) {
                bool captcha_validated = await ValidateCaptchaResponse(Request.Query["captcha_response"]);
                if (!captcha_validated) {
                    return 500;
                }
            }
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

            response.LastMovePositions = engine.LastMovePositions.Select((pos) => pos.ToString().ToLower()).ToArray();

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