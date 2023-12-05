
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;

namespace AB_Server
{
    class Client
    {
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient Client)
        {
            // Код простой HTML-странички
            string Html = "<html><body><h1>It works!</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            // Закроем соединение
            Client.Close();
        }
    }

    class Server
    {

        static void ClientThread(Object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }

        // Запуск сервера
        /*public Server(int Port)
        {
            // Создаем "слушателя" для указанного порта
            Listener = new HttpListener();
            Listener.Start(); // Запускаем его

            // В бесконечном цикле
            while (true)
            {
                // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                // с использованием пула потоков.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
            }
        }*/

        static Dictionary<string, Room> Rooms = new();

        static Dictionary<string, long> RoomToGID = new();
        static Dictionary<long, Game> GIDToGame = new();
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static void Main(string[] args)
        {
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".

            // Create a listener.
            using HttpListener listener = new HttpListener();
            // Add the prefixes.
            listener.Prefixes.Add("http://localhost/");
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                JObject postedJson;
                try
                {
                    var toParse = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
                    postedJson = JObject.Parse(toParse);
                }
                catch
                {
                    postedJson = new();
                }

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                string requestedResource = request.Url.ToString().Split('/')[^1];

                JObject answer = new();

                long GID;
                Game game;
                int player;

                switch (requestedResource)
                {
                    case "createroom":
                        string room = RandomString(8);
                        Rooms.Add(room, new Room((short)postedJson["playerCount"]));
                        answer.Add("room", room);
                        break;

                    case "joinroom":
                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
                        {
                            Rooms[(string)postedJson["roomName"]].AddPlayer((long)postedJson["UUID"]);
                            answer.Add("success", true);
                            break;
                        }
                        answer.Add("success", false);
                        break;

                    case "leaveroom":
                        if (Rooms.ContainsKey((string)postedJson["roomName"]))
                            Rooms[(string)postedJson["roomName"]].RemovePlayer((long)postedJson["UUID"]);
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
                        answer.Add("players", new JArray(Rooms[(string)postedJson["roomName"]].Players));
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
                        GID = random.NextInt64();
                        RoomToGID.Add(room, GID);
                        GIDToGame.Add(GID, game);

                        answer.Add("gid", GID);
                        break;

                    case "getsession":
                        answer.Add("UUID", random.NextInt64());
                        break;

                    case "getgid":
                        if (RoomToGID.ContainsKey(postedJson["room"].ToString()))
                            answer.Add("gid", RoomToGID[postedJson["room"].ToString()]);
                        else
                            answer.Add("gid", 0);
                        break;

                    case "join":
                        GID = (long)postedJson["gid"];
                        game = GIDToGame[GID];
                        answer.Add("pid", game.AddPlayer((JObject)postedJson["deck"], (long)postedJson["UUID"]));
                        answer.Add("playerCount", game.Players.Where(x => x != null).Count());
                        if (game.PlayerCount == game.Players.Count)
                            new Thread(game.Initiate).Start();
                        break;

                    case "getupdates":
                        answer.Add("updates", JArray.FromObject(GIDToGame[(long)postedJson["gid"]].GetUpdates((int)postedJson["pid"])));
                        break;

                    case "getmoves":
                        GID = (long)postedJson["gid"];
                        game = GIDToGame[GID];
                        player = (int)postedJson["playerID"];

                        answer.Add("moves", game.GetPossibleMoves(player));

                        break;

                    case "answer":
                        GID = (long)postedJson["gid"];
                        game = GIDToGame[GID];
                        player = (int)postedJson["playerID"];

                        bool hasStarted = game.Started;
                        game.IncomingSelection[player] = postedJson;
                        game.awaitingAnswers[player]?.Invoke();
                        
                        if (hasStarted)
                            game.NewEvents[game.activePlayer].Add(new JObject { { "Type", "PlayerTurnStart" }, { "PID", game.activePlayer } });
                        break;

                    case "move":
                        GID = (long)postedJson["gid"];
                        game = GIDToGame[GID];
                        player = (int)postedJson["playerID"];

                        game.IncomingSelection[player] = postedJson;
                        game.GameStep();
                        break;

                    case "leave":
                        GID = (long)postedJson["gid"];
                        game = GIDToGame[GID];

                        game.Left++;
                        if (game.Left == game.PlayerCount)
                        {
                            GIDToGame.Remove(GID);
                            game = null;
                        }
                        break;
                }

                string responseString = answer.ToString();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
        }
    }
}