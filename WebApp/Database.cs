using System;
using System.Threading.Tasks;

using TrulyQuantumChess.Kernel.Errors;
using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Quantize;
using TrulyQuantumChess.Kernel.Engine;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core;

namespace TrulyQuantumChess.WebApp {
    public class MongoException : Exception {
        public MongoException(string message)
            : base(message)
        {}
    }

    public class Game {
        public ObjectId Id;
        public QuantumChessEngine Engine;

        public Game(ObjectId id, QuantumChessEngine engine) {
            Id = id;
            Engine = engine;
        }
    }

    public interface IDatabaseManager {
        Task<QuantumChessEngine> RequestEngine(string gameId);
        Task<string> InsertEngine(QuantumChessEngine engine);
        Task UpdateEngine(string gameId, QuantumChessEngine engine);
    }

    public class MongoManager : IDatabaseManager {
        private readonly IMongoClient Client_;
        private readonly IMongoDatabase Database_;
        private IMongoCollection<BsonDocument> ActiveGames_;

        public MongoManager() {
            Client_ = new MongoClient(WebAppConfig.Instance.Mongo.ConnectionString);
            Database_ = Client_.GetDatabase(WebAppConfig.Instance.Mongo.Database);
            Database_.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();
            Console.WriteLine("Established connection to mongo db");
            ActiveGames_ = Database_.GetCollection<BsonDocument>("active_games");
        }

        public async Task<QuantumChessEngine> RequestEngine(string gameId) {
            var filter = new BsonDocument();
            filter.Set("_id", new ObjectId(gameId));
            using (var cursor = await ActiveGames_.FindAsync(filter)) {
                while (await cursor.MoveNextAsync()) {
                    var batch = cursor.Current;
                    foreach (var document in batch) {
                        return ChessBsonSerializationUtils.Deserialize(document).Engine;
                    }
                }
            }
            throw new MongoException($"No game with gameId \"{gameId}\" found");
        }

        public async Task<string> InsertEngine(QuantumChessEngine engine) {
            var game = new Game(new ObjectId(), engine);
            await ActiveGames_.InsertOneAsync(ChessBsonSerializationUtils.Serialize(game));
            return game.Id.ToString();
        }

        public async Task UpdateEngine(string gameId, QuantumChessEngine engine) {
            var filter = new BsonDocument();
            filter.Set("_id", new ObjectId(gameId));
            var replacement = new Game(new ObjectId(gameId), engine);
            await ActiveGames_.FindOneAndReplaceAsync(filter, ChessBsonSerializationUtils.Serialize(replacement));
        }
    }

    public static class ChessBsonSerializationUtils {
        public static BsonDocument Serialize(Game game) {
            throw new NotImplementedException();
        }

        public static Game Deserialize(BsonDocument document) {
            throw new NotImplementedException();
        }
    }
}