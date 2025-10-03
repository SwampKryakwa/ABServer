using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class GaleStrike : AbilityCard
{
    public GaleStrike(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner.TeamId != Owner.TeamId }
        ];
    }

    public override void TriggerEffect()
    {
        BakuganSelector bakSelector = (CondTargetSelectors[0] as BakuganSelector)!;
        if (bakSelector.SelectedBakugan.Position is GateCard posGate && bakSelector.SelectedBakugan.Power < User.Power)
            bakSelector.SelectedBakugan.MoveFromFieldToDrop(posGate.EnterOrder);
        else
            bakSelector.SelectedBakugan.Boost(-300, this);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Zephyros) && user.Owner.BakuganOwned.Count(x => x.OnField() && x.IsAttribute(Attribute.Zephyros)) >= 3;

    public static new bool HasValidTargets(Bakugan user) =>
        user.Position.Bakugans.Any(x => x.Owner != user.Owner);

    [ModuleInitializer]
    internal static void Init() => Register(42, CardKind.NormalAbility, (cID, owner) => new GaleStrike(cID, owner, 42));
}
