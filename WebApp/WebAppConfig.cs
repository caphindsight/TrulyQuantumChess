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
        public string Prefix { get; private set; }
        public MongoConnection Mongo { get; private set; }
        public double CleanAfterHours { get; private set; }
        public bool Debug { get; private set; }
        public PiecesInfo Pieces { get; private set; }
        public string DocUrl { get; private set; }
    }

    // Helper for dependency injections
    public static class WebAppManagers {
        private static readonly IDatabaseManager DatabaseManager_ =
            new MongoManager();

        public static IDatabaseManager DatabaseManager {
            get { return DatabaseManager_; }
        }
    }

    public class MongoConnection {
        public string ConnectionString { get; private set; }
        public string Database { get; private set; }
    }

    public class PiecesInfo {
        public string Collection { get; private set; }
        public PiecesWidthRatiosInfo WidthRatios { get; private set; }
    }

    public class PiecesWidthRatiosInfo {
        public double Pawn { get; private set; }
        public double Knight { get; private set; }
        public double Bishop { get; private set; }
        public double Rook { get; private set; }
        public double Queen { get; private set; }
        public double King { get; private set; }
    }
}