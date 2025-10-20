namespace AB_Server.Abilities;

internal abstract partial class FusionAbility(int cID, Player owner, int typeId, Type baseAbilityType) : AbilityCard(cID, owner, typeId)
{
    public static Func<int, Player, FusionAbility>[] FusionCtrs = Array.Empty<Func<int, Player, FusionAbility>>();

    internal static void Register(int typeId, Func<int, Player, FusionAbility> constructor)
    {
        if (FusionCtrs.Length <= typeId)
            Array.Resize(ref FusionCtrs, typeId + 1);
        FusionCtrs[typeId] = constructor;
    }

    public override CardKind Kind { get; } = CardKind.FusionAbility;
    public AbilityCard FusedTo;

    public override bool IsActivateable() =>
        Owner.AbilityBlockers.Count == 0 && Owner.GreenAbilityBlockers.Count == 0 && Owner.BakuganOwned.Any(x => IsActivateableByBakugan(x) && x.IsPartner) && Owner.AbilityHand.Any(baseAbilityType.IsInstanceOfType);

    public override void Setup(bool asCounter)
    {
        this.asCounter = asCounter;
        Game.ThrowEvent(Owner.PlayerId, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
            EventBuilder.AbilitySelection("INFO_FUSIONBASE", Owner.AbilityHand.Where(baseAbilityType.IsInstanceOfType))
            ));
        Game.OnAnswer[Owner.PlayerId] = PickUser;
    }

    public virtual void PickUser()
    {
        FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["ability"]];

        Game.ThrowEvent(Owner.PlayerId, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
            EventBuilder.AnyBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(UserValidator))
            ));

        Game.OnAnswer[Owner.PlayerId] = RecieveUser;
    }

    public override void Activate()
    {
        FusedTo.Discard();

        EffectId = Game.NextEffectId++;

        Game.ThrowEvent(new()
        {
            ["Type"] = "AbilityAddedActiveZone",
            ["IsCopy"] = IsCopy,
            ["Id"] = EffectId,
            ["Card"] = TypeId,
            ["Kind"] = (int)Kind,
            ["User"] = User.BID,
            ["IsCounter"] = asCounter,
            ["Owner"] = Owner.PlayerId
        });

        Game.ActiveZone.Add(this);
        Game.CardChain.Push(this);
        Game.CheckChain(Owner, this, User);
    }
}
