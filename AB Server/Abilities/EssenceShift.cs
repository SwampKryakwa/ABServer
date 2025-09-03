using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class EssenceShift : AbilityCard
    {
        public EssenceShift(int cId, Player owner, int typeId) : base(cId, owner, typeId)
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", Message = "INFO_ABILITY_TARGET", ForPlayer = p => p == Owner, TargetValidator = x => x.OnField() },
                new OptionSelector() { Message = "INFO_PICKER_ATTRIBUTE", ForPlayer = (p) => p == Owner, OptionCount = 6 }
            ];
        }

        public override void TriggerEffect()
        {
            (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.ChangeAttribute((Attribute)(ResTargetSelectors[1] as OptionSelector)!.SelectedOption, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Aqua) && user.OnField();

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(31, CardKind.NormalAbility, (cID, owner) => new EssenceShift(cID, owner, 31));
    }
}
