using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class HolyLight : AbilityCard
{
    public HolyLight(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BG", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_REVIVETARGET", TargetValidator = x => x.Power < User.Power && x.InDrop() }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (target.InDrop())
            target.MoveFromDropToHand();
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Lumina) && user.OnField() && Owner.BakuganDrop.Bakugans.Any(x => x.Power < user.Power);

    [ModuleInitializer]
    internal static void Init() => Register(2, CardKind.NormalAbility, (cID, owner) => new HolyLight(cID, owner, 2));
}
