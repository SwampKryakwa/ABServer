using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class JumpOver : AbilityCard
    {
        public JumpOver(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacentVertically(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect()
        {
            new MoveBakuganEffect(User, User, (CondTargetSelectors[0] as GateSelector).SelectedGate, TypeId, (int)Kind).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Position is GateCard positionGate && user.IsAttribute(Attribute.Zephyros) && Game.GateIndex.Any(positionGate.IsAdjacentVertically) && Game.CurrentWindow == ActivationWindow.Normal;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(x => x.IsAdjacentVertically(user.Position as GateCard));
    }
}
