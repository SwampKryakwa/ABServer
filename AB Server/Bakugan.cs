
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;

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
        public int Value { get; set; }
        public bool Active = true;
    }

    internal class Bakugan
    {
        Game game;

        public int BID;
        public BakuganType Type;

        public List<object> affectingEffects = new();

        public short DefaultPower { get; }
        public short BasePower;
        public List<Boost> Boosts = new();
        public int Power
        {
            get => BasePower + Boosts.Sum(b => b.Value);
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

        public Bakugan(BakuganType type, short power, Attribute attribute, Treatment treatment, Player owner, Game game, int BID)
        {
            Type = type;
            DefaultPower = power;
            BasePower = power;
            this.game = game;
            this.BID = BID;
            BaseAttribute = attribute;
            Attribute = attribute;
            Treatment = treatment;
            Owner = owner;
            Position = owner;
        }

        public void Boost(Boost boost, object source)
        {
            Boosts.Add(boost);
            foreach (var e in game.NewEvents)
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
            game.OnBakuganBoosted(this, boost, source);
        }

        //public void PermaBoost(short boost, object source)
        //{
        //    BasePower += boost;

        //    foreach (var e in game.NewEvents)
        //    {
        //        e.Add(new JObject {
        //            { "Type", "BakuganBoostedEvent" },
        //            { "Owner", Owner.Id },
        //            { "Boost", boost },
        //            { "Bakugan", new JObject {
        //                { "Type", (int)Type },
        //                { "Attribute", (int)Attribute },
        //                { "Treatment", (int)Treatment },
        //                { "Power", Power },
        //                { "BID", BID } }
        //            }
        //        });
        //    }
        //    game.OnBakuganBoosted(this, boost, source);
        //}

        public void AddFromHand(GateCard destination)
        {
            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.DisallowedPlayers[Owner.Id] = true;
            destination.EnterOrder.Add([this]);
            if (destination.ActiveBattle) InBattle = true;
            foreach (var e in game.NewEvents)
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
            game.OnBakuganAdded(this, Owner.Id, destination);
            game.isBattleGoing = destination.CheckBattles();
            InHands = false;
        }

        public void Throw(GateCard destination)
        {
            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.DisallowedPlayers[Owner.Id] = true;
            destination.EnterOrder.Add([this]);
            foreach (var e in game.NewEvents)
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
            game.OnBakuganThrown(this, Owner.Id, destination);
            game.isBattleGoing = destination.CheckBattles();
            InHands = false;
        }

        public void Move(GateCard destination)
        {
            Position.Remove(this);
            GateCard oldPosition = Position as GateCard;

            int f = oldPosition.EnterOrder.IndexOf(oldPosition.EnterOrder.First(x => x.Contains(this)));
            if (oldPosition.EnterOrder[f].Length == 1) oldPosition.EnterOrder.RemoveAt(f);
            else oldPosition.EnterOrder[f] = oldPosition.EnterOrder[f].Where(x => x != this).ToArray();

            if (!oldPosition.Bakugans.Any(x => x.Owner == Owner)) oldPosition.DisallowedPlayers[Owner.Id] = false;

            destination.DisallowedPlayers[Owner.Id] = true;
            if (destination.ActiveBattle) InBattle = true;
            destination.EnterOrder.Add([this]);

            foreach (var e in game.NewEvents)
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
            game.OnBakuganMoved(this, destination);

            game.isBattleGoing = false;
            foreach (var gate in game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
            {
                if (gate.CheckBattles())
                {
                    game.isBattleGoing = true;
                    break;
                }
            }
        }

        public void FromGrave(GateCard destination)
        {
            Defeated = false;
            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.DisallowedPlayers[Owner.Id] = true;
            destination.EnterOrder.Add([this]);
            game.OnBakuganPlacedFromGrave(this, Owner.Id, destination);
            game.isBattleGoing = destination.CheckBattles();
        }

        public void Revive()
        {
            Defeated = false;
            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);
            game.OnBakuganRevived(this, Owner.Id);
            InHands = true;
        }

        public void ToHand(List<Bakugan[]> entryOrder)
        {
            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);

            int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
            if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
            else entryOrder[f] = entryOrder[f].Where(x => x != this).ToArray();

            foreach (List<JObject> e in game.NewEvents)
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
            game.OnBakuganReturned(this, Owner.Id);
            InHands = true;

            game.isBattleGoing = false;
            foreach (var gate in game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
            {
                if (gate.CheckBattles())
                {
                    game.isBattleGoing = true;
                    break;
                }
            }
        }

        public void Destroy(List<Bakugan[]> entryOrder)
        {
            Defeated = true;
            Position.Remove(this);
            Position = Owner.BakuganGrave;
            Owner.BakuganGrave.Bakugans.Add(this);

            int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
            if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
            else entryOrder[f] = entryOrder[f].Where(x => x != this).ToArray();

            foreach (List<JObject> e in game.NewEvents)
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
            game.OnBakuganDestroyed(this, Owner.Id);
            InHands = false;
        }

        public bool OnField()
        {
            return typeof(IGateCard).IsAssignableFrom(Position.GetType());
        }

        public bool InHand()
        {
            return Position.GetType() == typeof(Player);
        }

        public bool InGrave()
        {
            return Position.GetType() == typeof(GraveBakugan);
        }

        public bool HasNeighbourEnemies()
        {
            if (!OnField() ||
                !game.BakuganIndex.Any(x => x.Owner.SideID != Owner.SideID && x.OnField())) return false;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            (int X, int Y) = (Position as GateCard).Position;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (game.GetGateByCoord(X - 1, Y) != null) return true;
            if (game.GetGateByCoord(X + 1, Y) != null) return true;
            if (game.GetGateByCoord(X, Y - 1) != null) return true;
            if (game.GetGateByCoord(X, Y + 1) != null) return true;
            return false;
        }
    }
}
