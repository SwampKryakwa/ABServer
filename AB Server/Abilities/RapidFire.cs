using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class RapidFire : AbilityCard
{
    public RapidFire(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.OnField && x.Owner.TeamId != Owner.TeamId && x != User.Position },
            new BakuganSelector { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.InHand() }
        ];
    }

    public override void TriggerEffect()
    {
        var gateTarget = (ResTargetSelectors[0] as GateSelector)!.SelectedGate;
        var bakTarget = (ResTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;
        bakTarget.AddFromHandToField(gateTarget);
    }

    public override bool UserValidator(Bakugan user) =>
        user.Position is GateCard posGate && posGate.BattleStarting && user.IsAttribute(Attribute.Nova);

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(40, CardKind.NormalAbility, (cID, owner) => new RapidFire(cID, owner, 40));
}

