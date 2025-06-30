using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class FireTornado : AbilityCard
    {
        public FireTornado(int cId, Player owner, int typeId) : base(cId, owner, typeId)
        {
            ResTargetSelectors =
            [
                new YesNoSelector { ForPlayer = (p) => p == Owner, Message = "INFO_WANTTARGET", Condition = () => Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x)) && User.Position is GateCard positionGate && positionGate.Owner.TeamId != Owner.TeamId },
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DECREASETARGET", TargetValidator = x => x.Position == User.Position && User.IsOpponentOf(x), Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
            ];
        }

        public override void TriggerEffect()
        {
            new BoostEffect(User, User, 100).Activate();

            if ((ResTargetSelectors[0] as YesNoSelector)!.IsYes && ResTargetSelectors[1] is BakuganSelector targetSelector)
                new BoostEffect(User, targetSelector.SelectedBakugan, -100).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.IsAttribute(Attribute.Nova);

        public static new bool HasValidTargets(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Position.Bakugans.Any(x => user.IsOpponentOf(x) && x.Power > user.Power);
    }
}
