﻿using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace AB_Server.Gates
{
    internal class GateCard : IGateCard
    {
        static Func<int, Player, IGateCard>[] GateCtrs = new Func<int, Player, IGateCard>[]
        {
            (x, y) => { throw new Exception("IncorrectGateCreation"); },
            (x, y) => new TripleBattle(x, y),
            (x, y) => new QuartetBattle(x, y),
            (x, y) => new MindGhost(x, y),
            (x, y) => { throw new Exception("IncorrectGateCreation"); },
        };

        public static IGateCard CreateCard(Player owner, int cID, int type)
        {
            return GateCtrs[type].Invoke(cID, owner);
        }

        public List<Bakugan[]> EnterOrder = new();

        private protected Game game;

        public int CID { get; set; }

        public List<Bakugan> Bakugans { get; set; } = new();
        public Player Owner { get; set; }
        public (int X, int Y) Position { get; set; }
        public bool[] DisallowedPlayers { get; set; }
        public bool ActiveBattle { get; set; } = false;
        public bool IsFrozen = false;
        public List<object> Freezing;
        public bool IsOpen { get; set; } = false;
        public bool Negated = false;


        public void Freeze(object frozer)
        {
            IsFrozen = true;
            Freezing.Add(frozer);
            ActiveBattle = true;

            if (!game.Field.Cast<GateCard>().Any(x => x.ActiveBattle))
            {
                game.isFightGoing = false;
                game.EndTurn();
            }
        }

        public void TryUnfreeze(object frozer)
        {
            Freezing.Remove(frozer);
            if (Freezing.Count == 0) IsFrozen = false;
            game.isFightGoing |= CheckBattles();
        }

        public void DetermineWinner()
        {
            for (int i = 0; i < DisallowedPlayers.Length; i++)
            {
                DisallowedPlayers[i] = false;
            }
            foreach (Bakugan b in Bakugans)
            {
                b.InBattle = false;
            }
            int[] teamTotals = new int[game.Sides.Length];
            for (int i = 0; i < game.PlayerCount; i++) teamTotals[i] = 0;
            foreach (var b in Bakugans)
            {
                teamTotals[b.Owner.SideID] += b.Power;
            }

            int winnerPower = teamTotals.Max();

            if (teamTotals.Count(x => x == winnerPower) > 1)
            {
                Draw();
                return;
            }

            int winner = Array.IndexOf(teamTotals, teamTotals.Max());

            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                if (b.Owner.SideID == winner)
                {
                    b.ToHand(Bakugans, EnterOrder);
                }

                else
                {
                    b.Destroy(Bakugans, EnterOrder);
                }
            }

            foreach (List<JObject> e in game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "BattleOver" },
                    { "IsDraw", false },
                    { "Victor", winner }
                });
            }
            game.OnBattleOver(this, (ushort)winner);

            game.Field[Position / 10, Position % 10] = null;

            (this as IGateCard).Remove();
        }

        void Draw()
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.ToHand(Bakugans, EnterOrder);
            }
            foreach (List<JObject> e in game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "BattleOver" },
                    { "IsDraw", true }
                });
            }
            game.OnBattleOver(this, game.PlayerCount);
        }

        public void SetStart(int pos)
        {
            game.Field[pos / 10, pos % 10] = this;
            Owner.GateHand.Remove(this);
            Position = pos;
        }

        public void Set(int pos)
        {
            game.Field[pos / 10, pos % 10] = this;
            Owner.GateHand.Remove(this);
            Position = pos;
            foreach (var e in game.NewEvents)
            {
                JObject obj = new()
                {
                    { "Type", "GateSetEvent" },
                    { "Pos", pos },
                    { "GateData", new JObject {
                        { "Type", (this as IGateCard).GetTypeID() } }
                    },
                    { "Owner", Owner.ID },
                    { "CID", CID }
                };
                if ((this as IGateCard).GetTypeID() == 0)
                {
                    (obj["GateData"] as JObject).Add(new JProperty("Attribute", (int)(this as NormalGate).Attribute));
                    (obj["GateData"] as JObject).Add(new JProperty("Power", (int)(this as NormalGate).Power));
                }
                e.Add(obj);
            }
            game.OnGateAdded(this, Owner.ID, pos);
        }

        public void Open() { throw new NotImplementedException(); }

        public void Negate() { throw new NotImplementedException(); }

        public void Remove()
        {
            List<Bakugan> bakuganToSort;
            for (int i = 0; i < game.PlayerCount; i++)
            {
                Bakugans.FindAll(x => x.Owner.ID == i & !x.Defeated).ForEach(x => x.ToHand(Bakugans, EnterOrder));
                Bakugans.FindAll(x => x.Owner.ID == i & x.Defeated).ForEach(x => x.ToHand(Bakugans, EnterOrder));
            }

            foreach (List<JObject> e in game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "GateRemoved" },
                    { "Pos", Position }
                });
            }
        }

        public void ToGrave()
        {
            Remove();
            Position = -Owner.ID * 2 - 2;
            Owner.GateGrave.Add(this);
            game.Field[Position / 10, Position % 10] = null;
            IsOpen = false;
        }

        public bool IsOpenable()
        {
            return !Negated & Position >= 0 & Bakugans.Count >= 2 & !IsOpen;
        }

        public bool CheckBattles()
        {
            if (IsFrozen) return false;

            int[] numbSides = new int[game.PlayerCount];
            for (int i = 0; i < numbSides.Length; i++) numbSides[i] = 0;

            foreach (var b in Bakugans)
                numbSides[b.Owner.SideID]++;

            bool isBattle = numbSides.Where(x => x > 0).Count() >= 2;

            if (isBattle)
            {
                Bakugans.ForEach(x => x.InBattle = true);
                ActiveBattle = true;
            }

            return numbSides.Where(x => x > 0).Count() >= 2;
        }

        public int GetTypeID()
        {
            throw new NotImplementedException();
        }

        public bool IsTouching(IGateCard card)
        {
            return AreTouching(this, card);
        }

        public bool IsTouching(int pos)
        {
            return AreTouching(Position, pos);
        }

        public static bool AreTouching(int pos1, int pos2)
        {
            int X1 = pos1 / 10;
            int Y1 = pos1 % 10;
            int X2 = pos2 / 10;
            int Y2 = pos2 % 10;
            int DX = Math.Abs(X1 - X2);
            int DY = Math.Abs(Y1 - Y2);
            return (DX + DY == 1) & pos1 > 0 & pos2 > 0;
        }

        public static bool AreTouching(IGateCard card1, IGateCard card2)
        {
            int X1 = card1.Position / 10;
            int Y1 = card1.Position % 10;
            int X2 = card2.Position / 10;
            int Y2 = card2.Position % 10;
            int DX = Math.Abs(X1 - X2);
            int DY = Math.Abs(Y1 - Y2);
            return (DX + DY == 1) & card1.Position > 0 & card2.Position > 0;
        }
    }

    interface IGateCard : BakuganContainer
    {
        public int CID { get; set; }
        public bool IsOpen { get; set; }
        public List<Bakugan> Bakugans { get; set; }
        public Player Owner { get; set; }
        public bool ActiveBattle { get; set; }
        public bool[] DisallowedPlayers { get; set; }
        public (int X, int Y) Position { get; set; }

        public int GetTypeID();
        public void SetStart(int pos);
        public void Set(int pos);
        public void Open();
        public void Negate();
        public void Remove();
        public void DetermineWinner();

        public bool IsOpenable() { return false; }
        public bool CheckBattles();
    }
}
