using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class SaurusRage : FusionAbility
{
    public SaurusRage(int cID, Player owner) : base(cID, owner, 4, typeof(SaurusGlow))
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Power > User.Power }
        ];
    }

    public override void TriggerEffect() =>
        User.Boost(new Boost((short)Math.Abs((User.Power - (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Power) * 2)), this);

    public override bool UserValidator(Bakugan user) =>
        user.Type == BakuganType.Saurus && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(5, (cID, owner) => new SaurusRage(cID, owner));
}
