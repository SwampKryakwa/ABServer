using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class DefiantCounterattack : AbilityCard
{
    public DefiantCounterattack(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.OnField && x.Bakugans.Any(User.IsOpponentOf) }
        ];
    }

    public override void TriggerEffect()
    {
        if (User.InDrop() && (ResTargetSelectors[0] as GateSelector)!.SelectedGate is GateCard targetGate && targetGate.OnField)
            User.MoveFromDropToField(targetGate);
    }

    public override bool UserValidator(Bakugan user) =>
        user.Type == BakuganType.Raptor && user.InDrop();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(15, CardKind.NormalAbility, (cID, owner) => new DefiantCounterattack(cID, owner, 15));
}

