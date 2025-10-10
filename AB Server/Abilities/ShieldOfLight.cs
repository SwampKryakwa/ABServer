using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class ShieldOfLight : AbilityCard
{
    public ShieldOfLight(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position && x.InBattle && x.IsOpponentOf(User)}
        ];
    }

    public override void TriggerEffect()
    {
        if (CondTargetSelectors[0] is BakuganSelector targetSelector)
        {
            if (!targetSelector.SelectedBakugan.IsAttribute(Attribute.Lumina))
                targetSelector.SelectedBakugan.Boost(-100, this);
            if (targetSelector.SelectedBakugan.Power > User.Power)
                targetSelector.SelectedBakugan.Boost(-100, this);
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.InBattle && user.IsAttribute(Attribute.Lumina);

    [ModuleInitializer]
    internal static void Init() => Register(26, CardKind.NormalAbility, (cID, owner) => new ShieldOfLight(cID, owner, 26));
}
