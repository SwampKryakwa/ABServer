using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class CrystalFang : AbilityCard
{
    public CrystalFang(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField()}
        ];
    }

    public override void TriggerEffect()
    {
        (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Boost(-100, this);
    }

    public override bool UserValidator(Bakugan user) =>
        (user.OnField() || user.InHand()) && user.Type == BakuganType.Tigress;

    [ModuleInitializer]
    internal static void Init() => Register(16, CardKind.NormalAbility, (cID, owner) => new CrystalFang(cID, owner, 16));
}


