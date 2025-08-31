using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions
{
    internal class PowerAccord : FusionAbility
    {
        public PowerAccord(int cID, Player owner) : base(cID, owner, 11, typeof(CommandConvergence))
        {
            ResTargetSelectors =
            [
                new OptionSelector() { Message = "INFO_PICKER_ATTRIBUTE", ForPlayer = (p) => p == Owner, OptionCount = 6 }
            ];
        }

        public override void TriggerEffect()
        {
            User.ChangeAttribute((Attribute)(ResTargetSelectors[0] as OptionSelector)!.SelectedOption, this);
            User.Boost(new Boost((short)(80 * Game.BakuganIndex.Count(x => x.OnField() && x.IsAttribute((Attribute)(ResTargetSelectors[0] as OptionSelector)!.SelectedOption)))), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Knight && user.OnField();

        [ModuleInitializer]
        internal static void Init() => FusionAbility.Register(10, (cID, owner) => new PowerAccord(cID, owner));
    }
}
