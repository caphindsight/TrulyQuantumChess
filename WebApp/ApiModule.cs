using System;

using Nancy;

namespace TrulyQuantumChess.WebApp {
    public class ApiModule : NancyModule {
        public ApiModule()
            : base(WebAppConfig.Instance.ApiRoute)
        {
            Get["/"] = _ => "Hello, world!";
        }
    }
}