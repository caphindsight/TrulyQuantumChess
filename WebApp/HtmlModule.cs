using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class HtmlModule : NancyModule {
        public HtmlModule()
            : base("/")
        {
            Get["/"] = Hello;
        }

        private dynamic Hello(dynamic args) {
            var model = new {
                Name = "Nancy"
            };
            return View["Hello.sshtml", model];
        }
    }
}