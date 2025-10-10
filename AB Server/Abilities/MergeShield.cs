using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class MergeShield : AbilityCard
{
    public MergeShield(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() }
        ];
    }

    public override void TriggerEffect()
    {
        User.Boost(Math.Abs(User.Power - (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Power), this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Darkon);

    [ModuleInitializer]
    internal static void Init() => Register(29, CardKind.NormalAbility, (cID, owner) => new MergeShield(cID, owner, 29));
}
