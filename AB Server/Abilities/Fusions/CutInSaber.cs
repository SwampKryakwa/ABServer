using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class CutInSaber : FusionAbility
{
    public CutInSaber(int cID, Player owner) : base(cID, owner, 3, typeof(CrystalFang))
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = possibleTarget => possibleTarget.Owner != Owner && possibleTarget.Position is GateCard posGate && posGate.BattleStarting }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (User.InHand() && target.Position is GateCard posGate)
            User.AddFromHandToField(posGate);
    }

    public override bool UserValidator(Bakugan user) =>
    user.Type == BakuganType.Tigress && user.InHand();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(7, (cID, owner) => new CutInSaber(cID, owner));
}