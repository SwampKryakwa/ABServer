using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities;

internal static class GenericEffects
{
    /// <summary>
    /// Effect that moves a Bakugan to a specified Gate Card
    /// </summary>
    /// <remarks>
    /// When activated, this effect triggers an ability effect and moves the specified Bakugan to the target Gate Card.
    /// </remarks>
    /// <param name="target">The Bakugan being moved by the effect.</param>
    /// <param name="moveTarget">The Gate Card to move the Bakugan to.</param>
    /// <param name="moveEffect">An optional JObject describing the move effect animation.</param>
    public static void MoveBakuganEffect(Bakugan target, GateCard moveTarget, JObject? moveEffect = null)
    {
        target.Move(moveTarget, moveEffect ?? new JObject { ["MoveEffect"] = "None" }, MoveSource.Effect);
    }
}

/// <summary>
/// Represents an effect that applies a continuous boost to a Bakugan, which can be negated.
/// </summary>
/// <remarks>
/// This effect adds itself to the game's ActiveZone, applies a continuous boost to the user Bakugan,
/// and can be negated to remove the boost and itself from the ActiveZone.
/// </remarks>
/// <param name="user">The Bakugan receiving the continuous boost.</param>
/// <param name="boostAmount">The amount of the continuous boost.</param>
/// <param name="typeId">The type identifier for the effect.</param>
/// <param name="kind">The kind of card or effect.</param>
/// <param name="isCopy">Whether this effect is a copy.</param>
class ContinuousBoostEffect(Bakugan user, Bakugan target, short boostAmount, int typeId, CardKind kind, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = kind;
    public Bakugan User { get; set; } = user;
    public Bakugan target = target;
    Game game { get => User.Game; }
    Boost currentBoost;

    public Player Owner { get; set; } = user.Owner;
    bool IsCopy = isCopy;

    public void Activate()
    {
        game.ActiveZone.Add(this);


        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        currentBoost = new Boost(boostAmount);
        target.ContinuousBoost(currentBoost, this);
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        if (currentBoost.Active)
        {
            currentBoost.Active = false;
            target.RemoveBoost(currentBoost, this);
        }

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Represents an effect that applies a continuous boost to a Bakugan, which is automatically removed when the user Bakugan is defeated.
/// </summary>
/// <remarks>
/// This effect adds itself to the game's ActiveZone, applies a continuous boost to the user Bakugan,
/// and automatically removes the boost and itself from the ActiveZone when the user Bakugan is defeated.
/// </remarks>
/// <param name="user">The Bakugan receiving the continuous boost.</param>
/// <param name="boostAmount">The amount of the continuous boost.</param>
/// <param name="typeId">The type identifier for the effect.</param>
/// <param name="kind">The kind of card or effect.</param>
/// <param name="isCopy">Whether this effect is a copy.</param>
class ContinuousBoostUntilDestroyedEffect(Bakugan user, Bakugan target, short boostAmount, int typeId, CardKind kind, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = kind;
    public Bakugan User { get; set; } = user;
    public Bakugan target = target;
    Game game { get => User.Game; }
    Boost currentBoost;

    public Player Owner { get; set; } = user.Owner;
    bool IsCopy = isCopy;

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        currentBoost = new Boost(boostAmount);
        target.ContinuousBoost(currentBoost, this);


        User.OnDestroyed += OnUserDestroyed;
    }

    private void OnUserDestroyed()
    {
        game.ActiveZone.Remove(this);

        if (currentBoost.Active)
        {
            currentBoost.Active = false;
            target.RemoveBoost(currentBoost, this);
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        if (currentBoost.Active)
        {
            currentBoost.Active = false;
            target.RemoveBoost(currentBoost, this);
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Represents an effect that applies the same continuous boost amount to multiple target Bakugan.
/// </summary>
/// <remarks>
/// This effect applies a continuous boost to each Bakugan in the provided targets array and adds itself to the game's ActiveZone.
/// The boosts can be removed by calling Negate.
/// </remarks>
/// <param name="user">The Bakugan using the effect.</param>
/// <param name="boostTargets">The array of Bakugan to receive the continuous boost.</param>
/// <param name="boostAmount">The amount of the continuous boost to apply to each target.</param>
/// <param name="typeId">The type identifier for the effect.</param>
/// <param name="kindId">The kind identifier for the effect.</param>
/// <param name="isCopy">Whether this effect is a copy.</param>
class ContinuousBoostMultipleSameEffect(Bakugan user, Bakugan[] boostTargets, short boostAmount, int typeId, int kindId, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int KindId { get; } = kindId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = (CardKind)kindId;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    Boost[] currentBoosts;
    Bakugan[] Targets = boostTargets;
    bool IsCopy = isCopy;
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        currentBoosts = new Boost[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            currentBoosts[i] = new Boost(boostAmount);
            Targets[i].ContinuousBoost(currentBoosts[i], this);
        }
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        if (currentBoosts != null)
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                if (currentBoosts[i].Active)
                {
                    currentBoosts[i].Active = false;
                    Targets[i].RemoveBoost(currentBoosts[i], this);
                }
            }
        }

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Represents an effect that applies different continuous boost amounts to multiple target Bakugan.
/// </summary>
/// <remarks>
/// This effect applies a corresponding continuous boost amount from the boostAmounts array to each Bakugan in the boostTargets array and adds itself to the game's ActiveZone.
/// The boosts can be removed by calling Negate.
/// </remarks>
/// <param name="user">The Bakugan using the effect.</param>
/// <param name="boostTargets">The array of Bakugan to receive the continuous boost.</param>
/// <param name="boostAmounts">The array of continuous boost amounts, each corresponding to a target Bakugan.</param>
/// <param name="typeId">The type identifier for the effect.</param>
/// <param name="kindId">The kind identifier for the effect.</param>
/// <param name="isCopy">Whether this effect is a copy.</param>
class ContinuousBoostMultipleVariousEffect(Bakugan user, Bakugan[] boostTargets, short[] boostAmounts, int typeId, int kindId, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int KindId { get; } = kindId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = (CardKind)kindId;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    Boost[] currentBoosts;
    Bakugan[] Targets = boostTargets;
    short[] BoostAmounts = boostAmounts;
    bool IsCopy = isCopy;
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);


        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        currentBoosts = new Boost[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            currentBoosts[i] = new Boost(BoostAmounts[i]);
            Targets[i].ContinuousBoost(currentBoosts[i], this);
        }
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        if (currentBoosts != null)
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                if (currentBoosts[i].Active)
                {
                    currentBoosts[i].Active = false;
                    Targets[i].RemoveBoost(currentBoosts[i], this);
                }
            }
        }

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Represents an effect that applies a continuous boost to all Bakugan currently on the field.
/// </summary>
/// <remarks>
/// This effect applies a continuous boost to every Bakugan that is currently on the field and adds itself to the game's ActiveZone.
/// The boosts can be removed by calling Negate.
/// </remarks>
/// <param name="user">The Bakugan using the effect.</param>
/// <param name="boostAmount">The amount of continuous boost to apply to each Bakugan on the field.</param>
/// <param name="typeId">The type identifier for the effect.</param>
/// <param name="kindId">The kind identifier for the effect.</param>
/// <param name="isCopy">Whether this effect is a copy.</param>
class ContinuousBoostAllFieldEffect(Bakugan user, short boostAmount, int typeId, int kindId, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int KindId { get; } = kindId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = (CardKind)kindId;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    List<Boost> currentBoosts = new();
    List<Bakugan> Targets = new();
    bool IsCopy = isCopy;
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);


        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        foreach (Bakugan target in game.BakuganIndex.Where(x => x.OnField()))
        {
            var boost = new Boost(boostAmount);
            target.ContinuousBoost(boost, this);
            currentBoosts.Add(boost);
            Targets.Add(target);
        }
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        for (int i = 0; i < Targets.Count; i++)
        {
            if (currentBoosts[i].Active)
            {
                currentBoosts[i].Active = false;
                Targets[i].RemoveBoost(currentBoosts[i], this);
            }
        }

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Applies the same continuous boost to multiple Bakugan, removed when the user Bakugan is defeated.
/// </summary>
class ContinuousBoostMultipleSameUntilDestroyedEffect(Bakugan user, Bakugan[] boostTargets, short boostAmount, int typeId, int kindId, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int KindId { get; } = kindId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = (CardKind)kindId;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    Boost[] currentBoosts;
    Bakugan[] Targets = boostTargets;
    bool IsCopy = isCopy;
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        currentBoosts = new Boost[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            currentBoosts[i] = new Boost(boostAmount);
            Targets[i].ContinuousBoost(currentBoosts[i], this);
        }

        User.OnDestroyed += OnUserDestroyed;
    }

    private void OnUserDestroyed()
    {
        game.ActiveZone.Remove(this);

        for (int i = 0; i < Targets.Length; i++)
        {
            if (currentBoosts[i].Active)
            {
                currentBoosts[i].Active = false;
                Targets[i].RemoveBoost(currentBoosts[i], this);
            }
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }


    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        if (currentBoosts != null)
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                if (currentBoosts[i].Active)
                {
                    currentBoosts[i].Active = false;
                    Targets[i].RemoveBoost(currentBoosts[i], this);
                }
            }
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Applies different continuous boost amounts to multiple Bakugan, removed when the user Bakugan is defeated.
/// </summary>
class ContinuousBoostMultipleVariousUntilDefeatedEffect(Bakugan user, Bakugan[] boostTargets, short[] boostAmounts, int typeId, int kindId, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int KindId { get; } = kindId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = (CardKind)kindId;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    Boost[] currentBoosts;
    Bakugan[] Targets = boostTargets;
    short[] BoostAmounts = boostAmounts;
    bool IsCopy = isCopy;
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);


        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        currentBoosts = new Boost[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            currentBoosts[i] = new Boost(BoostAmounts[i]);
            Targets[i].ContinuousBoost(currentBoosts[i], this);
        }

        User.OnDestroyed += OnUserDestroyed;
    }

    private void OnUserDestroyed()
    {
        game.ActiveZone.Remove(this);

        for (int i = 0; i < Targets.Length; i++)
        {
            if (currentBoosts[i].Active)
            {
                currentBoosts[i].Active = false;
                Targets[i].RemoveBoost(currentBoosts[i], this);
            }
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        if (currentBoosts != null)
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                if (currentBoosts[i].Active)
                {
                    currentBoosts[i].Active = false;
                    Targets[i].RemoveBoost(currentBoosts[i], this);
                }
            }
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Applies a continuous boost to all Bakugan on the field, removed when the user Bakugan is defeated.
/// </summary>
class ContinuousBoostAllFieldUntilDefeatedEffect(Bakugan user, short boostAmount, int typeId, int kindId, bool isCopy) : IActive
{
    public int TypeId { get; } = typeId;
    public int KindId { get; } = kindId;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = (CardKind)kindId;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    List<Boost> currentBoosts = new();
    List<Bakugan> Targets = new();
    bool IsCopy = isCopy;
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);


        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        foreach (Bakugan target in game.BakuganIndex.Where(x => x.OnField()))
        {
            var boost = new Boost(boostAmount);
            target.ContinuousBoost(boost, this);
            currentBoosts.Add(boost);
            Targets.Add(target);
        }

        User.OnDestroyed += OnUserDestroyed;
    }

    private void OnUserDestroyed()
    {
        game.ActiveZone.Remove(this);

        for (int i = 0; i < Targets.Count; i++)
        {
            if (currentBoosts[i].Active)
            {
                currentBoosts[i].Active = false;
                Targets[i].RemoveBoost(currentBoosts[i], this);
            }
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }

    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        for (int i = 0; i < Targets.Count; i++)
        {
            if (currentBoosts[i].Active)
            {
                currentBoosts[i].Active = false;
                Targets[i].RemoveBoost(currentBoosts[i], this);
            }
        }

        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}

/// <summary>
/// Effect that negates a specified Gate Card
/// </summary>
/// <remarks>
/// When activated, this effect triggers an ability effect negates the target Gate Card.
/// </remarks>
/// <param name="user">The Bakugan using the effect.</param>
/// <param name="target">The Gate Card being negated.</param>
/// <param name="typeId">The type identifier for the effect.</param>
/// <param name="kindId">The kind identifier for the effect.</param>
/// <param name="isCopy">Whether this effect is a copy.</param>
internal class NegateGateEffect(Bakugan user, GateCard target, int typeId, int kindId, bool isCopy)
{
    int typeId = typeId;
    int kindId = kindId;
    Bakugan user = user;
    GateCard target = target;
    Game game { get => user.Game; }
    bool isCopy = isCopy;

    public void Activate()
    {


        target.IsOpen = true;
        target.Negate();
    }
}