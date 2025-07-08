
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

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
        Diamond,
        Translucent
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
        Tigress,
        Garrison,
        Griffon,
        Knight,
        Worm,
        Shredder,
        Fairy
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

    class AttributeState(params Attribute[] attributes)
    {
        public Attribute[] Attributes = attributes;

        public bool IsAttribute(Attribute attribute) =>
            Attributes.Contains(attribute);

        public void AddAttribute(Attribute attribute)
        {
            Attributes = [.. Attributes, attribute];
        }
    }

    internal class Bakugan(BakuganType type, short power, Attribute attribute, Treatment treatment, Player owner, Game game, int BID)
    {
        public delegate void Destroyed();
        public delegate void RemovedFromField();
        public delegate void FromHandToDrop();
        public delegate void FromDropToHand();

        public Game Game = game;

        public int BID = BID;
        public BakuganType Type = type;
        public bool IsPartner = false;

        public List<object> AffectingEffects = [];
        public List<object> AbilityBlockers = [];

        public short DefaultPower { get; } = power;
        public short BasePower = power;
        public List<Boost> Boosts = [];
        public List<Boost> ContinuousBoosts = [];

        public event Destroyed OnDestroyed;
        public event RemovedFromField OnRemovedFromField;
        public event FromHandToDrop OnFromHandToDrop;
        public event FromDropToHand OnFromDropToHand;

        public int Power
        {
            get => BasePower + Boosts.Sum(b => b.Value) + ContinuousBoosts.Sum(b => b.Value);
        }
        public int AdditionalPower
        {
            get => Boosts.Sum(b => b.Value) + ContinuousBoosts.Sum(b => b.Value);
        }

        public Player Owner = owner;
        public bool Frenzied = false;

        public Attribute BaseAttribute = attribute;
        public List<AttributeState> attributeChanges = [];

        public Treatment Treatment = treatment;

        public bool IsAttribute(Attribute attr)
        {
            return attributeChanges.Count == 0 ? BaseAttribute == attr : attributeChanges[^0].IsAttribute(attr);
        }

        public IBakuganContainer Position = owner;
        public bool InBattle
        {
            get => Position is GateCard gatePosition && gatePosition.IsBattleGoing;
        }
        public bool Defeated = false;
        public bool IsDummy = false;
        public bool JustEndedBattle = false;

        public bool StickOnce = false;
        public bool BattleEndedInDraw = false; // New flag

        public static Bakugan GetDummy() => new Bakugan(BakuganType.None, 0, Attribute.Clear, Treatment.None, null, null, -1)
        {
            IsDummy = true
        };

        public void Boost(int boost, object source)
        {
            Boost(new Boost((short)boost), source);
        }

        public void Boost(Boost boost, object source)
        {
            if (IsDummy || InHand()) return;

            Boosts.Add(boost);

            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganBoostedEvent",
                ["Owner"] = Owner.Id,
                ["Boost"] = boost.Value,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["InHand"] = InHand(),
                    ["InGrave"] = InDrop(),
                    ["BID"] = BID
                }
            });

            Game.OnBakuganBoosted(this, boost, source);
        }

        public void ContinuousBoost(Boost boost, object source)
        {
            if (IsDummy) return;

            ContinuousBoosts.Add(boost);

            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganBoostedEvent",
                ["Owner"] = Owner.Id,
                ["Boost"] = boost.Value,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["InHand"] = InHand(),
                    ["InGrave"] = InDrop(),
                    ["BID"] = BID
                }
            });

            Game.OnBakuganBoosted(this, boost, source);
        }

        public void RemoveBoost(Boost boost, object source)
        {
            if (IsDummy) return;

            Boosts.Remove(boost);
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganBoostedEvent",
                ["Owner"] = Owner.Id,
                ["Boost"] = -boost.Value,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
        }

        public void RemoveContinuousBoost(Boost boost, object source)
        {
            if (IsDummy) return;

            ContinuousBoosts.Remove(boost);
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganBoostedEvent",
                ["Owner"] = Owner.Id,
                ["Boost"] = -boost.Value,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
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
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromHand",
                ["Owner"] = Owner.Id,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAddedEvent",
                ["PosX"] = destination.Position.X,
                ["PosY"] = destination.Position.Y,
                ["Owner"] = Owner.Id,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
            Game.OnBakuganAdded(this, Owner.Id, destination);
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
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromHand",
                ["Owner"] = Owner.Id,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganThrownEvent",
                ["PosX"] = destination.Position.X,
                ["PosY"] = destination.Position.Y,
                ["Owner"] = Owner.Id,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
            Game.OnBakuganThrown(this, Owner.Id, destination);
            BattleEndedInDraw = false; // Reset flag
        }

        public AttributeState ChangeAttribute(Attribute newAttribute, object source)
        {
            Attribute oldAttribute = attributeChanges.Count == 0 ? BaseAttribute : attributeChanges[^1].Attributes[0];
            AttributeState change = new(newAttribute);
            attributeChanges.Add(change);

            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAttributeChangeEvent",
                ["Owner"] = Owner.Id,
                ["OldAttribute"] = (int)oldAttribute,
                ["Attribute"] = (int)newAttribute,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });

            return change;
        }

        public void RevertAttributeChange(AttributeState change, object source)
        {
            attributeChanges.Remove(change);

            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAttributeChangeEvent",
                ["Owner"] = Owner.Id,
                ["OldAttribute"] = (int)change.Attributes[^1],
                ["Attribute"] = (int)(attributeChanges.Count == 0 ? BaseAttribute : attributeChanges[^1].Attributes[^1]),
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
        }

        public void TurnFrenzied()
        {
            if (!OnField()) return;
            Frenzied = true;
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganFrenzy",
                ["Owner"] = Owner.Id,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
        }

        public void StopFrenzy()
        {
            if (!OnField()) return;
            Frenzied = false;
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganUnfrenzied",
                ["Owner"] = Owner.Id,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
        }

        public void Move(GateCard destination, JObject MoveEffect, MoveSource mover = MoveSource.Effect)
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
                else oldPosition.EnterOrder[f] = [.. oldPosition.EnterOrder[f].Where(x => x != this)];

                destination.EnterOrder.Add([this]);

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganMovedEvent",
                    ["PosX"] = destination.Position.X,
                    ["PosY"] = destination.Position.Y,
                    ["Owner"] = Owner.Id,
                    ["Bakugan"] = new JObject
                    {
                        ["Type"] = (int)Type,
                        ["Attribute"] = (int)BaseAttribute,
                        ["Treatment"] = (int)Treatment,
                        ["BasePower"] = BasePower,
                        ["Power"] = Power,
                        ["IsPartner"] = IsPartner,
                        ["BID"] = BID
                    },
                    ["MoveEffect"] = MoveEffect
                });

                destination.Bakugans.Add(this);
                Position = destination;
                Game.OnBakuganMoved(this, destination);

                BattleEndedInDraw = false; // Reset flag
            }
        }

        public static void MultiMove(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
        {
            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            List<Bakugan> bakuganToMove = [.. bakugans];

            foreach (var bakugan in bakugans)
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
                    else oldPosition.EnterOrder[f] = [.. oldPosition.EnterOrder[f].Where(x => x != bakugan)];

                    game.ThrowEvent(new JObject
                    {
                        ["Type"] = "BakuganMovedEvent",
                        ["PosX"] = destination.Position.X,
                        ["PosY"] = destination.Position.Y,
                        ["Owner"] = bakugan.Owner.Id,
                        ["Bakugan"] = new JObject
                        {
                            ["Type"] = (int)bakugan.Type,
                            ["Attribute"] = (int)bakugan.BaseAttribute,
                            ["Treatment"] = (int)bakugan.Treatment,
                            ["Power"] = bakugan.Power,
                            ["IsPartner"] = bakugan.IsPartner,
                            ["BID"] = bakugan.BID
                        }
                    }
                    );
                }
                else
                    bakuganToMove.Remove(bakugan);


            destination.EnterOrder.Add([.. bakuganToMove]);

            foreach (var bakugan in bakuganToMove)
            {
                destination.Bakugans.Add(bakugan);
                bakugan.Position = destination;
                game.OnBakuganMoved(bakugan, destination);
                bakugan.BattleEndedInDraw = false; // Reset flag
            }

        }

        public static void MultiAdd(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
        {
            if (destination.MovingInEffectBlocking.Count != 0)
                return;

            List<Bakugan> bakuganToAdd = [.. bakugans];

            foreach (var bakugan in bakugans)
            {
                bakugan.Position.Remove(bakugan);

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganRemovedFromHand",
                    ["Owner"] = bakugan.Owner.Id,
                    ["BakuganType"] = (int)bakugan.Type,
                    ["Attribute"] = (int)bakugan.BaseAttribute,
                    ["Treatment"] = (int)bakugan.Treatment,
                    ["BasePower"] = bakugan.BasePower,
                    ["Power"] = bakugan.Power,
                    ["IsPartner"] = bakugan.IsPartner,
                    ["BID"] = bakugan.BID
                });
                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganAddedEvent",
                    ["PosX"] = destination.Position.X,
                    ["PosY"] = destination.Position.Y,
                    ["Owner"] = bakugan.Owner.Id,
                    ["Bakugan"] = new JObject
                    {
                        ["Type"] = (int)bakugan.Type,
                        ["Attribute"] = (int)bakugan.BaseAttribute,
                        ["Treatment"] = (int)bakugan.Treatment,
                        ["BasePower"] = bakugan.BasePower,
                        ["Power"] = bakugan.Power,
                        ["IsPartner"] = bakugan.IsPartner,
                        ["BID"] = bakugan.BID
                    }
                });
            }

            destination.EnterOrder.Add([.. bakuganToAdd]);

            foreach (var bakugan in bakuganToAdd)
            {
                destination.Bakugans.Add(bakugan);
                bakugan.Position = destination;
                game.OnBakuganMoved(bakugan, destination);
                bakugan.BattleEndedInDraw = false; // Reset flag
            }
        }

        public void MoveFromDropToField(GateCard destination, MoveSource mover = MoveSource.Effect)
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
            Game.OnBakuganPlacedFromDrop(this, Owner.Id, destination);

            game.ThrowEvent(new JObject
            {
                ["Type"] = "HpRestored",
                ["Owner"] = Owner.Id,
                ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
            });
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAddedEvent",
                ["PosX"] = destination.Position.X,
                ["PosY"] = destination.Position.Y,
                ["Owner"] = Owner.Id,
                ["Bakugan"] = new JObject
                {
                    ["Type"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                }
            });
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromGrave",
                ["Owner"] = Owner.Id,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });
            BattleEndedInDraw = false; // Reset flag
        }

        public void MoveFromDropToHand()
        {
            if (IsDummy) return;

            attributeChanges.Clear();

            Defeated = false;
            Position.Remove(this);
            Position = Owner;
            Owner.Bakugans.Add(this);
            OnFromDropToHand?.Invoke();
            Game.OnBakuganRevived(this, Owner.Id);
            game.ThrowEvent(new JObject
            {
                ["Type"] = "HpRestored",
                ["Owner"] = Owner.Id,
                ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
            });
            game.ThrowEvent(new JObject {
                    { "Type", "BakuganAddedToHand" },
                    { "Owner", Owner.Id },
                    { "BakuganType", (int)Type },
                    { "Attribute", (int)BaseAttribute },
                    { "Treatment", (int)Treatment },
                    { "Power", Power },
                    { "IsPartner", IsPartner },
                    { "BID", BID }
                });
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromGrave",
                ["Owner"] = Owner.Id,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });

            BattleEndedInDraw = false; // Reset flag
        }

        public void MoveFromFieldToHand(List<Bakugan[]> entryOrder, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;

            attributeChanges.Clear();

            if (Position is GateCard positionGate)
            {
                Frenzied = false;
                if (positionGate.MovingAwayEffectBlocking.Count != 0)
                    return;

                Position.Remove(this);
                Position = Owner;
                Owner.Bakugans.Add(this);

                int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
                if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
                else entryOrder[f] = [.. entryOrder[f].Where(x => x != this)];

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganRemoved",
                    ["Owner"] = Owner.Id,
                    ["IsDestroy"] = false,
                    ["Bakugan"] = new JObject
                    {
                        ["Type"] = (int)Type,
                        ["Attribute"] = (int)BaseAttribute,
                        ["Treatment"] = (int)Treatment,
                        ["BasePower"] = BasePower,
                        ["Power"] = Power,
                        ["IsPartner"] = IsPartner,
                        ["BID"] = BID
                    }
                });

                Boosts.ForEach(x => x.Active = false);
                Boosts.Clear();
                OnRemovedFromField?.Invoke();
                Game.OnBakuganReturned(this, Owner.Id);

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganAddedToHand",
                    ["Owner"] = Owner.Id,
                    ["BakuganType"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                });

                BattleEndedInDraw = false;
            }
        }

        public static void MultiToHand(Game game, IEnumerable<Bakugan> providedBakugans, MoveSource mover = MoveSource.Effect)
        {
            Console.WriteLine($"Provided Bakugan: " + providedBakugans.Count());
            var removableBakugans = providedBakugans.Where(x => !x.IsDummy && x.OnField()).ToArray();

            if (mover == MoveSource.Effect)
                removableBakugans = [.. providedBakugans.Where(x => (x.Position as GateCard).MovingAwayEffectBlocking.Count == 0)];
            Console.WriteLine($"Removable Bakugan: " + removableBakugans.Count());

            foreach (var bakugan in removableBakugans)
            {
                bakugan.Frenzied = false;
                bakugan.OnRemovedFromField?.Invoke();
                game.OnBakuganReturned(bakugan, bakugan.Owner.Id);

                var entryOrder = (bakugan.Position as GateCard).EnterOrder;
                int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(bakugan)));
                if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
                else entryOrder[f] = [.. entryOrder[f].Where(x => x != bakugan)];

                bakugan.Position.Remove(bakugan);
                bakugan.Owner.Bakugans.Add(bakugan);
                bakugan.Position = bakugan.Owner;
            }
            Console.WriteLine($"Removable Bakugan: " + removableBakugans.Count());

            game.ThrowEvent(new JObject
            {
                ["Type"] = "MultiBakuganRemoved",
                ["IsDestroy"] = false,
                ["Bakugan"] = new JArray(removableBakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID,
                    ["Owner"] = x.Owner.Id
                }))
            });

            foreach (var bakugan in removableBakugans)
                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganAddedToHand",
                    ["Owner"] = bakugan.Owner.Id,
                    ["BakuganType"] = (int)bakugan.Type,
                    ["Attribute"] = (int)bakugan.BaseAttribute,
                    ["Treatment"] = (int)bakugan.Treatment,
                    ["Power"] = bakugan.Power,
                    ["IsPartner"] = bakugan.IsPartner,
                    ["BID"] = bakugan.BID
                });
        }

        public void MoveFromFieldToDrop(List<Bakugan[]> entryOrder, MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;
            if (Position is GateCard positionGate)
            {
                attributeChanges.Clear();
                Frenzied = false;
                Defeated = true;
                Position.Remove(this);
                Position = Owner.BakuganDrop;
                Owner.BakuganDrop.Bakugans.Add(this);

                int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
                if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
                else entryOrder[f] = [.. entryOrder[f].Where(x => x != this)];

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganRemoved",
                    ["Owner"] = Owner.Id,
                    ["IsDestroy"] = false,
                    ["Bakugan"] = new JObject
                    {
                        ["Type"] = (int)Type,
                        ["Attribute"] = (int)BaseAttribute,
                        ["Treatment"] = (int)Treatment,
                        ["BasePower"] = BasePower,
                        ["Power"] = Power,
                        ["IsPartner"] = IsPartner,
                        ["BID"] = BID
                    }
                });
                game.ThrowEvent(new JObject
                {
                    ["Type"] = "HpLost",
                    ["Owner"] = Owner.Id,
                    ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
                });

                Boosts.ForEach(x => x.Active = false);
                Boosts.Clear();

                OnRemovedFromField?.Invoke();
                OnDestroyed?.Invoke();
                Game.OnBakuganDestroyed(this, Owner.Id);

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganAddedToGrave",
                    ["Owner"] = Owner.Id,
                    ["BakuganType"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                });

                BattleEndedInDraw = false;
            }
        }

        public void MoveFromHandToDrop(MoveSource mover = MoveSource.Effect)
        {
            if (IsDummy) return;
            if (Position is Player positionPlayer)
            {
                attributeChanges.Clear();
                Defeated = true;
                Position.Remove(this);
                Position = Owner.BakuganDrop;
                Owner.BakuganDrop.Bakugans.Add(this);

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganRemovedFromHand",
                    ["Owner"] = Owner.Id,
                    ["BakuganType"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["BasePower"] = BasePower,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                });
                game.ThrowEvent(new JObject
                {
                    ["Type"] = "HpLost",
                    ["Owner"] = Owner.Id,
                    ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
                });

                Boosts.ForEach(x => x.Active = false);
                Boosts.Clear();

                OnFromHandToDrop?.Invoke();
                OnDestroyed?.Invoke();
                Game.OnBakuganDestroyed(this, Owner.Id);

                game.ThrowEvent(new JObject
                {
                    ["Type"] = "BakuganAddedToGrave",
                    ["Owner"] = Owner.Id,
                    ["BakuganType"] = (int)Type,
                    ["Attribute"] = (int)BaseAttribute,
                    ["Treatment"] = (int)Treatment,
                    ["Power"] = Power,
                    ["IsPartner"] = IsPartner,
                    ["BID"] = BID
                });
            }
        }

        /// <summary>
        /// Shows if the Bakugan is on the field, or, in other words, is standing
        /// </summary>
        public bool OnField() =>
            Position is GateCard;

        public bool InHand() =>
            Position is Player;

        public bool InDrop() =>
            Position is BakuganDrop;

        public bool IsOpponentOf(Bakugan bakugan) =>
            Owner.TeamId != bakugan.Owner.TeamId;

        public bool HasNeighbourEnemies()
        {
            if (Position is GateCard positionGate)
            {
                if (!OnField() ||
                    !Game.BakuganIndex.Any(x => IsOpponentOf(x) && x.OnField())) return false;
                (int X, int Y) = positionGate.Position;

                if (Game.GetGateByCoord(X - 1, Y) is GateCard gate1 && gate1.Bakugans.Any(IsOpponentOf)) return true;
                if (Game.GetGateByCoord(X + 1, Y) is GateCard gate2 && gate2.Bakugans.Any(IsOpponentOf)) return true;
                if (Game.GetGateByCoord(X, Y - 1) is GateCard gate3 && gate3.Bakugans.Any(IsOpponentOf)) return true;
                if (Game.GetGateByCoord(X, Y + 1) is GateCard gate4 && gate4.Bakugans.Any(IsOpponentOf)) return true;
            }
            return false;
        }

        public static bool IsAdjacent(Bakugan bakugan1, Bakugan bakugan2)
        {

            List<Attribute> attrs1 = [.. bakugan1.attributeChanges[^1].Attributes];
            List<Attribute> attrs2 = [.. bakugan1.attributeChanges[^1].Attributes];

            return attrs1.Contains(Attribute.Nova) && attrs2.Contains(Attribute.Subterra) ||
                attrs1.Contains(Attribute.Subterra) && attrs2.Contains(Attribute.Lumina) ||
                attrs1.Contains(Attribute.Lumina) && attrs2.Contains(Attribute.Darkon) ||
                attrs1.Contains(Attribute.Darkon) && attrs2.Contains(Attribute.Aqua) ||
                attrs1.Contains(Attribute.Aqua) && attrs2.Contains(Attribute.Zephyros) ||
                attrs1.Contains(Attribute.Zephyros) && attrs2.Contains(Attribute.Nova);
        }

        public static bool IsDiagonal(Bakugan bakugan1, Bakugan bakugan2)
        {

            List<Attribute> attrs1 = bakugan1.attributeChanges.Count == 0 ? [bakugan1.BaseAttribute] : [.. bakugan1.attributeChanges[^1].Attributes];
            List<Attribute> attrs2 = bakugan1.attributeChanges.Count == 0 ? [bakugan1.BaseAttribute] : [.. bakugan1.attributeChanges[^1].Attributes];

            return attrs1.Contains(Attribute.Nova) && attrs2.Contains(Attribute.Darkon) ||
                attrs1.Contains(Attribute.Darkon) && attrs2.Contains(Attribute.Nova) ||
                attrs1.Contains(Attribute.Subterra) && attrs2.Contains(Attribute.Aqua) ||
                attrs1.Contains(Attribute.Aqua) && attrs2.Contains(Attribute.Subterra) ||
                attrs1.Contains(Attribute.Lumina) && attrs2.Contains(Attribute.Zephyros) ||
                attrs1.Contains(Attribute.Zephyros) && attrs2.Contains(Attribute.Lumina);
        }

        public static bool IsTripleNode(out bool isPositive, params IEnumerable<Bakugan> bakugans)
        {
            List<Attribute> attrs = [];
            foreach (Bakugan b in bakugans)
            {
                if (b.attributeChanges.Count == 0)
                    attrs.Add(b.BaseAttribute);
                else
                    attrs.AddRange(b.attributeChanges[^1].Attributes);
            }
            isPositive = false;
            if (attrs.Contains(Attribute.Aqua) && attrs.Contains(Attribute.Nova) && attrs.Contains(Attribute.Lumina))
            {
                isPositive = true; return true;
            }
            else if
                (attrs.Contains(Attribute.Subterra) && attrs.Contains(Attribute.Zephyros) && attrs.Contains(Attribute.Darkon)) return true;
            else return false;
        }
    }
}
