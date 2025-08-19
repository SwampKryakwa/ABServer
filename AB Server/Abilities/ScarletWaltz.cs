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
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

            if (target.Owner.TeamId != Owner.TeamId) target.Boost(new Boost(-100), this);
            else
            {
                target.Boost(new Boost(50), this);
                User.Boost(new Boost(50), this);
            }
        }
        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Fairy && user.OnField() && Game.BakuganIndex.Any(x => x.OnField() && x != user);

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(37, CardKind.NormalAbility, (cID, owner) => new ScarletWaltz(cID, owner, 37));
    }
}
