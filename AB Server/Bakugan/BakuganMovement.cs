using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server;

internal partial class Bakugan
{

    public void AddFromHandToField(GateCard destination, MoveSource mover = MoveSource.Effect)
    {
        if (IsDummy) return;

        var wasBattleGoing = destination.IsBattleGoing;

        if (destination.MovingInEffectBlocking.Count != 0)
            return;

        Position.Remove(this);
        Position = destination;
        destination.BattleOver = false;
        destination.Bakugans.Add(this);
        destination.EnterOrder.Add([this]);
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganRemovedFromHand",
            ["Owner"] = Owner.PlayerId,
            ["BakuganType"] = (int)Type,
            ["Attribute"] = (int)BaseAttribute,
            ["Treatment"] = (int)Treatment,
            ["BasePower"] = BasePower,
            ["Power"] = Power,
            ["IsPartner"] = IsPartner,
            ["BID"] = BID
        });
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganAddedEvent",
            ["PosX"] = destination.Position.X,
            ["PosY"] = destination.Position.Y,
            ["Owner"] = Owner.PlayerId,
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
        Game.OnSingleBakuganFromHandsToField(this, destination);
        OnFromHandToField?.Invoke();
        if (!wasBattleGoing && destination.IsBattleGoing)
        {
            Game.OnBattleAboutToStart?.Invoke(destination);
        }

        // Reset flags
        BattleEndedInDraw = false;
        destination.BattleDeclaredOver = false;
        destination.BattleOver = false;
    }

    public void Throw(GateCard destination)
    {
        try
        {
            if (IsDummy) return;
            var wasBattleGoing = destination.IsBattleGoing;
            Position.Remove(this);
            Position = destination;
            destination.BattleOver = false;
            destination.Bakugans.Add(this);
            destination.EnterOrder.Add([this]);
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromHand",
                ["Owner"] = Owner.PlayerId,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganThrownEvent",
                ["PosX"] = destination.Position.X,
                ["PosY"] = destination.Position.Y,
                ["Owner"] = Owner.PlayerId,
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
            Game.OnSingleBakuganFromHandsToField(this, destination);
            OnFromHandToField?.Invoke();
            if (!wasBattleGoing && destination.IsBattleGoing)
                Game.OnBattleAboutToStart?.Invoke(destination);

            // Reset flags
            BattleEndedInDraw = false;
            destination.BattleDeclaredOver = false;
            destination.BattleOver = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void MoveOnField(GateCard destination, JObject MoveEffect, MoveSource mover = MoveSource.Effect)
    {
        if (IsDummy) return;

        if (destination.MovingInEffectBlocking.Count != 0)
            return;
        var wasBattleGoing = destination.IsBattleGoing;

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

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganMovedEvent",
                ["PosX"] = destination.Position.X,
                ["PosY"] = destination.Position.Y,
                ["Owner"] = Owner.PlayerId,
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
            Game.OnSingleBakuganMoved(this, destination);
            if (!wasBattleGoing && destination.IsBattleGoing)
            {
                Game.OnBattleAboutToStart?.Invoke(destination);
            }

            // Reset flags
            BattleEndedInDraw = false;
            destination.BattleDeclaredOver = false;
            destination.BattleOver = false;
        }
    }

    public static void MultiMove(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
    {
        if (destination.MovingInEffectBlocking.Count != 0)
            return;

        List<Bakugan> bakuganToMove = [.. bakugans];
        var wasBattleGoing = destination.IsBattleGoing;

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
                    ["Owner"] = bakugan.Owner.PlayerId,
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
            bakugan.BattleEndedInDraw = false; // Reset flag
        }

        game.OnBakugansMoved?.Invoke([.. bakugans.Select(b => (b, destination))]);

        if (!wasBattleGoing && destination.IsBattleGoing)
        {
            game.OnBattleAboutToStart?.Invoke(destination);
        }

    }

    public static void MultiAdd(Game game, GateCard destination, MoveSource mover, params Bakugan[] bakugans)
    {
        if (destination.MovingInEffectBlocking.Count != 0)
            return;
        var wasBattleGoing = destination.IsBattleGoing;

        List<Bakugan> bakuganToAdd = [.. bakugans];

        foreach (var bakugan in bakugans)
        {
            bakugan.Position.Remove(bakugan);

            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromHand",
                ["Owner"] = bakugan.Owner.PlayerId,
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
                ["Owner"] = bakugan.Owner.PlayerId,
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
            // Reset flags
            bakugan.BattleEndedInDraw = false;
            destination.BattleDeclaredOver = false;
            destination.BattleOver = false;
        }
        game.OnBakugansFromHandsToField?.Invoke([.. bakugans.Select(b => (b, destination))]);
        if (!wasBattleGoing && destination.IsBattleGoing)
            game.OnBattleAboutToStart?.Invoke(destination);
    }

    public void MoveFromDropToField(GateCard destination, MoveSource mover = MoveSource.Effect)
    {
        if (IsDummy) return;

        if (destination.MovingInEffectBlocking.Count != 0)
            return;
        var wasBattleGoing = destination.IsBattleGoing;

        Defeated = false;
        Position.Remove(this);
        Position = destination;

        destination.BattleOver = false;
        destination.Bakugans.Add(this);
        destination.EnterOrder.Add([this]);
        Game.OnSingleBakuganFromDropToField(this, destination);
        OnFromDropToField?.Invoke();
        if (!wasBattleGoing && destination.IsBattleGoing)
            Game.OnBattleAboutToStart?.Invoke(destination);

        Game.ThrowEvent(new JObject
        {
            ["Type"] = "HpRestored",
            ["Owner"] = Owner.PlayerId,
            ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
        });
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganAddedEvent",
            ["PosX"] = destination.Position.X,
            ["PosY"] = destination.Position.Y,
            ["Owner"] = Owner.PlayerId,
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
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganRemovedFromGrave",
            ["Owner"] = Owner.PlayerId,
            ["BakuganType"] = (int)Type,
            ["Attribute"] = (int)BaseAttribute,
            ["Treatment"] = (int)Treatment,
            ["Power"] = Power,
            ["IsPartner"] = IsPartner,
            ["BID"] = BID
        });

        // Reset flags
        BattleEndedInDraw = false;
        destination.BattleDeclaredOver = false;
        destination.BattleOver = false;
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
        Game.OnSingleBakuganFromDropToHand(this);
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "HpRestored",
            ["Owner"] = Owner.PlayerId,
            ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
        });
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganAddedToHand",
            ["Owner"] = Owner.PlayerId,
            ["BakuganType"] = (int)Type,
            ["Attribute"] = (int)BaseAttribute,
            ["Treatment"] = (int)Treatment,
            ["Power"] = Power,
            ["IsPartner"] = IsPartner,
            ["BID"] = BID
        });
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganRemovedFromGrave",
            ["Owner"] = Owner.PlayerId,
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

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemoved",
                ["Owner"] = Owner.PlayerId,
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
            Game.OnSingleBakuganFromFieldToHand(this);

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAddedToHand",
                ["Owner"] = Owner.PlayerId,
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

    public static void MultiToHand(Game game, Bakugan[] providedBakugans, MoveSource mover = MoveSource.Effect)
    {
        Console.WriteLine($"Provided Bakugan: " + providedBakugans.Count());
        var removableBakugans = providedBakugans.Where(x => !x.IsDummy && x.OnField()).ToArray();

        if (mover == MoveSource.Effect)
            removableBakugans = [.. providedBakugans.Where(x => (x.Position as GateCard)!.MovingAwayEffectBlocking.Count == 0)];
        Console.WriteLine($"Removable Bakugan: " + removableBakugans.Count());

        foreach (var bakugan in removableBakugans)
        {
            bakugan.Boosts.ForEach(x => x.Active = false);
            bakugan.Boosts.Clear();
            bakugan.OnRemovedFromField?.Invoke();
            game.OnBakugansFromFieldToHands?.Invoke(providedBakugans);

            var entryOrder = (bakugan.Position as GateCard)!.EnterOrder;
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
                ["Owner"] = x.Owner.PlayerId
            }))
        });

        foreach (var bakugan in removableBakugans)
            game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAddedToHand",
                ["Owner"] = bakugan.Owner.PlayerId,
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
            DestructionTurn = (byte)Game.turnNumber;
            DestroyedOn = positionGate;
            attributeChanges.Clear();
            Frenzied = false;
            Defeated = true;
            Position.Remove(this);
            Position = Owner.BakuganDrop;
            Owner.BakuganDrop.Bakugans.Add(this);

            int f = entryOrder.IndexOf(entryOrder.First(x => x.Contains(this)));
            if (entryOrder[f].Length == 1) entryOrder.RemoveAt(f);
            else entryOrder[f] = [.. entryOrder[f].Where(x => x != this)];

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemoved",
                ["Owner"] = Owner.PlayerId,
                ["IsDestroy"] = true,
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
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "HpLost",
                ["Owner"] = Owner.PlayerId,
                ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
            });

            Boosts.ForEach(x => x.Active = false);
            Boosts.Clear();

            OnRemovedFromField?.Invoke();
            OnDestroyed?.Invoke();
            Game.OnSingleBakuganDestroyed(this);

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAddedToGrave",
                ["Owner"] = Owner.PlayerId,
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

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganRemovedFromHand",
                ["Owner"] = Owner.PlayerId,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "HpLost",
                ["Owner"] = Owner.PlayerId,
                ["HpLeft"] = Owner.BakuganOwned.Count(x => !x.Defeated)
            });

            Boosts.ForEach(x => x.Active = false);
            Boosts.Clear();

            OnFromHandToDrop?.Invoke();
            OnDestroyed?.Invoke();
            Game.OnSingleBakuganDestroyed(this);

            Game.ThrowEvent(new JObject
            {
                ["Type"] = "BakuganAddedToGrave",
                ["Owner"] = Owner.PlayerId,
                ["BakuganType"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            });
        }
    }
}
