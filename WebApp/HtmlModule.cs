using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class HtmlModule : NancyModule {
        public HtmlModule()
            : base("/")
        {
            Get["/"] = Index;
            Get["/play"] = Play;
        }

        private dynamic Index(dynamic args) {
            return View["Index.sshtml"];
        }

        private dynamic Play(dynamic args) {
            return View["Play.sshtml"];
        }
    }
}