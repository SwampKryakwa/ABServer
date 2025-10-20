using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class LuminousEntrust : AbilityCard
{
    public LuminousEntrust(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.InHand() && x.Owner == Owner },
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.OnField }
        ];
    }

    public override void TriggerEffect()
    {
        var bakTarget = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        bakTarget.AddFromHandToField((CondTargetSelectors[1] as GateSelector)!.SelectedGate);
        bakTarget.Boost(50, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.IsAttribute(Attribute.Lumina) && user.InDrop();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(52, CardKind.NormalAbility, (cID, owner) => new LuminousEntrust(cID, owner, 52));
}