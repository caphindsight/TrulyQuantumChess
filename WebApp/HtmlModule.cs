using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class HtmlModule : NancyModule {
        public HtmlModule()
            : base(WebAppConfig.Instance.Prefix)
        {
            Get["/"] = Index;
            Get["/play"] = Play;
        }

        private dynamic Index(dynamic args) {
            return View["Index.sshtml", new { WebAppConfig.Instance.Prefix }];
        }

        private dynamic Play(dynamic args) {
            var model = new {
                WebAppConfig.Instance.Prefix,
                GameId = Request.Query["gameId"],
                PiecesCollection = WebAppConfig.Instance.Pieces.Collection,
                PawnWidthRatio = WebAppConfig.Instance.Pieces.WidthRatios.Pawn,
                KnightWidthRatio = WebAppConfig.Instance.Pieces.WidthRatios.Knight,
                BishopWidthRatio = WebAppConfig.Instance.Pieces.WidthRatios.Bishop,
                RookWidthRatio = WebAppConfig.Instance.Pieces.WidthRatios.Rook,
                QueenWidthRatio = WebAppConfig.Instance.Pieces.WidthRatios.Queen,
                KingWidthRatio = WebAppConfig.Instance.Pieces.WidthRatios.King,
            };
            return View["Play.sshtml", model];
        }
    }
}