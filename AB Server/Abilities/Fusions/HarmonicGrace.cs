using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities.Fusions
{
    internal class HarmonicGrace : FusionAbility
    {
        public HarmonicGrace(int cID, Player owner) : base(cID, owner, 14, typeof(ScarletWaltz))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.InBattle && x.Owner != Owner}
            ];

            ResTargetSelectors =
            [
                new OptionSelector() { Condition = () => User.IsAttribute(Attribute.Nova), Message = "INFO_PICKER_HARMONICGRACE", ForPlayer = (p) => p == Owner, OptionCount = 2, SelectedOption = 1}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            switch ((ResTargetSelectors[0] as OptionSelector)!.SelectedOption)
            {
                case 0:
                    target.Boost(new Boost((short)User.Power), this);
                    break;
                case 1:
                    User.Boost(new Boost((short)target.Power), this);
                    break;
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Fairy && user.OnField() && Game.BakuganIndex.Any(x => x.OnField() && x != user);
    }
}
