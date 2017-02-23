using System;
using System.IO;
using System.Web.Script.Serialization;

namespace TrulyQuantumChess.WebApp {
    public class WebAppConfig {
        private static readonly WebAppConfig Instance_;

        public static WebAppConfig Instance {
            get { return Instance_; }
        }

        static WebAppConfig() {
            string input = File.ReadAllText("WebAppConfig.json");
            var jss = new JavaScriptSerializer();
            Instance_ = jss.Deserialize<WebAppConfig>(input);
        }

        public string ListenUrl { get; private set; }
        public string ApiRoute { get; private set; }
    }
}