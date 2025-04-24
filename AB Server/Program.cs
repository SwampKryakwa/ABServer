using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace AB_Server
{
    class Server
    {
        public static HttpListener listener;

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static Dictionary<string, Room> Rooms = new();

        static Dictionary<string, Game> GIDToGame = new();
        private static Random random = new Random();


        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                try
                {
                    // Will wait here until we hear from a connection
                    HttpListenerContext ctx = await listener.GetContextAsync();

                    // Peel out the requests and response objects
                    HttpListenerRequest request = ctx.Request;
                    HttpListenerResponse resp = ctx.Response;

                    // Write the response info
                    string disableSubmit = !runServer ? "disabled" : "";

                    StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
                    string body = reader.ReadToEnd();

                    if (request.HttpMethod == "GET")
                    {
                        try
                        {
                            if (request.Url is Uri url)
                            {
                                string key = url.ToString();

                                dynamic postedJson = JObject.Parse(body);

                                string requestedResource = request.Url.ToString().Split('/')[^1];

                                // Decode the key value
                                string[] dontlog = { "getupdates", "getplayerlist", "getallready", "checkstarted", "checkgamestarted", "getroomupdates", "" };
                                string[] validkeys = { "ping", "createroom", "getroomlist", "joinroom", "leaveroom", "getmyposition", "updateready", "getplayerlist", "getallready", "checkready", "checkstarted", "getroomupdates", "checkgamestarted", "getgameinfo", "startroom", "newgame", "getsession", "join", "getroomnicknames", "getupdates", "sendchatmessage", "checkturnstart", "getmoves", "answer", "move", "leave", "roomspectate", "gamespectate", "redeemcode" };
                                if (!validkeys.Contains(requestedResource))
                                {
                                    resp.StatusCode = 400; // Bad Request
                                    resp.Close();
                                    continue;
                                }
                                if (!dontlog.Contains(requestedResource))
                                {
                                    Console.WriteLine(key);
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine(body);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    //if (OperatingSystem.IsWindows())
                                    //    Console.ForegroundColor = ConsoleColor.White;
                                    //else
                                    //    Console.ForegroundColor = ConsoleColor.Black;
                                }

                                JObject answer = new();

                                string GID;
                                Game game;
                                int player;
                                switch (requestedResource)
                                {
                                    case "ping":
                                        answer.Add("response", true);
                                        break;

                                    case "createroom":
                                        string room = RandomString(8);
                                        while (Rooms.ContainsKey(room))
                                            room = RandomString(8);
                                        Rooms.Add(room, new Room((short)postedJson["playerCount"], (string)postedJson["roomName"], (bool)postedJson["isBotRoom"]));
                                        answer.Add("room", room);
                                        break;

                                    case "getroomlist":
                                        answer.Add("rooms", new JArray(Rooms.Where(x => !x.Value.Started && !x.Value.IsBotRoom).Select(r => new JObject { ["roomKey"] = r.Key, ["roomName"] = r.Value.RoomName, ["roomPlayers"] = new JArray(r.Value.UserNames) })));
                                        break;

                                    case "joinroom":
                                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
                                        {
                                            if (Rooms[(string)postedJson["roomName"]].AddPlayer((long)postedJson["UUID"], postedJson["userName"].ToString()))
                                            {
                                                answer.Add("success", true);
                                                answer.Add("player", true);
                                            }
                                            else
                                            {
                                                Rooms[(string)postedJson["roomName"]].Spectate((long)postedJson["UUID"]);
                                                answer.Add("success", true);
                                                answer.Add("player", true);
                                            }
                                            break;
                                        }
                                        answer.Add("success", false);
                                        break;

                                    case "roomspectate":
                                        Rooms[(string)postedJson["roomName"]].Spectate((long)postedJson["UUID"]);
                                        break;

                                    case "leaveroom":
                                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
                                        {
                                            try { Rooms[(string)postedJson["roomName"]].RemovePlayer((long)postedJson["UUID"]); } catch { }
                                            if (!Rooms[(string)postedJson["roomName"]].Players.Any(x => x != null)) Rooms.Remove((string)postedJson["roomName"]);
                                        }
                                        break;

                                    case "getmyposition":
                                        answer.Add("position", Rooms[(string)postedJson["roomName"]].GetPosition((long)postedJson["UUID"]));
                                        break;

                                    case "updateready":
                                        try { answer.Add("canStart", Rooms[(string)postedJson["roomName"]].UpdateReady((long)postedJson["UUID"], (bool)postedJson["isReady"])); }
                                        catch
                                        {
                                            Console.WriteLine(postedJson["roomName"]);
                                            Console.WriteLine(postedJson["UUID"]);
                                            Console.WriteLine(postedJson["isReady"]);
                                        }
                                        break;

                                    case "getplayerlist":
                                        answer.Add("players", new JArray(Rooms[(string)postedJson["roomName"]].UserNames));
                                        break;

                                    case "getallready":
                                        answer.Add("ready", new JArray(Rooms[(string)postedJson["roomName"]].IsReady));
                                        break;

                                    case "checkready":
                                        answer.Add("canStart", Rooms[(string)postedJson["roomName"]].AreAllReady());
                                        break;

                                    case "checkstarted":
                                        answer.Add("started", Rooms[(string)postedJson["roomName"]].Started);
                                        break;

                                    case "getroomupdates":
                                        answer.Add("updates", Rooms[(string)postedJson["roomName"]].GetUpdates((long)postedJson["uuid"]));
                                        break;

                                    case "checkgamestarted":
                                        answer.Add("started", GIDToGame[(string)postedJson["roomName"]].Started);
                                        break;

                                    case "getgameinfo":
                                        answer.Add("players", new JArray(
                                            GIDToGame[(string)postedJson["roomName"]].Players.Select(x => new JObject
                                            {
                                                ["nickname"] = x.DisplayName,
                                                ["avatar"] = x.Avatar,
                                                ["partnerType"] = (int)x.BakuganOwned[0].Type,
                                                ["partnerAttribute"] = (int)x.BakuganOwned[0].BaseAttribute,
                                                ["partnerTreatment"] = (int)x.BakuganOwned[0].Treatment
                                            })
                                            ));
                                        break;

                                    case "startroom":
                                        Rooms[(string)postedJson["roomName"]].Started = true;
                                        if (Rooms[(string)postedJson["roomName"]].Players.Contains((long)postedJson["UUID"]))
                                        {
                                            answer.Add("successful", true);
                                        }
                                        else
                                            answer.Add("successful", false);
                                        break;

                                    case "newgame":
                                        game = new((byte)postedJson["playerCount"]);
                                        room = (string)postedJson["roomName"];
                                        GIDToGame.Add(room, game);

                                        answer.Add("gid", room);
                                        break;

                                    case "getsession":
                                        answer.Add("UUID", random.NextInt64());

                                        break;

                                    case "join":
                                        GID = (string)postedJson["gid"];
                                        game = GIDToGame[GID];
                                        answer.Add("pid", game.AddPlayer((JObject)postedJson["deck"], (long)postedJson["UUID"], (string)postedJson["name"], (byte)postedJson["ava"]));
                                        answer.Add("playerCount", game.Players.Where(x => x != null).Count());
                                        if (game.PlayerCount == game.Players.Count)
                                            new Thread(game.Initiate).Start();
                                        break;

                                    case "gamespectate":
                                        GID = (string)postedJson["gid"];
                                        game = GIDToGame[GID];
                                        answer.Add("playerCount", game.Players.Where(x => x != null).Count());
                                        game.AddSpectator((long)postedJson["UUID"]);
                                        break;

                                    case "getroomnicknames":
                                        answer.Add("nicknames", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].Players.Select(x => x.DisplayName)));
                                        answer.Add("avas", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].Players.Select(x => x.Avatar)));
                                        break;

                                    case "getupdates":
                                        answer.Add("updates", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].GetEvents((int)postedJson["pid"])));
                                        break;

                                    case "sendchatmessage":
                                        GIDToGame[(string)postedJson["gid"]].ThrowEvent(new JObject
                                        {
                                            { "Type", "NewMessage" },
                                            { "Sender", postedJson["pid"] },
                                            { "Text", postedJson["text"] }
                                        });
                                        break;

                                    case "checkturnstart":
                                        answer.Add("turnplayer", new JObject { { "Type", "PlayerTurnStart" }, { "PID", GIDToGame[(string)postedJson["gid"]].ActivePlayer } });
                                        break;

                                    case "getmoves":
                                        GID = (string)postedJson["gid"];
                                        game = GIDToGame[GID];
                                        player = (int)postedJson["playerID"];

                                        answer.Add("moves", game.GetPossibleMoves(player));

                                        break;

                                    case "answer":
                                        GID = (string)postedJson["gid"];
                                        game = GIDToGame[GID];
                                        player = (int)postedJson["playerID"];

                                        bool hasStarted = game.Started;
                                        game.IncomingSelection[player] = postedJson;
                                        game.OnAnswer[player]?.Invoke();

                                        break;

                                    case "move":
                                        GID = (string)postedJson["gid"];
                                        game = GIDToGame[GID];
                                        player = (int)postedJson["playerID"];

                                        if (!game.doNotMakeStep)
                                            game.GameStep(postedJson);
                                        break;

                                    case "leave":
                                        GID = (string)postedJson["gid"];
                                        game = GIDToGame[GID];

                                        game.Left++;
                                        if (game.Left == game.PlayerCount)
                                        {
                                            GIDToGame.Remove(GID);
                                        }
                                        break;

                                    case "redeemcode":
                                        JObject codes = JObject.Parse(File.ReadAllText(@"codes.json"));
                                        if (codes.ContainsKey((string)postedJson["code"]))
                                            answer.Add("reward", codes[(string)postedJson["code"]]);
                                        break;
                                }
                                if (!dontlog.Contains(requestedResource))
                                    Console.WriteLine();
                                byte[] data = Encoding.UTF8.GetBytes(answer.ToString());
                                resp.ContentType = "text/html";
                                resp.ContentEncoding = Encoding.UTF8;
                                resp.ContentLength64 = data.LongLength;

                                // Write out to the response stream (asynchronously), then close it
                                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                resp.Close();
                            }
                        }
                        catch (Exception e) { Console.WriteLine(e); }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        static void Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            listener.Start();

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();

            /*//// HTTP server port
            //int port = 8080;
            //if (args.Length > 0)
            //    port = int.Parse(args[0]);

            //Console.WriteLine($"HTTP server port: {port}");
            //Console.WriteLine($"HTTP server website: http://localhost:{port}/api/index.html");

            //Console.WriteLine();

            //// Create a new HTTP server
            //var server = new HttpCacheServer(IPAddress.Any, port);

            //// Start the server
            //Console.Write("Server starting...");
            //server.Start();
            //Console.WriteLine("Done!");

            //Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            //// Perform text input
            //for (; ; )
            //{
            //    string line = Console.ReadLine();
            //    if (string.IsNullOrEmpty(line))
            //        break;

            //    // Restart the server
            //    if (line == "!")
            //    {
            //        Console.Write("Server restarting...");
            //        server.Restart();
            //        Console.WriteLine("Done!");
            //    }
            //}

            //// Stop the server
            //Console.Write("Server stopping...");
            //server.Stop();
            //Console.WriteLine("Done!");*/
        }
    }
}