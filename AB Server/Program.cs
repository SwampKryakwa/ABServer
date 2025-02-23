using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

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
                                if (requestedResource != "getupdates" && requestedResource != "getplayerlist" && requestedResource != "getallready" && requestedResource != "checkstarted" && requestedResource != "")
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
                                        Rooms.Add(room, new Room((short)postedJson["playerCount"], (string)postedJson["roomName"]));
                                        answer.Add("room", room);
                                        break;

                                    case "getroomlist":
                                        answer.Add("rooms", new JArray(Rooms.Where(x => !x.Value.Started).Select(r => new JObject { ["roomKey"] = r.Key, ["roomName"] = r.Value.RoomName, ["roomPlayers"] = new JArray(r.Value.UserNames) })));
                                        break;

                                    case "joinroom":
                                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
                                        {
                                            answer.Add("success", Rooms[(string)postedJson["roomName"]].AddPlayer((long)postedJson["UUID"], postedJson["userName"].ToString()));
                                            break;
                                        }
                                        answer.Add("success", false);
                                        break;

                                    case "leaveroom":
                                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
                                        {
                                            Rooms[(string)postedJson["roomName"]].RemovePlayer((long)postedJson["UUID"]);
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
                                        answer.Add("pid", game.AddPlayer((JObject)postedJson["deck"], (long)postedJson["UUID"], (string)postedJson["name"]));
                                        answer.Add("playerCount", game.Players.Where(x => x != null).Count());
                                        if (game.PlayerCount == game.Players.Count)
                                        {
                                            Console.WriteLine("starting");
                                            new Thread(game.Initiate).Start();
                                        }

                                        break;

                                    case "getroomnicknames":
                                        answer.Add("nicknames", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].Players.Select(x => x.DisplayName)));
                                        break;

                                    case "getupdates":
                                        answer.Add("updates", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].GetUpdates((int)postedJson["pid"])));
                                        break;

                                    case "sendchatmessage":
                                        foreach (var updates in GIDToGame[(string)postedJson["gid"]].NewEvents)
                                            updates.Add(new JObject
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
                                        game.AwaitingAnswers[player]?.Invoke();

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
                                }
                                if (requestedResource != "getupdates" && requestedResource != "getplayerlist" && requestedResource != "getallready" && requestedResource != "checkstarted" && requestedResource != "")
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