using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class MirrorFlash : AbilityCard
{
    public MirrorFlash(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.BasePower > User.BasePower }
        ];
    }

    public override void TriggerEffect()
    {
        Bakugan target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

        short difference = (short)(User.Power - target.Power);
        User.Boost(-difference, this);
        target.Boost(difference, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.InBattle;

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Normal && Owner.BakuganOwned.Count(x => x.IsAttribute(Attribute.Lumina)) >= 2;

    [ModuleInitializer]
    internal static void Init() => Register(27, CardKind.NormalAbility, (cID, owner) => new MirrorFlash(cID, owner, 27));
}
