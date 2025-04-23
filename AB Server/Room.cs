using Newtonsoft.Json.Linq;

namespace AB_Server
{
    internal class Room
    {
        public string RoomName;
        public long?[] Players;
        public long? RoomOwner;
        public bool[] IsReady;
        public string?[] UserNames;
        public bool Started = false;
        public bool IsBotRoom;
        public Dictionary<long, List<JObject>> Updates = [];

        public Room(short playerCount, string roomName, bool isBotRoom)
        {
            Players = new long?[playerCount];
            IsReady = new bool[playerCount];
            UserNames = new string?[playerCount];
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = null;
                IsReady[i] = false;
                UserNames[i] = null;
            }

            if (roomName != null)
                RoomName = roomName;
            else
                RoomName = "Room";
            IsBotRoom = isBotRoom;
        }

        public int GetPosition(long uuid) => Array.IndexOf(Players, uuid);

        public bool UpdateReady(long uuid, bool isReady)
        {
            IsReady[Array.IndexOf(Players, uuid)] = isReady;
            foreach (var item in Updates.Values)
                item.Add(new()
                {
                    ["Type"] = "PlayerReadyChanged",
                    ["Position"] = Array.IndexOf(Players, uuid),
                    ["State"] = isReady
                });
            return IsReady.Contains(false);
        }

        public bool AreAllReady()
        {
            return IsReady.Contains(false);
        }

        public bool AddPlayer(long uuid, string userName)
        {
            if (!Players.Contains(null)) return false;
            UserNames[Array.IndexOf(Players, null)] = userName;
            Players[Array.IndexOf(Players, null)] = uuid;
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
            Updates[uuid].Add(new()
            {
                ["Type"] = "RoomState",
                ["UserNames"] = new JArray(UserNames),
                ["ReadyStates"] = new JArray(IsReady),
                ["Position"] = Array.IndexOf(Players, uuid)
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

        public void RemovePlayer(long uuid)
        {
            foreach (var item in Updates.Values)
                item.Add(new()
                {
                    ["Type"] = "PlayerLeft",
                    ["Position"] = Array.IndexOf(Players, uuid)
                });
            UserNames[Array.IndexOf(Players, uuid)] = null;
            IsReady[Array.IndexOf(Players, uuid)] = false;
            Players[Array.IndexOf(Players, uuid)] = null;
            if (Updates.ContainsKey(uuid)) Updates.Remove(uuid);
            if (RoomOwner == uuid && Players.First(x=>x is not null) is long newOwner)
            {
                RoomOwner = newOwner;
                Updates[newOwner].Add(new()
                {
                    ["Type"] = "BecameOwner"
                });
            }
        }

        public JArray GetUpdates(long uuid)
        {
            JArray updates = new(Updates[uuid]);
            Updates[uuid].Clear();
            return updates;
        }
    }
}
