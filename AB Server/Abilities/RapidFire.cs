using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class RapidFire : AbilityCard
{
    public RapidFire(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.OnField && x.Owner.TeamId != Owner.TeamId },
            new BakuganSelector { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.InHand() }
        ];
    }

    public override void TriggerEffect()
    {
        var gateTarget = (CondTargetSelectors[0] as GateSelector)!.SelectedGate;
        var bakTarget = (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;
        bakTarget.AddFromHandToField(gateTarget);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Intermediate && user.Position is GateCard posGate && posGate.BattleOver && user.IsAttribute(Attribute.Nova) && Game.GateIndex.Any(x => x.OnField && x.Owner.TeamId != Owner.TeamId) && Owner.Bakugans.Count != 0;

    public static new bool HasValidTargets(Bakugan user) =>
        user.Position.Bakugans.Any(x => x.Owner != user.Owner);

    [ModuleInitializer]
    internal static void Init() => Register(40, CardKind.NormalAbility, (cID, owner) => new RapidFire(cID, owner, 40));
}

