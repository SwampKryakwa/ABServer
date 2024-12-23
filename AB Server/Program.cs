﻿using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace AB_Server
{
    //class CommonCache
    //{
    //    public static CommonCache GetInstance()
    //    {
    //        if (_instance == null)
    //            _instance = new CommonCache();
    //        return _instance;
    //    }

    //    public string GetAllCache()
    //    {
    //        var result = new StringBuilder();
    //        result.Append("[\n");
    //        foreach (var item in _cache)
    //        {
    //            result.Append("  {\n");
    //            result.AppendFormat($"    \"key\": \"{item.Key}\",\n");
    //            result.AppendFormat($"    \"value\": \"{item.Value}\",\n");
    //            result.Append("  },\n");
    //        }
    //        result.Append("]\n");
    //        return result.ToString();
    //    }

    //    public bool GetCacheValue(string key, out string value)
    //    {
    //        return _cache.TryGetValue(key, out value);
    //    }

    //    public void PutCacheValue(string key, string value)
    //    {
    //        _cache[key] = value;
    //    }

    //    public bool DeleteCacheValue(string key, out string value)
    //    {
    //        return _cache.TryRemove(key, out value);
    //    }

    //    private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
    //    private static CommonCache _instance;
    //}

    //class HttpCacheSession : HttpSession
    //{
    //    public HttpCacheSession(NetCoreServer.HttpServer server) : base(server) { }

    //    public static string RandomString(int length)
    //    {
    //        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    //        return new string(Enumerable.Repeat(chars, length)
    //            .Select(s => s[random.Next(s.Length)]).ToArray());
    //    }

    //    static Dictionary<string, Room> Rooms = new();

    //    static Dictionary<string, Game> GIDToGame = new();
    //    private static Random random = new Random();

    //    object locker = new();

    //    protected override void OnReceivedRequest(HttpRequest request)
    //    {
    //        if (request.Method == "GET")
    //        {
    //            try
    //            {
    //                string key = request.Url;

    //                // Decode the key value
    //                key = Uri.UnescapeDataString(key);
    //                if (key != "/getupdates" && key != "/getplayerlist" && key != "/getallready" && key != "/checkstarted")
    //                {
    //                    Console.WriteLine(key);
    //                    Console.ForegroundColor = ConsoleColor.Yellow;
    //                    Console.WriteLine(request.Body);
    //                    Console.ForegroundColor = ConsoleColor.White;
    //                }

    //                JObject postedJson = null;
    //                try
    //                {
    //                    postedJson = JObject.Parse(request.Body);
    //                }
    //                catch (Exception e)
    //                {
    //                    Console.WriteLine(e);
    //                }

    //                string requestedResource = request.Url.ToString().Split('/')[^1];

    //                JObject answer = new();

    //                string GID;
    //                Game game;
    //                int player;
    //                switch (requestedResource)
    //                {
    //                    case "ping":
    //                        answer.Add("response", true);
    //                        break;

    //                    case "createroom":
    //                        string room = RandomString(8);
    //                        Rooms.Add(room, new Room((short)postedJson["playerCount"]));
    //                        answer.Add("room", room);
    //                        break;

    //                    case "joinroom":
    //                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
    //                        {
    //                            answer.Add("success", Rooms[(string)postedJson["roomName"]].AddPlayer((long)postedJson["UUID"], postedJson["userName"].ToString()));
    //                            break;
    //                        }
    //                        answer.Add("success", false);
    //                        break;

    //                    case "leaveroom":
    //                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
    //                        {
    //                            Rooms[(string)postedJson["roomName"]].RemovePlayer((long)postedJson["UUID"]);
    //                            if (!Rooms[(string)postedJson["roomName"]].Players.Any(x => x != null)) Rooms.Remove((string)postedJson["roomName"]);
    //                        }
    //                        break;

    //                    case "getmyposition":
    //                        answer.Add("position", Rooms[(string)postedJson["roomName"]].GetPosition((long)postedJson["UUID"]));
    //                        break;

    //                    case "updateready":
    //                        try { answer.Add("canStart", Rooms[(string)postedJson["roomName"]].UpdateReady((long)postedJson["UUID"], (bool)postedJson["isReady"])); }
    //                        catch
    //                        {
    //                            Console.WriteLine(postedJson["roomName"]);
    //                            Console.WriteLine(postedJson["UUID"]);
    //                            Console.WriteLine(postedJson["isReady"]);
    //                        }
    //                        break;

    //                    case "getplayerlist":
    //                        answer.Add("players", new JArray(Rooms[(string)postedJson["roomName"]].UserNames));
    //                        break;

    //                    case "getallready":
    //                        answer.Add("ready", new JArray(Rooms[(string)postedJson["roomName"]].IsReady));
    //                        break;

    //                    case "checkready":
    //                        answer.Add("canStart", Rooms[(string)postedJson["roomName"]].AreAllReady());
    //                        break;

    //                    case "checkstarted":
    //                        answer.Add("started", Rooms[(string)postedJson["roomName"]].Started);
    //                        break;

    //                    case "startroom":
    //                        Rooms[(string)postedJson["roomName"]].Started = true;
    //                        if (Rooms[(string)postedJson["roomName"]].Players.Contains((long)postedJson["UUID"]))
    //                        {
    //                            answer.Add("successful", true);
    //                        }
    //                        else
    //                            answer.Add("successful", false);
    //                        break;

    //                    case "newgame":
    //                        game = new((ushort)postedJson["playerCount"]);
    //                        room = (string)postedJson["roomName"];
    //                        GIDToGame.Add(room, game);

    //                        answer.Add("gid", room);
    //                        break;

    //                    case "getsession":
    //                        answer.Add("UUID", random.NextInt64());

    //                        break;

    //                    case "join":
    //                        lock (locker)
    //                        {
    //                            GID = (string)postedJson["gid"];
    //                            game = GIDToGame[GID];
    //                            answer.Add("pid", game.AddPlayer((JObject)postedJson["deck"], (long)postedJson["UUID"], (string)postedJson["name"]));
    //                            answer.Add("playerCount", game.Players.Where(x => x != null).Count());
    //                            if (game.PlayerCount == game.Players.Count)
    //                            {
    //                                Console.WriteLine("starting");
    //                                new Thread(game.Initiate).Start();
    //                            }
    //                        }
    //                        break;

    //                    case "getupdates":
    //                        answer.Add("updates", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].GetUpdates((int)postedJson["pid"])));
    //                        break;

    //                    case "checkturnstart":
    //                        answer.Add("turnplayer", new JObject { { "Type", "PlayerTurnStart" }, { "PID", GIDToGame[(string)postedJson["gid"]].activePlayer } });
    //                        break;

    //                    case "getmoves":
    //                        GID = (string)postedJson["gid"];
    //                        game = GIDToGame[GID];
    //                        player = (int)postedJson["playerID"];

    //                        answer.Add("moves", game.GetPossibleMoves(player));

    //                        break;

    //                    case "answer":
    //                        GID = (string)postedJson["gid"];
    //                        game = GIDToGame[GID];
    //                        player = (int)postedJson["playerID"];

    //                        bool hasStarted = game.Started;
    //                        game.IncomingSelection[player] = postedJson;
    //                        game.awaitingAnswers[player]?.Invoke();

    //                        break;

    //                    case "move":
    //                        GID = (string)postedJson["gid"];
    //                        game = GIDToGame[GID];
    //                        player = (int)postedJson["playerID"];

    //                        game.IncomingSelection[player] = postedJson;
    //                        if (!game.doNotMakeStep)
    //                            game.GameStep();
    //                        break;

    //                    case "leave":
    //                        GID = (string)postedJson["gid"];
    //                        game = GIDToGame[GID];

    //                        game.Left++;
    //                        if (game.Left == game.PlayerCount)
    //                        {
    //                            GIDToGame.Remove(GID);
    //                            game = null;
    //                        }
    //                        break;
    //                }
    //                if (key != "/getupdates" && key != "/getplayerlist" && key != "/getallready" && key != "/checkstarted")
    //                    Console.WriteLine();
    //                SendResponse(Response.MakeGetResponse(content: answer.ToString()));
    //            }
    //            catch (Exception e) { Console.WriteLine(e); }
    //        }
    //    }

    //    protected override void OnReceivedRequestError(HttpRequest request, string error)
    //    {
    //        Console.WriteLine($"Request error: {error}");
    //    }

    //    protected override void OnError(SocketError error)
    //    {
    //        Console.WriteLine($"HTTP session caught an error: {error}");
    //    }
    //}

    //class HttpCacheServer : HttpServer
    //{
    //    public HttpCacheServer(IPAddress address, int port) : base(address, port) { }

    //    protected override TcpSession CreateSession() { return new HttpCacheSession(this); }

    //    protected override void OnError(SocketError error)
    //    {
    //        Console.WriteLine($"HTTP session caught an error: {error}");
    //    }
    //}

    class Server
    {
        public static HttpListener listener;
        public static string url { get; set; } = "http://127.0.0.1:8080/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";

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
                            string key = request.Url.ToString();

                            JObject postedJson = null;
                            try
                            {
                                postedJson = JObject.Parse(body);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            string requestedResource = request.Url.ToString().Split('/')[^1];

                            // Decode the key value
                            if (requestedResource != "getupdates" && requestedResource != "getplayerlist" && requestedResource != "getallready" && requestedResource != "checkstarted" && requestedResource != "")
                            {
                                Console.WriteLine(key);
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine(body);
                                if (OperatingSystem.IsWindows())
                                    Console.ForegroundColor = ConsoleColor.White;
                                else
                                    Console.ForegroundColor = ConsoleColor.Black;
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
                                    Rooms.Add(room, new Room((short)postedJson["playerCount"]));
                                    answer.Add("room", room);
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
                                    game = new((ushort)postedJson["playerCount"]);
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

                                case "getupdates":
                                    answer.Add("updates", JArray.FromObject(GIDToGame[(string)postedJson["gid"]].GetUpdates((int)postedJson["pid"])));
                                    break;

                                case "checkturnstart":
                                    answer.Add("turnplayer", new JObject { { "Type", "PlayerTurnStart" }, { "PID", GIDToGame[(string)postedJson["gid"]].activePlayer } });
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

                                    game.IncomingSelection[player] = postedJson;
                                    if (!game.doNotMakeStep)
                                        game.GameStep();
                                    break;

                                case "leave":
                                    GID = (string)postedJson["gid"];
                                    game = GIDToGame[GID];

                                    game.Left++;
                                    if (game.Left == game.PlayerCount)
                                    {
                                        GIDToGame.Remove(GID);
                                        game = null;
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
                        catch (Exception e) { Console.WriteLine(e); }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        static async Task Main(string[] args)
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