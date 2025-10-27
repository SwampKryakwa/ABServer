using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Correlations;

internal class TripleNode : AbilityCard
{
    public TripleNode(int cID, Player owner) : base(cID, owner, 2)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField() && target.Owner == owner && target != User },
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField() && target.Owner == owner && target != User && target != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
        ];
    }

    public override CardKind Kind { get; } = CardKind.CorrelationAbility;

    public override void TriggerEffect()
    {
        if (CondTargetSelectors[0] is BakuganSelector targetSelector1 && CondTargetSelectors[1] is BakuganSelector targetSelector2 && Bakugan.IsTripleNode(out _, targetSelector1.SelectedBakugan, targetSelector2.SelectedBakugan, User))
        {
            User.Boost(200, this);
            targetSelector1.SelectedBakugan.Boost(200, this);
            targetSelector2.SelectedBakugan.Boost(200, this);
        }
    }

    public override bool IsActivateable() =>
        Owner.AbilityBlockers.Count == 0 && Owner.BlueAbilityBlockers.Count == 0 && Owner.BakuganOwned.Any(IsActivateableByBakugan);

    public override bool UserValidator(Bakugan user) => user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(2, CardKind.CorrelationAbility, (cID, owner) => new TripleNode(cID, owner));
}