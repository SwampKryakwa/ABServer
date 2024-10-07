using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;

namespace AB_Server.Gates
{
    internal class GateCard : IGateCard
    {
        static Func<int, Player, IGateCard>[] GateCtrs =
        [
            (x, y) => { throw new Exception("IncorrectGateCreation"); },
            (x, y) => new TripleBattle(x, y),
            (x, y) => new QuartetBattle(x, y),
            (x, y) => new MindGhost(x, y),
            (x, y) => { throw new Exception("IncorrectGateCreation"); },
        ];

        public static IGateCard CreateCard(Player owner, int cID, int type)
        {
            return GateCtrs[type].Invoke(cID, owner);
        }

        public List<Bakugan[]> EnterOrder = new();

        private protected Game game;

        public int CardId { get; set; }

        public List<Bakugan> Bakugans { get; set; } = new();
        public Player Owner { get; set; }
        public (int X, int Y) Position { get; set; }
        public bool[] DisallowedPlayers { get; set; }
        public bool ActiveBattle { get; set; } = false;
        public bool IsFrozen = false;
        public List<object> Freezing = new();
        public bool OnField { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool Negated = false;


        public void Freeze(object frozer)
        {
            IsFrozen = true;
            Freezing.Add(frozer);
            ActiveBattle = true;

            if (!game.Field.Cast<GateCard>().Any(x => x.ActiveBattle))
            {
                game.isBattleGoing = false;
                game.EndTurn();
            }
        }

        public void TryUnfreeze(object frozer)
        {
            Freezing.Remove(frozer);
            if (Freezing.Count == 0) IsFrozen = false;
            game.isBattleGoing |= CheckBattles();
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
                    b.ToHand(EnterOrder);
                }

                else
                {
                    b.Destroy(EnterOrder);
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

            game.Field[Position.X, Position.Y] = null;

            (this as IGateCard).Remove();
        }

        void Draw()
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.ToHand(EnterOrder);
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

        public void Set(int posX, int posY)
        {
            game.Field[posX, posY] = this;
            OnField = true;
            Owner.GateHand.Remove(this);
            Position = (posX, posY);
            for (int i = 0; i < game.PlayerCount; i++)
            {
                var e = game.NewEvents[i];
                if (game.Players[i] == Owner)
                {
                    JObject obj = new()
                    {
                        { "Type", "GateSetEvent" },
                        { "PosX", posX },
                        { "PosY", posY },
                        { "GateData", new JObject {
                            { "Type", (this as IGateCard).TypeId } }
                        },
                        { "Owner", Owner.ID },
                        { "CID", CardId }
                    };
                    if (this is NormalGate normalGate)
                    {
                        (obj["GateData"] as JObject).Add(new JProperty("Attribute", (int)normalGate.Attribute));
                        (obj["GateData"] as JObject).Add(new JProperty("Power", (int)normalGate.Power));
                    }
                    else if (this is NormalGate attributeHazard)
                    {
                        (obj["GateData"] as JObject).Add(new JProperty("Attribute", (int)attributeHazard.Attribute));
                    }
                    e.Add(obj);
                }
                else
                {
                    e.Add(new()
                    {
                        { "Type", "GateSetEvent" },
                        { "PosX", posX },
                        { "PosY", posY },
                        { "GateData", new JObject {
                            { "Type", -1 } }
                        },
                        { "Owner", Owner.ID },
                        { "CID", CardId }
                    });
                }
            }
            game.OnGateAdded(this, Owner.ID, posX, posY);
        }

        public void Open() { throw new NotImplementedException(); }

        public void Negate() { throw new NotImplementedException(); }

        public void Remove()
        {
            List<Bakugan> bakuganToSort;
            for (int i = 0; i < game.PlayerCount; i++)
            {
                Bakugans.FindAll(x => x.Owner.ID == i && !x.Defeated).ForEach(x => x.ToHand(EnterOrder));
                Bakugans.FindAll(x => x.Owner.ID == i && x.Defeated).ForEach(x => x.ToHand(EnterOrder));
            }

            foreach (List<JObject> e in game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "GateRemoved" },
                    { "PosX", Position.X },
                    { "PosY", Position.Y }
                });
            }
        }

        public void ToGrave()
        {
            Remove();
            Owner.GateGrave.Add(this);
            game.Field[Position.X, Position.Y] = null;
            IsOpen = false;
        }

        public bool IsOpenable()
        {
            return !Negated && OnField && Bakugans.Count >= 2 && !IsOpen;
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

        public int TypeId =>
            throw new NotImplementedException();

        public bool IsTouching(IGateCard card)
        {
            return AreTouching(this, card);
        }

        public bool IsTouching((int X, int Y) pos)
        {
            return AreTouching(Position, pos);
        }

        public static bool AreTouching((int X, int Y) pos1, (int X, int Y) pos2)
        {
            int DX = Math.Abs(pos1.X - pos2.X);
            int DY = Math.Abs(pos1.Y - pos2.Y);
            return DX + DY == 1;
        }

        public static bool AreTouching(IGateCard card1, IGateCard card2)
        {
            if (!card1.OnField | card2.OnField) return false;
            int X1 = card1.Position.X;
            int Y1 = card1.Position.Y;
            int X2 = card2.Position.X;
            int Y2 = card2.Position.Y;
            int DX = Math.Abs(card1.Position.X - card2.Position.X);
            int DY = Math.Abs(card1.Position.Y - card2.Position.Y);
            return DX + DY == 1;
        }
    }

    interface IGateCard : BakuganContainer
    {
        public int CardId { get; set; }
        public bool OnField { get; set; }
        public bool IsOpen { get; set; }
        public List<Bakugan> Bakugans { get; set; }
        public Player Owner { get; set; }
        public bool ActiveBattle { get; set; }
        public bool[] DisallowedPlayers { get; set; }
        public (int X, int Y) Position { get; set; }

        public int TypeId { get; }
        public void Set(int posX, int posY);
        public void Open();
        public void Negate();
        public void Remove();
        public void DetermineWinner();

        public bool IsOpenable() { return false; }
        public bool CheckBattles();
    }
}
