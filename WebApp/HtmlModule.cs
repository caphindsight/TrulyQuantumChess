using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class HtmlModule : NancyModule {
        public HtmlModule()
            : base(WebAppConfig.Instance.Prefix)
        {
            Get["/"] = Index;
            Get["/play"] = Play;
            Get["/rules"] = Rules;
        }

        private dynamic Index(dynamic args) {
            return View["Index.sshtml", new { WebAppConfig.Instance.Prefix }];
        }

        private dynamic Play(dynamic args) {
            var model = new {
                WebAppConfig.Instance.Prefix,
                GameId = Request.Query["gameId"]
            };
            return View["Play.sshtml", model];
        }

        private dynamic Rules(dynamic args) {
            return View["Rules.sshtml", new { WebAppConfig.Instance.Prefix }];
        }
    }
}