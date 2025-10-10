using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class MirrorFlash : AbilityCard
{
    public MirrorFlash(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position && x.IsOpponentOf(User) }
        ];
    }

    public override void TriggerEffect()
    {
        Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

        short difference = (short)(User.Power - target.Power);
        User.Boost(-difference, this);
        target.Boost(difference, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.Position is GateCard posGate && posGate.BattleStarting && user.IsAttribute(Attribute.Lumina);

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(27, CardKind.NormalAbility, (cID, owner) => new MirrorFlash(cID, owner, 27));
}
