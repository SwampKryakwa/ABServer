﻿
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection.Metadata;

namespace AB_Server
{
    enum Attribute : byte
    {
        Nova,
        Aqua,
        Darkon,
        Zephyros,
        Lumina,
        Subterra,
        Clear
    }
    enum Treatment : byte
    {
        None,
        Flip,
        Pearl,
        Diamond
    }
    enum BakuganType : sbyte
    {
        None = -1,
        Glorius,
        Laserman,
        Mantis,
        Raptor,
        Lucifer,
        Saurus,
        Elephant,
        Tigress
    }

    enum MoveSource : byte
    {
        Game,
        Effect
    }

    class Boost(short value)
    {
        public short Value { get; set; } = value;
        public bool Active = true;
    }

    internal class Bakugan
    {
        public Game Game;

        public int BID;
        public BakuganType Type;
        public bool IsPartner = false;

        public List<object> affectingEffects = [];

        public short DefaultPower { get; }
        public short BasePower;
        public List<Boost> Boosts = [];
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

        public IBakuganContainer Position;
        public bool InBattle
        {
            get
            {
                if (Position is GateCard gatePosition)
                    return gatePosition.ActiveBattle;
                return false;
            }
        }
        public bool Defeated = false;
        public bool InHands = true;
        public bool IsDummy = false;
        public bool JustEndedBattle = false;

        public bool StickOnce = false;
        public bool BattleEndedInDraw = false; // New flag

        public Bakugan(BakuganType type, short power, Attribute attribute, Treatment treatment, Player owner, Game game, int BID)
        {
            Type = type;
            DefaultPower = power;
            BasePower = power;
            Game = game;
            this.BID = BID;
            BaseAttribute = attribute;
            Attribute = attribute;
            Treatment = treatment;
            Owner = owner;
            Position = owner;
        }

        public static Bakugan GetDummy()
        {
            var dummy = new Bakugan(BakuganType.None, 0, Attribute.Clear, Treatment.None, null, null, -1)
            {
                IsDummy = true
            };

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
                    { "IsPartner", IsPartner },
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
                    { "IsPartner", IsPartner },
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
            destination.BattleOver = false;
            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                { "Type", "BakuganRemovedFromHand" },
                { "Owner", Owner.Id },
                { "BakuganType", (int)Type },
                { "Attribute", (int)Attribute },
                { "Treatment", (int)Treatment },
                { "Power", Power },
                { "IsPartner", IsPartner },
                { "BID", BID }
            });
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
                    { "IsPartner", IsPartner },
                    { "BID", BID } }
                }
            });
            }
            Game.OnBakuganAdded(this, Owner.Id, destination);
            if (destination.CheckBattles())
                Game.isBattleGoing = true;
            InHands = false;
            BattleEndedInDraw = false; // Reset flag
        }

        public void Throw(GateCard destination)
        {
            if (IsDummy) return;

            Position.Remove(this);
            Position = destination;
            destination.BattleOver = false;
            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                { "Type", "BakuganRemovedFromHand" },
                { "Owner", Owner.Id },
                { "BakuganType", (int)Type },
                { "Attribute", (int)Attribute },
                { "Treatment", (int)Treatment },
                { "Power", Power },
                { "IsPartner", IsPartner },
                { "BID", BID }
            });
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
                    { "IsPartner", IsPartner },
                    { "BID", BID } }
                }
            });
            }
            Game.OnBakuganThrown(this, Owner.Id, destination);
            if (destination.CheckBattles())
                Game.isBattleGoing = true;
            InHands = false;
            BattleEndedInDraw = false; // Reset flag
        }

        public Attribute ChangeAttribute(Attribute newAttribute, object source)
        {
            if (IsDummy) return Attribute.Clear;

            var oldAttribute = Attribute;
            Attribute = newAttribute;
            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject {
                { "Type", "BakuganAttributeChangeEvent" },
                { "Owner", Owner.Id },
                { "OldAttribute", (int)oldAttribute },
                { "Attribute", (int)newAttribute },
                { "Bakugan", new JObject {
                    { "Type", (int)Type },
                    { "Attribute", (int)Attribute },
                    { "Treatment", (int)Treatment },
                    { "Power", Power },
                    { "IsPartner", IsPartner },
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

            if (Position is GateCard PositionGate)
            {
                if (PositionGate.MovingInEffectBlocking.Count != 0)
                    return;

                Position.Remove(this);
                GateCard oldPosition = PositionGate;

                int f = oldPosition.EnterOrder.IndexOf(oldPosition.EnterOrder.First(x => x.Contains(this)));
                if (oldPosition.EnterOrder[f].Length == 1) oldPosition.EnterOrder.RemoveAt(f);
                else oldPosition.EnterOrder[f] = oldPosition.EnterOrder[f].Where(x => x != this).ToArray();

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
                        { "IsPartner", IsPartner },
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
                BattleEndedInDraw = false; // Reset flag
            }
        }

        public static void MultiMove(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
        {
            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            List<Bakugan> bakuganToMove = bakugans.ToList();

            foreach (var bakugan in bakugans)
            {
                if (bakugan.Position is GateCard gatePosition)
                {
                    if (gatePosition.MovingInEffectBlocking.Count != 0)
                    {
                        bakuganToMove.Remove(bakugan);
                        continue;
                    }

                    bakugan.Position.Remove(bakugan);
                    GateCard oldPosition = gatePosition;

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
                        { "IsPartner", bakugan.IsPartner },
                        { "BID", bakugan.BID } }
                    }
                });
                    }
                }
                else
                {
                    bakuganToMove.Remove(bakugan);
                }
            }

            destination.EnterOrder.Add(bakuganToMove.ToArray());

            foreach (var bakugan in bakuganToMove)
            {
                destination.Bakugans.Add(bakugan);
                bakugan.Position = destination;
                game.OnBakuganMoved(bakugan, destination);
                bakugan.BattleEndedInDraw = false; // Reset flag
            }

            game.isBattleGoing = false;
            foreach (var gate in game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
                if (gate.CheckBattles())
                    game.isBattleGoing = true;
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
                    { "Type", "BakuganRemovedFromHand" },
                    { "Owner", bakugan.Owner.Id },
                    { "BakuganType", (int)bakugan.Type },
                    { "Attribute", (int)bakugan.Attribute },
                    { "Treatment", (int)bakugan.Treatment },
                    { "Power", bakugan.Power },
                    { "IsPartner", bakugan.IsPartner },
                    { "BID", bakugan.BID }
                });
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
                        { "IsPartner", bakugan.IsPartner },
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
                bakugan.BattleEndedInDraw = false; // Reset flag
            }

            game.isBattleGoing = false;
            foreach (var gate in game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
                if (gate.CheckBattles())
                    game.isBattleGoing = true;
        }

        public void FromGrave(GateCard destination, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            Defeated = false;
            Position.Remove(this);
            Position = destination;

            destination.BattleOver = false;
            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            Game.OnBakuganPlacedFromGrave(this, Owner.Id, destination);

            foreach (var e in Game.NewEvents)
            {
                e.Add(new JObject
            {
                { "Type", "HpRestored" },
                { "Owner", Owner.Id },
                { "HpLeft", Owner.BakuganOwned.Count(x=>!x.Defeated) }
            });
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
                    { "IsPartner", IsPartner },
                    { "BID", BID } }
                }
            });
                e.Add(new JObject {
                { "Type", "BakuganRemovedFromGrave" },
                { "Owner", Owner.Id },
                { "BakuganType", (int)Type },
                { "Attribute", (int)Attribute },
                { "Treatment", (int)Treatment },
                { "Power", Power },
                { "IsPartner", IsPartner },
                { "BID", BID }
            });
            }

            Boosts.ForEach(x => x.Active = false);
            Boosts.Clear();

            Game.isBattleGoing |= destination.CheckBattles();
            BattleEndedInDraw = false; // Reset flag
        }

        public void Revive()
        {
            if (IsDummy) return;

            Defeated = false;
            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);
            Game.OnBakuganRevived(this, Owner.Id);
            foreach (List<JObject> e in Game.NewEvents)
            {
                e.Add(new JObject
            {
                { "Type", "HpRestored" },
                { "Owner", Owner.Id },
                { "HpLeft", Owner.BakuganOwned.Count(x=>!x.Defeated) }
            });
                e.Add(new JObject {
                { "Type", "BakuganAddedToHand" },
                { "Owner", Owner.Id },
                { "BakuganType", (int)Type },
                { "Attribute", (int)Attribute },
                { "Treatment", (int)Treatment },
                { "Power", Power },
                { "IsPartner", IsPartner },
                { "BID", BID }
            });
                e.Add(new JObject {
                { "Type", "BakuganRemovedFromGrave" },
                { "Owner", Owner.Id },
                { "BakuganType", (int)Type },
                { "Attribute", (int)Attribute },
                { "Treatment", (int)Treatment },
                { "Power", Power },
                { "IsPartner", IsPartner },
                { "BID", BID }
            });
            }

            Boosts.ForEach(x => x.Active = false);
            Boosts.Clear();

            InHands = true;
            BattleEndedInDraw = false; // Reset flag
        }

        public void ToHand(List<Bakugan[]> entryOrder, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            if (Position is GateCard positionGate)
            {
                if (positionGate.MovingAwayEffectBlocking.Count != 0)
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
                    { "IsDestroy", false },
                    { "Bakugan", new JObject {
                        { "Type", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "IsPartner", IsPartner },
                        { "BID", BID } }
                    }
                });
                }

                Boosts.ForEach(x => x.Active = false);
                Boosts.Clear();
                Game.OnBakuganReturned(this, Owner.Id);
                InHands = true;

                foreach (List<JObject> e in Game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganAddedToHand" },
                        { "Owner", Owner.Id },
                        { "BakuganType", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Attribute },
                        { "Power", Power },
                        { "IsPartner", IsPartner },
                        { "BID", BID }
                    });
                }

                Game.isBattleGoing = false;
                foreach (var gate in Game.GateIndex.Where(x => x.OnField && x.Bakugans.Count >= 0))
                {
                    if (gate.CheckBattles())
                    {
                        Game.isBattleGoing = true;
                        break;
                    }
                }
                BattleEndedInDraw = false;
            }
        }

        public void DestroyOnField(List<Bakugan[]> entryOrder, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;
            if (Position is GateCard positionGate)
            {
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
                        { "IsDestroy", true },
                        { "Bakugan", new JObject {
                            { "Type", (int)Type },
                            { "Attribute", (int)Attribute },
                            { "Treatment", (int)Treatment },
                            { "Power", Power },
                            { "IsPartner", IsPartner },
                            { "BID", BID } }
                        }
                    });
                    e.Add(new JObject
                    {
                        { "Type", "HpLost" },
                        { "Owner", Owner.Id },
                        { "HpLeft", Owner.BakuganOwned.Count(x=>!x.Defeated) }
                    });
                }

                Boosts.ForEach(x => x.Active = false);
                Boosts.Clear();

                foreach (List<JObject> e in Game.NewEvents)
                    e.Add(new JObject {
                        { "Type", "BakuganAddedToGrave" },
                        { "Owner", Owner.Id },
                        { "BakuganType", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "IsPartner", IsPartner },
                        { "BID", BID }
                    });

                Game.OnBakuganDestroyed(this, Owner.Id);
                BattleEndedInDraw = false;
            }
        }

        public void DestroyInHand(MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;
            if (Position is Player positionPlayer)
            {
                Defeated = true;
                Position.Remove(this);
                Position = Owner.BakuganGrave;
                Owner.BakuganGrave.Bakugans.Add(this);

                foreach (List<JObject> e in Game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganRemovedFromHand" },
                        { "Owner", Owner.Id },
                        { "BakuganType", (int)Type },
                        { "Attribute", (int)Attribute },
                        { "Treatment", (int)Treatment },
                        { "Power", Power },
                        { "BID", BID }
                    });
                    e.Add(new JObject
                    {
                        { "Type", "HpLost" },
                        { "Owner", Owner.Id },
                        { "HpLeft", Owner.BakuganOwned.Count(x=>!x.Defeated) }
                    });
                }

                Boosts.ForEach(x => x.Active = false);
                Boosts.Clear();
                Game.OnBakuganDestroyed(this, Owner.Id);
                InHands = false;
            }
        }

        public bool OnField() =>
            typeof(GateCard).IsAssignableFrom(Position.GetType());

        public bool InHand() =>
            Position.GetType() == typeof(Player);

        public bool InGrave() =>
            Position.GetType() == typeof(GraveBakugan);

        public bool IsEnemyOf(Bakugan bakugan) =>
            Owner.SideID != bakugan.Owner.SideID;

        public bool HasNeighbourEnemies()
        {
            if (Position is GateCard positionGate)
            {
                if (!OnField() ||
                    !Game.BakuganIndex.Any(x => IsEnemyOf(x) && x.OnField())) return false;
                (int X, int Y) = positionGate.Position;

                if (Game.GetGateByCoord(X - 1, Y) is GateCard gate1 && gate1.Bakugans.Any(IsEnemyOf)) return true;
                if (Game.GetGateByCoord(X + 1, Y) is GateCard gate2 && gate2.Bakugans.Any(IsEnemyOf)) return true;
                if (Game.GetGateByCoord(X, Y - 1) is GateCard gate3 && gate3.Bakugans.Any(IsEnemyOf)) return true;
                if (Game.GetGateByCoord(X, Y + 1) is GateCard gate4 && gate4.Bakugans.Any(IsEnemyOf)) return true;
            }
            return false;
        }
    }
}
