using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class CommandConvergence : AbilityCard
    {
        public CommandConvergence(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new OptionSelector() { Message = "INFO_PICKER_ATTRIBUTE", ForPlayer = (p) => p == Owner, OptionCount = 6 }
            ];
        }

        public override void TriggerEffect()
        {
            foreach (var target in Game.BakuganIndex.Where(x => x.OnField() && x.IsAttribute((Attribute)(ResTargetSelectors[0] as OptionSelector)!.SelectedOption)))
                target.Boost(150, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Knight && user.OnField();

        [ModuleInitializer]
        internal static void Init() => Register(34, CardKind.NormalAbility, (cID, owner) => new CommandConvergence(cID, owner, 34));
    }
}
