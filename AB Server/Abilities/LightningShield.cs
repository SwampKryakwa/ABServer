using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class LightningShield : AbilityCard
    {
        public LightningShield(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position && x.InBattle && x.IsOpponentOf(User)}
            ];
        }

        public override void TriggerEffect()
        {
            {
                if (CondTargetSelectors[0] is BakuganSelector targetSelector && User.IsAttribute(Attribute.Lumina))
                    new BoostEffect(User, targetSelector.SelectedBakugan, -100, TypeId, (int)Kind).Activate();
            }
            {
                if (CondTargetSelectors[0] is BakuganSelector targetSelector && targetSelector.SelectedBakugan.Power > User.Power)
                    new BoostEffect(User, targetSelector.SelectedBakugan, -100, TypeId, (int)Kind).Activate();
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.IsAttribute(Attribute.Lumina) && user.Position.Bakugans.Any(user.IsOpponentOf);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(user.IsOpponentOf);
    }
}
