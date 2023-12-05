using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server
{
    internal class Room
    {
        public long?[] Players;
        public bool[] IsReady;
        public bool Started = false;

        public Room(short playerCount)
        {
            Players = new long?[playerCount];
            IsReady = new bool[playerCount];
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = null;
                IsReady[i] = false;
            }
        }

        public bool UpdateReady(long uuid, bool isReady)
        {
            IsReady[Array.IndexOf(Players, uuid)] = isReady;
            return IsReady.Contains(false);
        }

        public bool AreAllReady()
        {
            return IsReady.Contains(false);
        }

        public void AddPlayer(long uuid)
        {
            Players[Array.IndexOf(Players, null)] = uuid;
        }

        public void RemovePlayer(long uuid)
        {
            Players[Array.IndexOf(Players, uuid)] = null;
            IsReady[Array.IndexOf(Players, uuid)] = false;
        }
    }
}
