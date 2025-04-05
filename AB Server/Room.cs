namespace AB_Server
{
    internal class Room
    {
        public string RoomName;
        public long?[] Players;
        public bool[] IsReady;
        public string?[] UserNames;
        public bool Started = false;
        public bool IsBotRoom;

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
            return true;
        }

        public void RemovePlayer(long uuid)
        {
            UserNames[Array.IndexOf(Players, uuid)] = null;
            IsReady[Array.IndexOf(Players, uuid)] = false;
            Players[Array.IndexOf(Players, uuid)] = null;
        }
    }
}
