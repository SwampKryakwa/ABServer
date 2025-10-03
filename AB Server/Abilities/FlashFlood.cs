using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class FlashFlood : AbilityCard
{
    public FlashFlood(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsAttribute(Attribute.Aqua) && x != User },
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsAttribute(Attribute.Aqua) && x != User && x != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
        ];
    }

    public override void TriggerEffect()
    {
        foreach (var bak in Game.BakuganIndex.Where(x => x.OnField() && x.Owner.TeamId != Owner.TeamId))
            bak.Boost(-bak.Power, this);

        if ((CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Position is GateCard posGate)
            (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.MoveFromFieldToHand(posGate.EnterOrder);
        if ((CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan.Position is GateCard otherPosGate)
            (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan.MoveFromFieldToHand(otherPosGate.EnterOrder);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Aqua) && Game.BakuganIndex.Count(x => x.OnField() && x.IsAttribute(Attribute.Aqua)) >= 3;

    [ModuleInitializer]
    internal static void Init() => Register(46, CardKind.NormalAbility, (cID, owner) => new FlashFlood(cID, owner, 46));
}
