using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class ScarletWaltz : AbilityCard
    {
        public ScarletWaltz(int cId, Player owner, int typeId) : base(cId, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x != User },
                new OptionSelector() { Message = "INFO_PICKER_SCARLETWALTZ", ForPlayer = (p) => p == Owner, OptionCount = 2 }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (target is null || User is null) return;

            int option = (ResTargetSelectors[1] as OptionSelector)!.SelectedOption;
            short boost = (short)(option == 0 ? 100 : -100);

            User.Boost(boost, this);
            target.Boost(boost, this);
        }
        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Fairy && user.OnField();

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(37, CardKind.NormalAbility, (cID, owner) => new ScarletWaltz(cID, owner, 37));
    }
}
