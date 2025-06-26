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

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            new MoveBakuganEffect(User, target, (User.Position as GateCard)!, TypeId, (int)Kind).Activate();
            target.TurnFrenzied();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Owner.BakuganOwned.Any(x => x != user && x.OnField()) && user.IsAttribute(Attribute.Darkon);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Owner.BakuganOwned.Any(x => x != user && x.OnField());
    }
}
