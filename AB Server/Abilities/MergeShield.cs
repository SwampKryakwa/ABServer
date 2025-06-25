using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class MergeShield : AbilityCard
    {
        public MergeShield(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsOpponentOf(User) }
            ];
        }

        public override void TriggerEffect() =>
            new BoostEffect(User, User, (short)(ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.AdditionalPower).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Owner.BakuganOwned.Any(x => x.OnField() && x.IsAttribute(Attribute.Darkon)) && Game.BakuganIndex.Any(x => x.IsOpponentOf(user) && x.OnField());
    }
}
