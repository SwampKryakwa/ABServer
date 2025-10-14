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
        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

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