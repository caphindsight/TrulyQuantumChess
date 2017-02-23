using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class ApiModule : NancyModule {
        public ApiModule()
            : base("/api")
        {
            Get["/"] = _ => "Hello, world!";
        }
    }
}