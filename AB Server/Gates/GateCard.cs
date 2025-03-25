using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using System.ComponentModel.Design;
using System.Reflection.Metadata.Ecma335;

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
            (x, y) => new Intercept(x, y),
            (x, y) => new PositiveDelta(x, y),
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
        public bool IsFrozen = false;
        public List<object> Freezing = new();
        public List<object> OpenBlocking = new();
        public List<object> MovingInEffectBlocking = new();
        public List<object> MovingAwayEffectBlocking = new();
        public bool OnField { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool Negated = false;
        protected bool counterNegated = false;


        public void Freeze(object frozer)
        {
            IsFrozen = true;
            Freezing.Add(frozer);
            ActiveBattle = false;

            for (int i = 0; i < game.Field.GetLength(0); i++)
                for (int j = 0; j < game.Field.GetLength(1); j++)
                    if (game.Field[i, j] is GateCard battleGate && battleGate.ActiveBattle) return;

            game.isBattleGoing = false;
            game.EndTurn();
        }

        public void TryUnfreeze(object frozer)
        {
            Freezing.Remove(frozer);
            if (Freezing.Count == 0) IsFrozen = false;
            game.isBattleGoing |= CheckBattles();
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

            game.BattlesToEnd.Add(this);
        }

        public virtual void DetermineWinnerFakeBattle()
        {
            int winnerPower = Bakugans.MaxBy(x => x.Power).Power;

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
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    b.ToHand(EnterOrder);
                }

                IsOpen = false;
                OnField = false;

                game.Field[Position.X, Position.Y] = null;

                foreach (List<JObject> e in game.NewEvents)
                    e.Add(EventBuilder.RemoveGate(this));
            }
            else game.ContinueGame();
        }

        public virtual void Set(byte posX, byte posY)
        {
            game.Field[posX, posY] = this;
            OnField = true;
            Owner.GateHand.Remove(this);
            Position = (posX, posY);
            for (int i = 0; i < game.PlayerCount; i++)
            {
                var e = game.NewEvents[i];
                e.Add(EventBuilder.SetGate(this, game.Players[i] == Owner));
            }
            game.OnGateAdded(this, Owner.Id, posX, posY);
        }

        public virtual void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));
            game.CheckChain(Owner, this);
        }

        public virtual void Resolve()
        {
        }

        public virtual bool IsOpenable() =>
            OpenBlocking.Count == 0 && !Negated && OnField && ActiveBattle && !IsOpen;

        public virtual bool CheckBattles()
        {
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

        public void StartBattle()
        {
            ActiveBattle = true;
        }

        public virtual int TypeId =>
            throw new NotImplementedException();

        public int EffectId { get; set; }

        public AbilityKind Kind { get; } = AbilityKind.Gate;

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
            return (card.Position.Y - Position.Y) == 0 && Math.Abs(card.Position.X - Position.X) == 1;
        }

        public virtual void Negate(bool asCounter = false)
        {
            if (asCounter) counterNegated = true;
            Negated = true;
            IsOpen = false;

            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateNegated(this));
        }
    }
}
