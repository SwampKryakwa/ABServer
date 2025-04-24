using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AB_Server.Gates
{
    internal class GateCard : IBakuganContainer, IActive, IChainable
    {
        public Bakugan User { get; set; } = null;

        static Func<int, Player, GateCard>[] GateCtrs =
        [
            //Set 1 Gate Cards
            (x, y) => new LevelDown(x, y),
            (x, y) => new Peacemaker(x, y),
            (x, y) => new Warlock(x, y),
            (x, y) => new CheeringBattle(x, y),
            (x, y) => new Transform(x, y),

            //Set 2 Gate Cards
            (x, y) => new JokersWild(x, y),
            (x, y) => new PositiveDelta(x, y),
            (x, y) => new Aquamerge(x, y),
            (x, y) => new GrandSpirit(x, y),
            (x, y) => new Supernova(x, y),
            (x, y) => new Reloaded(x, y),
            (x, y) => new AdditionalTask(x, y),
            (x, y) => new QuicksandFreeze(x, y),
        ];

        public static GateCard CreateCard(Player owner, int cID, int type)
        {
            return GateCtrs[type].Invoke(cID, owner);
        }

        public List<Bakugan[]> EnterOrder = new();

        private protected Game game;

        public int CardId { get; set; }

        public List<Bakugan> Bakugans { get; set; } = new();
        public Player Owner { get; set; }
        public (byte X, byte Y) Position { get; set; } = (255, 255);
        public bool AllowAnyPlayers { get; set; } = false;
        public bool ActiveBattle { get; set; } = false;
        public bool IsFrozen { get => Freezing.Count != 0; }
        public List<object> Freezing = new();
        public List<object> OpenBlocking = new();
        public List<object> ThrowBlocking = new();
        public List<object> MovingInEffectBlocking = new();
        public List<object> MovingAwayEffectBlocking = new();
        public bool OnField { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool Negated = false;

        bool IsDraw = false;


        public void Freeze(object frozer)
        {
            Freezing.Add(frozer);

            ActiveBattle = false;

            Console.WriteLine(GetType().ToString() + " frozen");

            Console.WriteLine("Battles going: " + game.isBattleGoing.ToString());
        }

        public void TryUnfreeze(object frozer)
        {
            Freezing.Remove(frozer);
            if (Freezing.Count == 0)
                Console.WriteLine(GetType().ToString() + " unfrozen");
            else
                Console.WriteLine(GetType().ToString() + " still frozen");
        }

        public bool BattleOver = false;

        public virtual void DetermineWinner()
        {
            foreach (Bakugan b in Bakugans)
            {
                b.JustEndedBattle = true;
            }
            ActiveBattle = false;

            var numSides = Bakugans.Select(x => x.Owner.SideID).Distinct().Count();
            BattleOver = true;

            if (Bakugans.Count == 1) return;
            if (numSides > 1) DetermineWinnerNormalBattle();
            else if (numSides == 1) DetermineWinnerFakeBattle();
        }

        public virtual void DetermineWinnerNormalBattle()
        {
            int[] teamTotals = new int[game.SideCount];
            for (int i = 0; i < game.PlayerCount; i++) teamTotals[i] = 0;
            foreach (var b in Bakugans)
            {
                teamTotals[b.Owner.SideID] += b.Power;
            }

            int winnerPower = teamTotals.Max();

            if (teamTotals.Count(x => x == winnerPower) == 1)
            {
                int winner = Array.IndexOf(teamTotals, teamTotals.Max());

                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                    if (b.Owner.SideID != winner)
                    {
                        b.JustEndedBattle = true;
                        b.DestroyOnField(EnterOrder, MoveSource.Game);
                    }
            }
            else
            {
                foreach (Bakugan b in Bakugans)
                {
                    b.BattleEndedInDraw = true;
                    IsDraw = true;
                }
            }

            game.BattlesToEnd.Add(this);
        }

        public virtual void DetermineWinnerFakeBattle()
        {
            int winnerPower = Bakugans.MaxBy(x => x.Power).Power;
            IsDraw = true;

            if (Bakugans.Any(x => x.Power < winnerPower)) FakeBattleNormal(winnerPower);
            else FakeBattleDraw();
        }

        public virtual void FakeBattleNormal(int winnerPower)
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans.Where(x => x.Power < winnerPower)))
                b.ToHand(EnterOrder);

            game.BattlesToEnd.Add(this);
        }

        public virtual void FakeBattleDraw()
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.JustEndedBattle = true;
                b.ToHand(EnterOrder);
            }

            game.BattlesToEnd.Add(this);
        }

        public virtual void Dispose()
        {
            if (!CheckBattles())
            {
                ActiveBattle = false;
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    b.ToHand(EnterOrder);
                }

                if (!IsDraw)
                {
                    IsOpen = false;
                    OnField = false;
                    Owner.GateGrave.Add(this);

                    game.Field[Position.X, Position.Y] = null;

                    game.ThrowEvent(EventBuilder.RemoveGate(this));
                    game.ThrowEvent(EventBuilder.SendGateToGrave(this));
                }
            }
            else game.NextStep();
        }

        public virtual void Set(byte posX, byte posY)
        {
            IsOpen = false;
            IsDraw = false;
            game.Field[posX, posY] = this;
            OnField = true;
            Owner.GateHand.Remove(this);
            Position = (posX, posY);
            game.ThrowEvent(EventBuilder.GateSet(this, false), Owner.Id);
            game.NewEvents[Owner.Id].Add(EventBuilder.GateSet(this, true));
            game.GateSetList.Add(this);
            game.OnGateAdded(this, Owner.Id, posX, posY);
        }

        public virtual void Retract()
        {
            game.ThrowEvent(EventBuilder.GateRetracted(this, false), Owner.Id);
            game.NewEvents[Owner.Id].Add(EventBuilder.GateRetracted(this, true));
            game.ThrowEvent(new JObject {
                { "Type", "GateAddedToHand" },
                { "Owner", Owner.Id },
                { "GateType", TypeId },
                { "CID", CardId }
            });
            OnField = false;
            (byte posX, byte posY) = Position;
            game.Field[posX, posY] = null;
            Owner.GateHand.Add(this);
            Position = (255, 255);
            game.GateSetList.Remove(this);
            game.OnGateRemoved(this, Owner.Id, posX, posY);
        }

        public virtual void CheckAutoConditions(Bakugan target, byte owner, IBakuganContainer pos) { }

        public virtual void Open()
        {
            game.OnGateOpen(this);
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));
            game.CheckChain(Owner, this);
        }

        public virtual void Resolve()
        {
        }

        public virtual bool IsOpenable() =>
            OpenBlocking.Count == 0 && !Negated && OnField && ActiveBattle && !IsOpen;

        public virtual bool CheckBattles()
        {
            IsDraw = false;
            if (IsFrozen || BattleOver) return false;

            bool isBattle = Bakugans.Count > 1;

            if (isBattle)
            {
                if (!ActiveBattle)
                    game.BattlesToStart.Add(this);
            }
            else
            {
                ActiveBattle = false;
            }

            return isBattle;
        }

        public virtual void StartBattle()
        {
            ActiveBattle = true;
        }

        public virtual int TypeId =>
            throw new NotImplementedException();

        public int EffectId { get; set; }

        public CardKind Kind { get; } = CardKind.Gate;

        public bool IsTouching(GateCard card)
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
            return (DX + DY) == 1;
        }

        public static bool AreTouching(GateCard card1, GateCard card2)
        {
            if (!card1.OnField || !card2.OnField) return false;
            int DX = Math.Abs(card1.Position.X - card2.Position.X);
            int DY = Math.Abs(card1.Position.Y - card2.Position.Y);
            return (DX + DY) == 1;
        }

        public bool IsAdjacentVertically(GateCard card)
        {
            if (!OnField || !card.OnField) return false;
            return (card.Position.Y - Position.Y) == 0 && Math.Abs(card.Position.X - Position.X) == 1;
        }

        public bool IsAdjacentHorizontally(GateCard card)
        {
            if (!OnField || !card.OnField) return false;
            return (card.Position.X - Position.X) == 0 && Math.Abs(card.Position.Y - Position.Y) == 1;
        }

        public virtual void Negate(bool asCounter = false)
        {
            Negated = true;
            IsOpen = false;

            game.ThrowEvent(EventBuilder.GateNegated(this));
        }
    }
}
