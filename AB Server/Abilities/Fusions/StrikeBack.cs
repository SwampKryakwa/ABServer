using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class StrikeBack : FusionAbility
{
    public StrikeBack(int cID, Player owner) : base(cID, owner, 2, typeof(DefiantCounterattack))
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsOpponentOf(User)}
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (User.InDrop())
        {
            User.MoveFromDropToField((target.Position as GateCard)!);
            User.Boost((short)(target.Power - User.Power + 10), this);
        }
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Intermediate && user.InDrop() && user.Type == BakuganType.Raptor && user.IsPartner && Game.BakuganIndex.Any(x => x.OnField() && x.IsOpponentOf(user));

    [ModuleInitializer]
    internal static void Init() => Register(3, (cID, owner) => new StrikeBack(cID, owner));
}
