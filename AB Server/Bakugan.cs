
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection.Metadata;

namespace AB_Server
{
    enum Attribute
    {
        Nova,
        Aqua,
        Darkon,
        Zephyros,
        Lumina,
        Subterra,
        Clear
    }
    enum Treatment
    {
        None,
        Flip,
        Pearl,
        Diamond
    }
    enum BakuganType
    {
        BeeStriker,
        Cancer,
        Centipede,
        Crow,
        ElCondor,
        Elephant,
        Fairy,
        Gargoyle,
        Garrison,
        Glorius,
        Griffon,
        Jackal,
        Juggernaut,
        Knight,
        Laserman,
        Limulus,
        Mantis,
        Raptor,
        Sidewinder,
        Saurus,
        Scorpion,
        Serpent,
        Shredder,
        Sphinx,
        Worm
    }

    enum MoveSource
    {
        Game,
        Effect
    }

    interface BakuganContainer
    {
        List<Bakugan> Bakugans { get; }

        public void Remove(Bakugan bakugan)
        {
            Bakugans.Remove(bakugan);
        }

        public void Add(Bakugan bakugan)
        {
            Bakugans.Add(bakugan);
        }
    }

    class Boost
    {
        public Boost(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
        public bool Active = true;
    }

    internal class Bakugan
    {
        public Game Game;

        public int BID;
        public BakuganType Type;

        public List<object> affectingEffects = new();

        public short DefaultPower { get; }
        public short BasePower;
        public List<Boost> Boosts = new();
        public int PowerModifier = 1;
        public int Power
        {
            get => (BasePower + Boosts.Sum(b => b.Value)) * PowerModifier;
        }
        public int AdditionalPower
        {
            get => Boosts.Sum(b => b.Value) * PowerModifier;
        }

        public Player Owner;

        public Attribute BaseAttribute;
        public Attribute Attribute;
        public Treatment Treatment;

        public BakuganContainer Position;
        public bool InBattle = false;
        public bool Defeated = false;
        public bool InHands = true;
        public bool UsedAbilityThisTurn = false;
        public bool IsDummy = false;

        public bool StickOnce = false;

        public Bakugan(BakuganType type, short power, Attribute attribute, Treatment treatment, Player owner, Game game, int BID)
        {
            Type = type;
            DefaultPower = power;
            BasePower = power;
            this.Game = game;
            this.BID = BID;
            BaseAttribute = attribute;
            Attribute = attribute;
            Treatment = treatment;
            Owner = owner;
            Position = owner;
        }

        public static Bakugan GetDummy()
        {
            var dummy = new Bakugan(BakuganType.Fairy, 0, Attribute.Clear, Treatment.None, null, null, -1);

            dummy.IsDummy = true;

            return dummy;
        }

        public void Boost(Boost boost, object source)
        {
            if (IsDummy) return;

            Boosts.Add(boost);

            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganBoostedEvent" },
                    { "Owner", Owner.Id },
                    { "Boost", boost.Value },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
            Game.OnBakuganBoosted(this, boost, source);

            if (PowerModifier < 0)
                boost.Value *= -1;
        }

        public void RemoveBoost(Boost boost, object source)
        {
            if (IsDummy) return;

            Boosts.Remove(boost);
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganBoostedEvent" },
                    { "Owner", Owner.Id },
                    { "Boost", -boost.Value },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
        }

        public void AddFromHand(GateCard destination, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            if (destination.ActiveBattle) InBattle = true;
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganAddedEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.Id },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
            Game.OnBakuganAdded(this, Owner.Id, destination);
            Game.isBattleGoing = destination.CheckBattles();
            InHands = false;
        }

        public void Throw(GateCard destination)
        {
            if (IsDummy) return;

            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganThrownEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.Id },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
            Game.OnBakuganThrown(this, Owner.Id, destination);
            Game.isBattleGoing = destination.CheckBattles();
            InHands = false;
        }

        public Attribute ChangeAttribute(Attribute newAttribute, object source)
        {
            if (IsDummy) return Attribute.Clear;

            Console.WriteLine(newAttribute);
            var oldAttribute = Attribute;
            Attribute = newAttribute;
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganAttributeChangeEvent" },
                    { "Owner", Owner.Id },
                    { "Attribute", (int)newAttribute },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
            return oldAttribute;
        }

        public void Move(GateCard destination, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            if (destination.MovingInEffectBlocking.Count != 0)
                return;


            if ((Position as GateCard).MovingInEffectBlocking.Count != 0)
                return;

            Position.Remove(this);
            GateCard oldPosition = Position as GateCard;

            int f = oldPosition.EnterOrder.IndexOf(oldPosition.EnterOrder.First(x => x.Contains(this)));
            if (oldPosition.EnterOrder[f].Length == 1) oldPosition.EnterOrder.RemoveAt(f);
            else oldPosition.EnterOrder[f] = oldPosition.EnterOrder[f].Where(x => x != this).ToArray();

            if (destination.ActiveBattle) InBattle = true;
            destination.EnterOrder.Add([this]);

            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganMovedEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.Id },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }

            destination.Bakugans.Add(this);
            Position = destination;
            Game.OnBakuganMoved(this, destination);

            Game.isBattleGoing = false;
            foreach (var gate in Game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
            {
                if (gate.CheckBattles())
                {
                    Game.isBattleGoing = true;
                    break;
                }
            }
        }

        public static void MultiMove(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
        {
            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            List<Bakugan> bakuganToMove = bakugans.ToList();

            foreach (var bakugan in bakugans)
            {
                if ((bakugan.Position as GateCard).MovingInEffectBlocking.Count != 0)
                {
                    bakuganToMove.Remove(bakugan);
                    continue;
                }

                bakugan.Position.Remove(bakugan);
                GateCard oldPosition = bakugan.Position as GateCard;

                int f = oldPosition.EnterOrder.IndexOf(oldPosition.EnterOrder.First(x => x.Contains(bakugan)));
                if (oldPosition.EnterOrder[f].Length == 1) oldPosition.EnterOrder.RemoveAt(f);
                else oldPosition.EnterOrder[f] = oldPosition.EnterOrder[f].Where(x => x != bakugan).ToArray();

                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganMovedEvent" },
                        { "PosX", destination.Position.X },
                        { "PosY", destination.Position.Y },
                        { "Owner", bakugan.Owner.Id },
                        { "Bakugan", new JObject {
                            { "Type", (int)bakugan.Type },
                            { "Attribute", (int)bakugan.Attribute },
                            { "Treatment", (int)bakugan.Treatment },
                            { "Power", bakugan.Power },
                            { "BID", bakugan.BID } }
                        }
                    });
                }
            }

            destination.EnterOrder.Add(bakuganToMove.ToArray());

            foreach (var bakugan in bakuganToMove)
            {
                destination.Bakugans.Add(bakugan);
                bakugan.Position = destination;
                game.OnBakuganMoved(bakugan, destination);
            }

            game.isBattleGoing = false;
            foreach (var gate in game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
                if (gate.CheckBattles())
                    game.isBattleGoing = true;

            foreach (var bakugan in bakugans)
                if (destination.ActiveBattle) bakugan.InBattle = true;
        }

        public static void MultiAdd(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
        {
            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            List<Bakugan> bakuganToAdd = bakugans.ToList();

            foreach (var bakugan in bakugans)
            {
                bakugan.Position.Remove(bakugan);

                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganAddedEvent" },
                        { "PosX", destination.Position.X },
                        { "PosY", destination.Position.Y },
                        { "Owner", bakugan.Owner.Id },
                        { "Bakugan", new JObject {
                            { "Type", (int)bakugan.Type },
                            { "Attribute", (int)bakugan.Attribute },
                            { "Treatment", (int)bakugan.Treatment },
                            { "Power", bakugan.Power },
                            { "BID", bakugan.BID } }
                        }
                    });
                }
            }

            destination.EnterOrder.Add(bakuganToAdd.ToArray());


            foreach (var bakugan in bakuganToAdd)
            {
                destination.Bakugans.Add(bakugan);
                bakugan.Position = destination;
                game.OnBakuganMoved(bakugan, destination);
            }

            game.isBattleGoing = false;
            foreach (var gate in game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
                if (gate.CheckBattles())
                    game.isBattleGoing = true;

            foreach (var bakugan in bakugans)
                if (destination.ActiveBattle) bakugan.InBattle = true;
        }

        public void FromGrave(GateCard destination, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            Defeated = false;
            Position.Remove(this);
            Position = destination;

            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganMovedEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.Id },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }

            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            Game.OnBakuganPlacedFromGrave(this, Owner.Id, destination);
            Game.isBattleGoing = destination.CheckBattles();
        }

        public void Revive()
        {
            if (IsDummy) return;

            Defeated = false;
            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);
            Game.OnBakuganRevived(this, Owner.Id);
            InHands = true;
        }

        public void ToHand(List<Bakugan[]> entryOrder, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            Console.WriteLine((Position as GateCard).MovingAwayEffectBlocking.Count);

            if ((Position as GateCard).MovingAwayEffectBlocking.Count != 0)
                return;

            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);

            int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
            if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
            else entryOrder[f] = entryOrder[f].Where(x => x != this).ToArray();

            foreach (List<JObject> e in Game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "BakuganRemoved" },
                    { "Owner", Owner.Id },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }

            Boosts.ForEach(x => x.Active = false);
            Boosts.Clear();
            Game.OnBakuganReturned(this, Owner.Id);
            InHands = true;

            Game.isBattleGoing = false;
            foreach (var gate in Game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
            {
                if (gate.CheckBattles())
                {
                    Game.isBattleGoing = true;
                    break;
                }
            }
        }

        public void Destroy(List<Bakugan[]> entryOrder, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            if ((Position as GateCard).MovingInEffectBlocking.Count != 0)
                return;

            Defeated = true;
            Position.Remove(this);
            Position = Owner.BakuganGrave;
            Owner.BakuganGrave.Bakugans.Add(this);

            int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
            if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
            else entryOrder[f] = entryOrder[f].Where(x => x != this).ToArray();

            foreach (List<JObject> e in Game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "BakuganRemoved" },
                    { "Owner", Owner.Id },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }

            Boosts.ForEach(x => x.Active = false);
            Boosts.Clear();
            Game.OnBakuganDestroyed(this, Owner.Id);
            InHands = false;
        }

        public bool OnField() =>
            typeof(IGateCard).IsAssignableFrom(Position.GetType());

        public bool InHand() =>
            Position.GetType() == typeof(Player);

        public bool InGrave() =>
            Position.GetType() == typeof(GraveBakugan);

        public bool HasNeighbourEnemies()
        {
            if (!OnField() ||
                !Game.BakuganIndex.Any(x => x.Owner.SideID != Owner.SideID && x.OnField())) return false;
            (int X, int Y) = (Position as GateCard).Position;

            if (Game.GetGateByCoord(X - 1, Y) != null) return true;
            if (Game.GetGateByCoord(X + 1, Y) != null) return true;
            if (Game.GetGateByCoord(X, Y - 1) != null) return true;
            if (Game.GetGateByCoord(X, Y + 1) != null) return true;
            return false;
        }
    }
}
