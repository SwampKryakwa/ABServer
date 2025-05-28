using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    /// <summary>
    /// Represents an effect that applies a boost to a target Bakugan during gameplay.
    /// </summary>
    /// <remarks>
    /// This effect is associated with a specific user Bakugan and a target Bakugan. When activated, 
    /// it applies a boost of the specified amount to the target Bakugan and triggers an in-game event 
    /// to notify other components of the effect activation.
    /// </remarks>
    /// <param name="user">The Bakugan using the effect.</param>
    /// <param name="boostTarget">The Bakugan receiving the boost.</param>
    /// <param name="boostAmmount">The amount of boost to apply.</param>
    /// <param name="typeId">The type identifier for the effect.</param>
    /// <param name="kindId">The kind identifier for the effect.</param>
    class BoostEffect(Bakugan user, Bakugan boostTarget, short boostAmmount, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        Bakugan target = boostTarget;
        short boostAmmount = boostAmmount;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            target.Boost(new Boost(boostAmmount), this);
        }
    }

    /// <summary>
    /// Represents an effect that applies the same boost amount to multiple target Bakugan.
    /// </summary>
    /// <remarks>
    /// When activated, this effect applies a boost of the specified amount to each Bakugan in the provided targets array.
    /// An in-game event is triggered to notify other components of the effect activation.
    /// </remarks>
    /// <param name="user">The Bakugan using the effect.</param>
    /// <param name="boostTargets">The array of Bakugan to receive the boost.</param>
    /// <param name="boostAmmount">The amount of boost to apply to each target.</param>
    /// <param name="typeId">The type identifier for the effect.</param>
    /// <param name="kindId">The kind identifier for the effect.</param>
    class BoostMultipleSameEffect(Bakugan user, Bakugan[] boostTargets, short boostAmmount, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        Bakugan[] targets = boostTargets;
        short boostAmmount = boostAmmount;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            foreach (Bakugan target in targets)
                target.Boost(new Boost(boostAmmount), this);
        }
    }

    /// <summary>
    /// Represents an effect that applies different boost amounts to multiple target Bakugan.
    /// </summary>
    /// <remarks>
    /// When activated, this effect applies a corresponding boost amount from the boostAmmounts array to each Bakugan in the boostTargets array.
    /// An in-game event is triggered to notify other components of the effect activation.
    /// </remarks>
    /// <param name="user">The Bakugan using the effect.</param>
    /// <param name="boostTargets">The array of Bakugan to receive the boost.</param>
    /// <param name="boostAmmounts">The array of boost amounts, each corresponding to a target Bakugan.</param>
    /// <param name="typeId">The type identifier for the effect.</param>
    /// <param name="kindId">The kind identifier for the effect.</param>
    class BoostMultipleVariousEffect(Bakugan user, Bakugan[] boostTargets, short[] boostAmmounts, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        Bakugan[] targets = boostTargets;
        short[] boostAmmounts = boostAmmounts;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            foreach ((Bakugan target, short boostAmmount) in targets.Zip(boostAmmounts, Tuple.Create))
                target.Boost(new Boost(boostAmmount), this);
        }
    }

    /// <summary>
    /// Represents an effect that applies a boost to all Bakugan currently on the field.
    /// </summary>
    /// <remarks>
    /// When activated, this effect applies a boost of the specified amount to every Bakugan that is currently on the field.
    /// An in-game event is triggered to notify other components of the effect activation.
    /// </remarks>
    /// <param name="user">The Bakugan using the effect.</param>
    /// <param name="boostAmmount">The amount of boost to apply to each Bakugan on the field.</param>
    /// <param name="typeId">The type identifier for the effect.</param>
    /// <param name="kindId">The kind identifier for the effect.</param>
    class BoostAllFieldEffect(Bakugan user, short boostAmmount, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        short boostAmmount = boostAmmount;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            foreach (Bakugan target in game.BakuganIndex.Where(x => x.OnField()))
                target.Boost(new Boost(boostAmmount), this);
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

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
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
    class ContinuousBoostUntilDefeatedEffect(Bakugan user, Bakugan target, short boostAmount, int typeId, CardKind kind, bool isCopy) : IActive
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

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            currentBoost = new Boost(boostAmount);
            target.ContinuousBoost(currentBoost, this);

            // Subscribe to the BakuganDestroyed event for this Bakugan
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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });

            // Unsubscribe to avoid memory leaks
            User.OnDestroyed -= OnUserDestroyed;
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            if (currentBoost.Active)
            {
                currentBoost.Active = false;
                target.RemoveBoost(currentBoost, this);
            }

            // Unsubscribe in case Negate is called directly
            User.OnDestroyed -= OnUserDestroyed;

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
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
        Game game { get => user.Game; }
        Boost[] currentBoosts;
        Bakugan[] Targets = boostTargets;
        bool IsCopy = isCopy;
        public Player Owner { get; set; } = user.Owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
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
        Game game { get => user.Game; }
        Boost[] currentBoosts;
        Bakugan[] Targets = boostTargets;
        short[] BoostAmounts = boostAmounts;
        bool IsCopy = isCopy;
        public Player Owner { get; set; } = user.Owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
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
        Game game { get => user.Game; }
        List<Boost> currentBoosts = new();
        List<Bakugan> Targets = new();
        bool IsCopy = isCopy;
        public Player Owner { get; set; } = user.Owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
        }
    }

    /// <summary>
    /// Applies the same continuous boost to multiple Bakugan, removed when the user Bakugan is defeated.
    /// </summary>
    class ContinuousBoostMultipleSameUntilDefeatedEffect(Bakugan user, Bakugan[] boostTargets, short boostAmount, int typeId, int kindId, bool isCopy) : IActive
    {
        public int TypeId { get; } = typeId;
        public int KindId { get; } = kindId;
        public int EffectId { get; set; } = user.Game.NextEffectId++;
        public CardKind Kind { get; } = (CardKind)kindId;
        public Bakugan User { get; set; } = user;
        Game game { get => user.Game; }
        Boost[] currentBoosts;
        Bakugan[] Targets = boostTargets;
        bool IsCopy = isCopy;
        public Player Owner { get; set; } = user.Owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });

            User.OnDestroyed -= OnUserDestroyed;
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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
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
        Game game { get => user.Game; }
        Boost[] currentBoosts;
        Bakugan[] Targets = boostTargets;
        short[] BoostAmounts = boostAmounts;
        bool IsCopy = isCopy;
        public Player Owner { get; set; } = user.Owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            currentBoosts = new Boost[Targets.Length];
            for (int i = 0; i < Targets.Length; i++)
            {
                currentBoosts[i] = new Boost(BoostAmounts[i]);
                Targets[i].ContinuousBoost(currentBoosts[i], this);
            }

            User.OnDestroyed += OnUserDefeated;
        }

        private void OnUserDefeated()
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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });

            User.OnDestroyed -= OnUserDefeated;
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

            User.OnDestroyed -= OnUserDefeated;

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
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
        Game game { get => user.Game; }
        List<Boost> currentBoosts = new();
        List<Bakugan> Targets = new();
        bool IsCopy = isCopy;
        public Player Owner { get; set; } = user.Owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            foreach (Bakugan target in game.BakuganIndex.Where(x => x.OnField()))
            {
                var boost = new Boost(boostAmount);
                target.ContinuousBoost(boost, this);
                currentBoosts.Add(boost);
                Targets.Add(target);
            }

            User.OnDestroyed += OnUserDefeated;
        }

        private void OnUserDefeated()
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

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });

            User.OnDestroyed -= OnUserDefeated;
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

            User.OnDestroyed -= OnUserDefeated;

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
        }
    }

    /// <summary>
    /// Effect that moves a Bakugan to a specified Gate Card
    /// </summary>
    /// <remarks>
    /// When activated, this effect triggers an ability effect and moves the specified Bakugan to the target Gate Card.
    /// </remarks>
    /// <param name="user">The Bakugan being moved by the effect.</param>
    /// <param name="moveTarget">The Gate Card to move the Bakugan to.</param>
    /// <param name="typeId">The type identifier for the effect.</param>
    /// <param name="kindId">The kind identifier for the effect.</param>
    /// <param name="moveEffect">An optional JObject describing the move effect animation.</param>
    /// <param name="isCopy">Whether this effect is a copy.</param>
    class MoveEffect(Bakugan user, Bakugan target, GateCard moveTarget, int typeId, int kindId, JObject? moveEffect = null, bool isCopy = false)
    {
        int typeId = typeId;
        int kindId = kindId;
        Bakugan user = user;
        Bakugan target = target;
        Game game { get => user.Game; }
        GateCard moveTarget = moveTarget;
        JObject moveEffect = moveEffect ?? new JObject { ["MoveEffect"] = "None" };
        bool IsCopy = isCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(typeId, kindId, user));
            target.Move(moveTarget, moveEffect, MoveSource.Effect);
        }
    }

    /// <summary>
    /// Effect that negates a specified Gate Card
    /// </summary>
    /// <remarks>
    /// When activated, this effect triggers an ability effect negates the target Gate Card.
    /// </remarks>
    /// <param name="user">The Bakugan being moved by the effect.</param>
    /// <param name="moveTarget">The Gate Card to move the Bakugan to.</param>
    /// <param name="typeId">The type identifier for the effect.</param>
    /// <param name="kindId">The kind identifier for the effect.</param>
    /// <param name="moveEffect">An optional JObject describing the move effect animation.</param>
    /// <param name="isCopy">Whether this effect is a copy.</param>
    internal class NegateGateEffect(Bakugan user, GateCard target, int typeID, int kindId, bool IsCopy)
    {
        int typeId = typeID;
        int kindId = kindId;
        Bakugan user = user;
        GateCard target = target;
        Game game { get => user.Game; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(typeId, kindId, user));

            target.IsOpen = true;
            target.Negate();
        }
    }
}