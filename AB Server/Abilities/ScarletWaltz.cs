using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class ScarletWaltz : AbilityCard
{
    public ScarletWaltz(int cId, Player owner, int typeId) : base(cId, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x != User }
        ];

        ResTargetSelectors =
        [
            new OptionSelector() { Message = "INFO_PICKER_SCARLETWALTZ", ForPlayer = (p) => p == Owner, OptionCount = 2 }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (target is null || User is null) return;

        int option = (ResTargetSelectors[0] as OptionSelector)!.SelectedOption;
        short boost = (short)(option == 0 ? 100 : -100);

        User.Boost(boost, this);
        target.Boost(boost, this);
    }
    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Fairy && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(37, CardKind.NormalAbility, (cID, owner) => new ScarletWaltz(cID, owner, 37));
}
