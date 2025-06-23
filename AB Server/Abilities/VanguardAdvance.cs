using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class VanguardAdvance : AbilityCard
    {
        public VanguardAdvance(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacent(User.Position as GateCard) && x.Bakugans.Any(User.IsEnemyOf) }
            ];
        }

        public override void TriggerEffect()
        {
            new MoveBakuganEffect(User, User, (CondTargetSelectors[0] as GateSelector).SelectedGate, TypeId, (int)Kind).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Position is GateCard positionGate && user.IsAttribute(Attribute.Nova) && Game.GateIndex.Any(positionGate.IsAdjacent) && Game.CurrentWindow == ActivationWindow.BattleEnd;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(x => x.IsAdjacent(user.Position as GateCard));
    }
}
