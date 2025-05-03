using Newtonsoft.Json.Linq;
using System.Timers;

namespace AB_Server
{
    internal class Room
    {
        public string RoomName;
        public string RoomKey;
        public byte TeamCount;
        public byte PlayersPerTeam;
        public long? RoomOwner;
        Dictionary<long, (int, int)> playerPositions = [];
        public long?[,] Players;
        public bool[,] IsReady;
        public string?[,] UserNames;
        public bool Started = false;
        public bool IsBotRoom;
        public Dictionary<long, List<JObject>> Updates = [];
        public System.Timers.Timer dieTimer;

        public Room(byte teamCount, byte playersPerTeam, string roomName, string roomKey, bool isBotRoom)
        {
            RoomKey = roomKey;
            TeamCount = teamCount;
            PlayersPerTeam = playersPerTeam;
            Players = new long?[teamCount, playersPerTeam];
            IsReady = new bool[teamCount, playersPerTeam];
            UserNames = new string?[teamCount, playersPerTeam];
            for (int i = 0; i < TeamCount; i++)
                for (int j = 0; j < PlayersPerTeam; j++)
                {
                    Players[i, j] = null;
                    IsReady[i, j] = false;
                    UserNames[i, j] = null;
                }

            if (roomName != "")
                RoomName = roomName;
            else
                RoomName = "Room";
            IsBotRoom = isBotRoom;
            dieTimer = new System.Timers.Timer()
            {
                AutoReset = false,
                Enabled = false,
                Interval = 2500
            };

            dieTimer.Elapsed += Die;
            dieTimer.Start();
        }

        private void Die(object? sender, ElapsedEventArgs e)
        {
            Server.Rooms.Remove(RoomKey);
            dieTimer.Stop();
            dieTimer.Dispose();
        }

        public int GetPosition(long uuid) => Array.IndexOf(Players, uuid);

        public void UpdateReady(long uuid, bool isReady)
        {
            (int team, int position) = playerPositions[uuid];
            IsReady[team, position] = isReady;
            foreach (var item in Updates.Keys)
                Updates[item].Add(new()
                {
                    ["Type"] = "PlayerReadyChanged",
                    ["Team"] = team,
                    ["Position"] = position,
                    ["State"] = isReady,
                    ["CanStart"] = RoomOwner == item ? !IsReady.Cast<bool>().Contains(false) : false
                });
        }

        public bool AddPlayer(long uuid, string userName)
        {
            int team = -1;
            for (int i = 0; i < TeamCount; i++)
                for (int j = 0; j < PlayersPerTeam; j++)
                    if (Players[i, j] == null)
                    {
                        Console.WriteLine($"[{i}, {j}]: null");
                        team = i;
                        playerPositions[uuid] = (i, j);
                        UserNames[i, j] = userName;
                        Players[i, j] = uuid;
                        IsReady[i, j] = false;
                        foreach (var item in Updates.Values)
                        {
                            item.Add(new()
                            {
                                ["Type"] = "PlayerJoined",
                                ["UserName"] = userName,
                                ["Position"] = Array.IndexOf(Players, uuid)
                            });
                        }
                        if (!Updates.ContainsKey(uuid)) Updates.Add(uuid, new List<JObject>());
                        else Updates[uuid].Clear();
                        string?[][] userNames = new string?[TeamCount][];
                        bool[][] readys = new bool[TeamCount][];
                        for (int i = 0; i < TeamCount; i++)
                        {
                            userNames[i] = new string?[PlayersPerTeam];
                            readys[i] = new bool[PlayersPerTeam];
                            for (int j = 0; j < PlayersPerTeam; j++)
                            {
                                userNames[i][j] = UserNames[i, j];
                                readys[i][j] = IsReady[i, j];
                                for (int j = 0; j < PlayersPerTeam; j++)
                                {
                                    userNames[i][j] = UserNames[i, j];
                                    readys[i][j] = IsReady[i, j];
                                }
                            }
                        }
                        Updates[uuid].Add(new()
                        {
                            ["Type"] = "RoomState",
                            ["UserNames"] = new JArray(userNames.Select(x => new JArray(x))),
                            ["ReadyStates"] = new JArray(readys.Select(x => new JArray(x))),
                            ["Team"] = i,
                            ["Position"] = j
                        });
                        if (RoomOwner == null)
                        {
                            RoomOwner = uuid;
                            Updates[uuid].Add(new()
                            {
                                ["Type"] = "BecameOwner"
                            });
                        }
                        return true;
                    }
                    else
                        Console.WriteLine($"[{i}, {j}]: {Players[i, j]}");
            return team != -1;
        }

        public void Spectate(long uuid)
        {
            if (!Updates.ContainsKey(uuid)) Updates.Add(uuid, new List<JObject>());
            else Updates[uuid].Clear();
            if (playerPositions.ContainsKey(uuid))
            {
                (int team, int position) = playerPositions[uuid];
                UserNames[team, position] = null;
                IsReady[team, position] = false;
                Players[team, position] = null;
                foreach (var item in Updates.Values)
                    item.Add(new()
                    {
                        ["Type"] = "PlayerLeft",
                        ["Team"] = team,
                        ["Position"] = position
                    });
                playerPositions.Remove(uuid);
            }

            string[][] userNames = new string[TeamCount][];
            bool[][] readys = new bool[TeamCount][];
            for (int x = 0; x < TeamCount; x++)
            {
                userNames[x] = new string[PlayersPerTeam];
                readys[x] = new bool[PlayersPerTeam];
            }

            Updates[uuid].Add(new()
            {
                ["Type"] = "RoomState",
                ["UserNames"] = new JArray(userNames.Select(x => new JArray(x))),
                ["ReadyStates"] = new JArray(readys.Select(x => new JArray(x)))
            });
        }

        public void RemovePlayer(long uuid)
        {
            if (playerPositions.ContainsKey(uuid))
            {
                (int team, int position) = playerPositions[uuid];
                UserNames[team, position] = null;
                IsReady[team, position] = false;
                Players[team, position] = null;
                foreach (var item in Updates.Values)
                {
                    item.Add(new()
                    {
                        ["Type"] = "PlayerLeft",
                        ["Position"] = Array.IndexOf(Players, uuid)
                    });
                }
            }
            if (Updates.ContainsKey(uuid)) Updates.Remove(uuid);
            if (RoomOwner == uuid && playerPositions.Count != 0)
            {
                RoomOwner = playerPositions.Keys.First();
                Updates[(long)RoomOwner].Add(new()
                {
                    ["Type"] = "BecameOwner"
                });
            }
        }

        public void SendMessage(long uuid, string nickname, string msg)
        {
            foreach (var item in Updates.Values)
                item.Add(new()
                {
                    ["Type"] = "NewMessage",
                    ["UUID"] = uuid,
                    ["Nickname"] = nickname,
                    ["Message"] = msg
                });
        }

        public JArray GetUpdates(long uuid)
        {
            dieTimer.Stop();
            dieTimer.Start();
            JArray updates = new(Updates[uuid]);
            Updates[uuid].Clear();
            return updates;
        }

        Game game;

        public Game Start()
        {
            Started = true;
            game = new((byte)(TeamCount * PlayersPerTeam), TeamCount);
            for (byte i = 0; i < TeamCount; i++)
                for (byte j = 0; j < PlayersPerTeam; j++)
                {
                    Updates[(long)Players[i, j]].Add(new()
                    {
                        ["Type"] = "DataRequest",
                        ["PlayerId"] = game.CreatePlayer(UserNames[i, j], i, (long)Players[i, j])
                    });
                }
            return game;
        }

        public void TellPlayersToJoin()
        {
            foreach (var item in Updates.Keys)
            {
                Updates[item].Add(new()
                {
                    ["Type"] = "JoinReady",
                    ["AsSpectator"] = !Players.Cast<long>().Contains(item),
                    ["TeamCount"] = game.TeamCount,
                    ["Players"] = new JArray(game.Players.Select(x => new JObject
                    {
                        ["Name"] = x.DisplayName,
                        ["PartnerType"] = (int)x.BakuganOwned[0].Type,
                        ["PartnerAttribute"] = (int)x.BakuganOwned[0].BaseAttribute,
                        ["PartnerTreatment"] = (int)x.BakuganOwned[0].Treatment,
                        ["Ava"] = x.Avatar,
                        ["Id"] = x.Id,
                        ["Team"] = x.TeamId
                    }))
                });
            }
        }
    }
}
