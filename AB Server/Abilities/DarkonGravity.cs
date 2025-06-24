using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class DarkonGravity : AbilityCard
    {
        public DarkonGravity(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.Owner == Owner && x.OnField() }
            ];
        }

        public override void TriggerEffect() =>
            new MoveBakuganEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, (User.Position as GateCard)!, TypeId, (int)Kind).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.Owner.BakuganOwned.All(x => x.OnField()) && user.IsAttribute(Attribute.Darkon);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Owner.BakuganOwned.All(x => x.OnField());
    }
}
