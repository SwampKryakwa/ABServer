using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class HolyLight : AbilityCard
{
    public HolyLight(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BG", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_REVIVETARGET", TargetValidator = x => x.Power != User.Power && x.Type != User.Type && x.InDrop() }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        target.MoveFromDropToHand();
        if (target.IsOpponentOf(User))
            User.Boost(300, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.IsAttribute(Attribute.Lumina) && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(2, CardKind.NormalAbility, (cID, owner) => new HolyLight(cID, owner, 2));
}
