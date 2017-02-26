using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TrulyQuantumChess.Kernel.Chess;
using TrulyQuantumChess.Kernel.Quantize;
using TrulyQuantumChess.Kernel.Engine;

using MongoDB.Bson;
using MongoDB.Driver;


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

    public struct GameInfo {
        public string GameId;
        public TimeSpan LastModification;
        public string LastModificationString {
            get {
                if (LastModification > TimeSpan.FromSeconds(90))
                    return $"{Convert.ToInt32(LastModification.TotalMinutes)} mins";
                else
                    return $"{Convert.ToInt32(LastModification.TotalSeconds)} secs";
            }
        }
    }

    public interface IDatabaseManager {
        Task<List<GameInfo>> RequestActiveGames();
        Task<QuantumChessEngine> RequestEngine(string gameId);
        Task<string> InsertEngine(QuantumChessEngine engine);
        Task UpdateEngine(string gameId, QuantumChessEngine engine);
        Task CleanOldEntities(DateTime modification_instant);
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

        private static readonly BsonDocument EmptyFilter_ =
            new BsonDocument();

        private static BsonDocument FilterById(ObjectId id) {
            var filter = new BsonDocument();
            filter.Set("_id", id);
            return filter;
        }

        private static BsonDocument FilterById(string id) {
            return FilterById(new ObjectId(id));
        }

        public async Task<List<GameInfo>> RequestActiveGames() {
            var res = new List<GameInfo>();
            using (var cursor = await ActiveGames_.FindAsync(EmptyFilter_)) {
                while (await cursor.MoveNextAsync()) {
                    var batch = cursor.Current;
                    foreach (var document in batch) {
                        res.Add(new GameInfo() {
                            GameId = document["_id"].AsObjectId.ToString(),
                            LastModification = DateTime.UtcNow - document["last_modification_time"].ToUniversalTime(),
                        });
                    }
                }
            }
            res.Sort((x, y) => x.LastModification.CompareTo(y.LastModification));
            return res;
        }

        public async Task<QuantumChessEngine> RequestEngine(string gameId) {
            using (var cursor = await ActiveGames_.FindAsync(FilterById(gameId))) {
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
            var game = new Game(ObjectId.GenerateNewId(), engine);
            await ActiveGames_.InsertOneAsync(ChessBsonSerializationUtils.Serialize(game));
            return game.Id.ToString();
        }

        public async Task UpdateEngine(string gameId, QuantumChessEngine engine) {
            var replacement = new Game(new ObjectId(gameId), engine);
            await ActiveGames_.FindOneAndReplaceAsync(FilterById(gameId), ChessBsonSerializationUtils.Serialize(replacement));
        }

        public async Task CleanOldEntities(DateTime modification_instant) {
            using (var cursor = await ActiveGames_.FindAsync(EmptyFilter_)) {
                while (await cursor.MoveNextAsync()) {
                    var batch = cursor.Current;
                    foreach (var document in batch) {
                        DateTime last_access_time = document["last_modification_time"].ToUniversalTime();
                        if (last_access_time < modification_instant) {
                            Console.WriteLine($"{last_access_time} < {modification_instant}");
                            await ActiveGames_.DeleteOneAsync(FilterById(document["_id"].AsObjectId));
                        }
                    }
                }
            }
        }
    }

    public static class ChessBsonSerializationUtils {
        public static BsonDocument Serialize(Game game) {
            var document = new BsonDocument();
            document.Set("_id", new BsonObjectId(game.Id));
            document.Set("active_player", PlayerUtils.ToString(game.Engine.ActivePlayer));
            document.Set("game_state", GameStateUtils.ToString(game.Engine.QuantumChessboard.GameState));
            document.Set("last_modification_time", DateTime.Now);

            var bson_harmonics = new BsonArray();
            foreach (QuantumHarmonic harmonic in game.Engine.QuantumChessboard.Harmonics) {
                var bson_harmonic = new BsonDocument();
                bson_harmonic.Set("harmonic_state", GameStateUtils.ToString(harmonic.Board.GameState));
                bson_harmonic.Set("degeneracy", Convert.ToInt64(harmonic.Degeneracy));

                var bson_chessboard = new BsonArray();
                for (int i = 0; i < 64; i++) {
                    Piece? piece = harmonic.Board[i];
                    if (piece.HasValue) {
                        var bson_piece = new BsonDocument();
                        bson_piece.Set("player", PlayerUtils.ToString(piece.Value.Player));
                        bson_piece.Set("piece", PieceTypeUtils.ToString(piece.Value.PieceType));
                        bson_chessboard.Add(bson_piece);
                    } else {
                        bson_chessboard.Add(BsonNull.Value);
                    }
                }

                bson_harmonic.Set("chessboard", bson_chessboard);
                bson_harmonics.Add(bson_harmonic);
            }

            document.Set("harmonics", bson_harmonics);

            return document;
        }

        public static Game Deserialize(BsonDocument document) {
            ObjectId id = document["_id"].AsObjectId;
            Player active_player = PlayerUtils.FromString(document["active_player"].AsString);
            GameState game_state = GameStateUtils.FromString(document["game_state"].AsString);

            var harmonics = new List<QuantumHarmonic>();
            foreach (BsonValue bson_harmonic_val in document["harmonics"].AsBsonArray) {
                BsonDocument bson_harmonic = bson_harmonic_val.AsBsonDocument;
                GameState harmonic_state = GameStateUtils.FromString(bson_harmonic["harmonic_state"].AsString);
                ulong degeneracy = Convert.ToUInt64(bson_harmonic["degeneracy"].AsInt64);
                BsonArray bson_chessboard = bson_harmonic["chessboard"].AsBsonArray;
                var chessboard = Chessboard.EmptyChessboard(harmonic_state);
                for (int i = 0; i < 64; i++) {
                    BsonValue bson_square_val = bson_chessboard[i];
                    if (bson_square_val.IsBsonNull) {
                        chessboard[i] = null;
                    } else {
                        BsonDocument bson_square = bson_square_val.AsBsonDocument;
                        Player square_player = PlayerUtils.FromString(bson_square["player"].AsString);
                        PieceType square_piece = PieceTypeUtils.FromString(bson_square["piece"].AsString);
                        chessboard[i] = new Piece(square_player, square_piece);
                    }
                }
                harmonics.Add(new QuantumHarmonic(chessboard, degeneracy));
            }

            var quantum_chessboard = new QuantumChessboard(harmonics, game_state);
            var engine = new QuantumChessEngine(quantum_chessboard, active_player);
            return new Game(id, engine);
        }
    }
}