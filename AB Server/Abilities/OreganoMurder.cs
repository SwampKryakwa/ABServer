using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class OreganoMurder : AbilityCard
{
    public OreganoMurder(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new YesNoSelector { Message = "INFO_WANTTARGET", ForPlayer = (p) => p == Owner, Condition = () => Owner.Bakugans.Count == 0 && User.Position is GateCard posGate && posGate.Bakugans.Any(x=>x.IsOpponentOf(User)) },
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_BOOSTTARGET", TargetValidator = x => x.IsOpponentOf(User), Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
        ];
    }

    public override void Resolve()
    {
        User.Boost(100, this);
        base.Resolve();
    }

    public override void TriggerEffect()
    {
        if ((ResTargetSelectors[0] as YesNoSelector)!.IsYes)
            (ResTargetSelectors[1] as BakuganSelector)!.SelectedBakugan.Boost(-100, this);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Game.BakuganIndex.Any(x => x.OnField() && x.Owner.TeamId != user.Owner.TeamId);

    [ModuleInitializer]
    internal static void Init() => Register(49, CardKind.NormalAbility, (cID, owner) => new OreganoMurder(cID, owner, 49));
}
