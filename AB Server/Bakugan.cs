
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server
{
    enum Attribute
    {
        Pyrus,
        Aquos,
        Darkus,
        Ventus,
        Haos,
        Subterra,
        Clear
    }
    enum Treatment
    {
        None,
        Clear,
        Diamond,
        Camo,
        Lightup,
        Golden
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

    internal class Bakugan
    {
        Game game;

        public int BID;
        public BakuganType Type;

        public List<object> affectingEffects = new();

        public short DefaultPower { get; }
        public short BasePower;
        public short Power;

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
            Power = power;
            this.game = game;
            this.BID = BID;
            BaseAttribute = attribute;
            Attribute = attribute;
            Treatment = treatment;
            Owner = owner;
            Position = owner;
        }

        public int SwitchPowers(Bakugan otherBakugan)
        {
            short newThisPower = otherBakugan.Power;
            short newOtherPower = Power;
            otherBakugan.Power = newOtherPower;
            Power = newThisPower;
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganPowerSetEvent" },
                    { "Owner", Owner.ID },
                    { "Power", Power },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", otherBakugan.Power },
                        { "BID", BID } }
                    }
                });
                e.Add(new JObject {
                    { "Type", "BakuganPowerSetEvent" },
                    { "Owner", Owner.ID },
                    { "Power", otherBakugan.Power },
                    { "Bakugan", new JObject {
                        { "Type", (int)otherBakugan.Type },
                        { "Attribute", (int)otherBakugan.Attribute },
                        { "Treatment", (int)otherBakugan.Treatment },
                        { "Power", otherBakugan.Power },
                        { "BID", otherBakugan.BID } }
                    }
                });
            }
            return newThisPower - newOtherPower;
        }

        public void Boost(short boost, object source)
        {
            Power += boost;
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganBoostedEvent" },
                    { "Owner", Owner.ID },
                    { "Boost", boost },
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

        public void PermaBoost(short boost, object source)
        {
            BasePower += boost;
            Power += boost;
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganPermaBoostedEvent" },
                    { "Owner", Owner.ID },
                    { "Boost", boost },
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

        public void AddFromHand(GateCard destination)
        {
            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.DisallowedPlayers[Owner.ID] = true;
            destination.EnterOrder.Add(new Bakugan[] { this });
            if (destination.ActiveBattle) InBattle = true;
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganAddedEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.ID },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
            game.OnBakuganAdded(this, Owner.ID, destination);
            game.isBattleGoing = destination.CheckBattles();
            Power = BasePower;
            InHands = false;
        }

        public void Throw(GateCard destination)
        {
            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.DisallowedPlayers[Owner.ID] = true;
            destination.EnterOrder.Add([this]);
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganThrownEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.ID },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }
            game.OnBakuganThrown(this, Owner.ID, destination);
            game.isBattleGoing = destination.CheckBattles();
            Power = BasePower;
            InHands = false;
        }

        public void Move(GateCard destination)
        {
            Position.Remove(this);
            GateCard oldPosition = Position as GateCard;

            int f = oldPosition.EnterOrder.IndexOf(oldPosition.EnterOrder.First(x => x.Contains(this)));
            if (oldPosition.EnterOrder[f].Length == 1) oldPosition.EnterOrder.RemoveAt(f);
            else oldPosition.EnterOrder[f] = oldPosition.EnterOrder[f].Where(x => x != this).ToArray();

            if (!oldPosition.Bakugans.Any(x => x.Owner == Owner)) oldPosition.DisallowedPlayers[Owner.ID] = false;

            destination.DisallowedPlayers[Owner.ID] = true;
            if (destination.ActiveBattle) InBattle = true;
            destination.EnterOrder.Add(new Bakugan[] { this });

            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganMovedEvent" },
                    { "PosX", destination.Position.X },
                    { "PosY", destination.Position.Y },
                    { "Owner", Owner.ID },
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
            game.isBattleGoing = destination.CheckBattles();
        }

        public void FromGrave(GateCard destination)
        {
            Defeated = false;
            Position.Remove(this);
            Position = destination;
            destination.Bakugans.Add(this);
            destination.DisallowedPlayers[Owner.ID] = true;
            destination.EnterOrder.Add(new Bakugan[] { this });
            game.OnBakuganPlacedFromGrave(this, Owner.ID, destination);
            game.isBattleGoing = destination.CheckBattles();
            Power = BasePower;
        }

        public void Revive()
        {
            Defeated = false;
            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);
            game.OnBakuganRevived(this, Owner.ID);
            Power = BasePower;
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
                    { "Owner", Owner.ID },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }

            game.OnBakuganReturned(this, Owner.ID);
            Power = BasePower;
            InHands = true;
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
                    { "Owner", Owner.ID },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID } }
                    }
                });
            }

            game.OnBakuganDestroyed(this, Owner.ID);
            Power = BasePower;
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
