
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Reflection;

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
        Rattloid,
        Saurus,
        Scorpion,
        Serpent,
        Shredder,
        Sphinx,
        Worm
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

        public Attribute Attribute;
        public Treatment Treatment;

        public GateCard ParentGate;
        public int Position;
        public bool InBattle = false;
        public bool Defeated = false;
        public bool InHands = true;
        public bool usedAbilityThisTurn = false;

        public Bakugan(BakuganType type, short power, Attribute attribute, Treatment treatment, Player owner, Game game, int BID)
        {
            Type = type;
            DefaultPower = power;
            BasePower = power;
            Power = power;
            this.game = game;
            this.BID = BID;
            Attribute = attribute;
            Treatment = treatment;
            Owner = owner;
            Position = -owner.ID * 2 - 1;
        }

        public void Boost(short boost)
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
            game.OnBakuganBoosted(this, boost);
        }

        public void PermaBoost(short boost)
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
            game.OnBakuganBoosted(this, boost);
        }

        public void AddFromHand(int pos)
        {
            Position = pos;
            Owner.BakuganHand.Remove(this);
            game.Field[pos / 10, pos % 10].Bakugans.Add(this);
            game.Field[pos / 10, pos % 10].DisallowedPlayers[Owner.ID] = true;
            if (game.Field[pos / 10, pos % 10].ActiveBattle) InBattle = true;
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganAddedEvent" },
                    { "Pos", pos },
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
            game.OnBakuganAdded(this, Owner.ID, pos);
            game.isFightGoing = game.Field[pos / 10, pos % 10].CheckBattles();
            Power = BasePower;
            InHands = false;
        }

        public void Throw(int pos)
        {
            Position = pos;
            Owner.BakuganHand.Remove(this);
            game.Field[pos / 10, pos % 10].Bakugans.Add(this);
            game.Field[pos / 10, pos % 10].DisallowedPlayers[Owner.ID] = true;
            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganThrownEvent" },
                    { "Pos", pos },
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
            game.OnBakuganThrown(this, Owner.ID, pos);
            game.isFightGoing = game.Field[pos / 10, pos % 10].CheckBattles();
            Power = BasePower;
            InHands = false;
        }

        public void Move(int pos)
        {
            game.Field[Position / 10, Position % 10].Bakugans.Remove(this);
            if (!game.Field[Position / 10, Position % 10].Bakugans.Any(x=>x.Owner == Owner)) game.Field[Position / 10, Position % 10].DisallowedPlayers[Owner.ID] = false;
            game.Field[pos / 10, pos % 10].DisallowedPlayers[Owner.ID] = true;
            if (game.Field[pos / 10, pos % 10].ActiveBattle) InBattle = true;

            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "BakuganMovedEvent" },
                    { "Pos", pos },
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

            game.Field[pos / 10, pos % 10].Bakugans.Add(this);
            Position = pos;
            game.OnBakuganMoved(this, pos);
            game.isFightGoing = game.Field[pos / 10, pos % 10].CheckBattles();
        }

        public void FromGrave(int pos)
        {
            Defeated = false;
            Position = pos;
            Owner.BakuganGrave.Remove(this);
            game.Field[pos / 10, pos % 10].Bakugans.Add(this);
            game.OnBakuganPlacedFromGrave(this, Owner.ID, pos);
            game.isFightGoing = game.Field[pos / 10, pos % 10].CheckBattles();
            Power = BasePower;
        }

        public void Revive()
        {
            Defeated = false;
            Position = -Owner.ID * 2 - 2;
            Owner.BakuganGrave.Remove(this);
            Owner.BakuganHand.Add(this);
            game.OnBakuganRevived(this, Owner.ID);
            Power = BasePower;
            InHands = true;
        }

        public void ToHand(List<Bakugan> oldContainer)
        {
            Position = -Owner.ID * 2 - 1;
            Owner.BakuganHand.Add(this);
            oldContainer.Remove(this);

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

        public void Destroy(List<Bakugan> oldContainer)
        {
            Defeated = true;
            Position = -Owner.ID * 2 - 2;
            Owner.BakuganGrave.Add(this);
            oldContainer.Remove(this);

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

        /*public bool HasNeighborEnemies(Field field)
        {
            for (int j = 0; j < field.Gates.GetLength(1); j++)
                if (Math.Abs(j - int.Parse(Position[1].ToString())) <= 1)
                    for (int i = 0; i < field.Gates.GetLength(0); i++)
                        if (Math.Abs(i - int.Parse(Position[0].ToString())) <= 1)
                            if (field.Gates[i, j].BakuganList.Any(x => x.Owner != OwnerID) & $"{i}{j}" != Position) return true;
            return false;
        }*/

        /*public List<GateCard> GetListNeighborGatesWithEnemies(Field field)
        {
            List<GateCard> gates = new();
            for (int j = 0; j < field.Gates.GetLength(1); j++)
                if (Math.Abs(j - int.Parse(Position[1].ToString())) <= 1)
                    for (int i = 0; i < field.Gates.GetLength(0); i++)
                        if (Math.Abs(i - int.Parse(Position[0].ToString())) <= 1)
                            if (field.Gates[i, j].BakuganList.Any(x => x.Owner != OwnerID) & $"{i}{j}" != Position) gates.Add(field.Gates[i, j]);
            return gates;
        }*/
    }
}
