using AB_Server.Abilities;
using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    abstract class GateCard : IBakuganContainer, IActive, IChainable
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
            (x, y) => new NegativeDelta(x, y),

            //Set 3 Gate Cards
            (x, y) => new ResonanceCircuit(x, y),
            (x, y) => new Shockwave(x, y),
            (x, y) => new DarkInvitation(x, y),
            (x, y) => new PowerSpike(x, y),
            (x, y) => new MindGhost(x, y),
            (x, y) => new Anastasis(x, y),
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
        public (byte X, byte Y) Position = (255, 255);
        public bool AllowAnyPlayers = false;

        public bool BattleStarted = false;
        public bool IsBattleGoing { get => Freezing.Count == 0 && (Bakugans.Select(x => x.Owner.TeamId).Distinct().Count() > 1 || (Bakugans.Count >= 2 && Bakugans.Any(x => x.Frenzied))); }
        public bool BattleStarting { get => !BattleStarted && IsBattleGoing; }
        public bool BattleDeclaredOver = false;
        public bool BattleOver = false;
        public bool BattleEnding { get => BattleDeclaredOver && !BattleOver; }
        public bool IsFrozen { get => Freezing.Count != 0; }
        public bool AllowsThrows { get => ThrowBlocking.Count != 0; }
        public List<object> Freezing = new();
        public List<object> OpenBlocking = new();
        public List<object> ThrowBlocking = new();
        public List<object> MovingInEffectBlocking = new();
        public List<object> MovingAwayEffectBlocking = new();
        public bool OnField { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool Negated = false;


        public void Freeze(object frozer)
        {
            Freezing.Add(frozer);

            Console.WriteLine(GetType().ToString() + " frozen");

            Console.WriteLine("Battles going: " + game.isBattleGoing.ToString());
            BattleStarted = false;
        }

        public void TryUnfreeze(object frozer)
        {
            Freezing.Remove(frozer);
            if (Freezing.Count == 0)
                Console.WriteLine(GetType().ToString() + " unfrozen");
            else
                Console.WriteLine(GetType().ToString() + " still frozen");
            BattleStarted = false;
        }

        protected readonly List<Bakugan> bakugansDefeatedThisBattle = [];
        public virtual void DetermineWinner()
        {
            bakugansDefeatedThisBattle.Clear();
            BattleOver = true;
            BattleDeclaredOver = false;
            BattleStarted = false;

            foreach (Bakugan b in Bakugans)
            {
                b.JustEndedBattle = true;
            }

            List<List<Bakugan>> sides = [];

            for (int i = 0; i < game.TeamCount; i++)
                sides.Add([.. Bakugans.Where(x => !x.Frenzied && x.Owner.TeamId == i)]);

            foreach (Bakugan bakugan in Bakugans.Where(x => x.Frenzied))
                sides.Add([bakugan]);

            BattleOver = true;

            if (sides.Count < 2) return;

            int[] teamTotals = new int[sides.Count];
            for (int i = 0; i < sides.Count; i++) teamTotals[i] = sides[i].Sum(x => x.Power);

            int winnerPower = teamTotals.Max();
            List<int> sidesToDefeat = [];
            for (int i = 0; i < sides.Count; i++)
                if (teamTotals[i] < winnerPower) sides[i].ForEach(x =>
                {
                    bakugansDefeatedThisBattle.Add(x);
                    x.MoveFromFieldToDrop(EnterOrder, MoveSource.Game);
                });

            List<List<Bakugan>> survivingSides = [.. sides.Where(x => x.Any(y => y.Position == this))];
            if (survivingSides.Count > 1)
            {
                Bakugan randomFirstBakugan = EnterOrder[0][new Random().Next(EnterOrder[0].Length)];
                for (int i = 0; i < survivingSides.Count; i++)
                    if (!survivingSides[i].Contains(randomFirstBakugan))
                        survivingSides[i].ForEach(x =>
                        {
                            bakugansDefeatedThisBattle.Add(x);
                            x.MoveFromFieldToDrop(EnterOrder, MoveSource.Game);
                        });
            }

            game.BattlesToEnd.Add(this);
        }

        public virtual void Dispose()
        {
            if (!IsBattleGoing)
            {
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    b.MoveFromFieldToHand(EnterOrder);
                }

                IsOpen = false;
                OnField = false;
                Owner.GateDrop.Add(this);

                game.Field[Position.X, Position.Y] = null;

                game.ThrowEvent(EventBuilder.RemoveGate(this));
                game.ThrowEvent(EventBuilder.SendGateToDrop(this));
            }
            else game.NextStep();
        }

        public virtual void ToDrop()
        {
            IsOpen = false;
            OnField = false;
            Owner.GateDrop.Add(this);

            game.Field[Position.X, Position.Y] = null;

            game.ThrowEvent(EventBuilder.RemoveGate(this));
            game.ThrowEvent(EventBuilder.SendGateToDrop(this));
        }

        public void Set(byte posX, byte posY)
        {
            IsOpen = false;
            game.Field[posX, posY] = this;
            OnField = true;
            Owner.GateHand.Remove(this);
            Position = (posX, posY);
            game.ThrowEvent(EventBuilder.GateSet(this, false), Owner.Id);
            game.ThrowEvent(Owner.Id, EventBuilder.GateSet(this, true));
            game.GateSetList.Add(this);
            game.OnGateAdded(this, Owner.Id, posX, posY);
        }

        public static void MultiSet(Game game, GateCard[] gateCards, (byte posX, byte posY)[] positions, byte[] setBy)
        {
            for (int i = 0; i < gateCards.Length; i++)
            {
                GateCard card = gateCards[i];
                card.IsOpen = false;
                card.OnField = true;
                card.Owner.GateHand.Remove(card);
                game.Field[positions[i].posX, positions[i].posY] = card;
                card.Position = positions[i];
                game.GateSetList.Add(card);
            }

            var settables = gateCards.Zip(setBy, (first, second) => (first, second)).ToArray();
            for (byte i = 0; i < game.PlayerCount; i++)
                game.ThrowEvent(i, EventBuilder.MultiGateSet(settables, i));

            for (int i = 0; i < gateCards.Length; i++)
                game.OnGateAdded(gateCards[i], setBy[i], positions[i].posX, positions[i].posY);
        }

        public virtual void Retract()
        {
            game.ThrowEvent(EventBuilder.GateRetracted(this, false), Owner.Id);
            game.ThrowEvent(Owner.Id, EventBuilder.GateRetracted(this, true));
            game.ThrowEvent(new JObject
            {
                ["Type"] = "GateAddedToHand",
                ["Owner"] = Owner.Id,
                ["Kind"] = (int)Kind,
                ["CardType"] = TypeId,
                ["CID"] = CardId
            });
            OnField = false;
            (byte posX, byte posY) = Position;
            game.Field[posX, posY] = null;
            Owner.GateHand.Add(this);
            Position = (255, 255);
            game.GateSetList.Remove(this);
            game.OnGateRemoved(this, Owner.Id, posX, posY);
        }

        public virtual void CheckAutoBattleStart() { }

        public virtual void CheckAutoBattleEnd() { }

        public virtual void Open()
        {
            game.OnGateOpen(this);
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Push(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));
            game.CheckChain(Owner, this);
        }

        public abstract void Resolve();

        public virtual bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !Negated && OnField && (IsBattleGoing || (game.Targets is not null && Bakugans.Any(x => game.Targets.Contains(x)))) && !IsOpen;

        public virtual int TypeId =>
            throw new NotImplementedException();

        public int EffectId { get; set; }

        public virtual CardKind Kind { get; } = CardKind.CommandGate;

        public bool IsAdjacent(GateCard card)
        {
            return AreAdjacent(this, card);
        }

        public static bool AreAdjacent(GateCard card1, GateCard card2)
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

        public bool IsDiagonal(GateCard otherCard)
        {
            return Math.Abs(otherCard.Position.X - Position.X) == 1 && Math.Abs(otherCard.Position.Y - Position.Y) == 1;
        }

        public virtual void Negate(bool asCounter = false)
        {
            Negated = true;
            IsOpen = false;

            game.ThrowEvent(EventBuilder.GateNegated(this));
        }

        public void TransformFrom(GateCard source)
        {
            Bakugans = source.Bakugans;
            EnterOrder = source.EnterOrder;
            Freezing = source.Freezing;
            OnField = source.OnField;
            Position = source.Position;
            BattleStarted = source.BattleStarted;
            BattleDeclaredOver = source.BattleDeclaredOver;
            BattleOver = source.BattleOver;
            ThrowBlocking = source.ThrowBlocking;
            MovingInEffectBlocking = source.MovingInEffectBlocking;
            MovingAwayEffectBlocking = source.MovingAwayEffectBlocking;

            game.ThrowEvent(new JObject
            {
                ["Type"] = "GateTransformed",
                ["FromId"] = source.CardId,
                ["FromPosX"] = source.Position.X,
                ["FromPosY"] = source.Position.Y,
                ["ToId"] = CardId,
                ["ToKind"] = (int)Kind,
                ["ToType"] = TypeId,
                ["ToOwner"] = Owner.Id
            });
        }
    }
}
