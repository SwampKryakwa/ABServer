using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities.Fusions
{
    internal class GrandDevourer : FusionAbility
    {
        public GrandDevourer(int cID, Player owner) : base(cID, owner, 12, typeof(Earthmover))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTROYTARGET", TargetValidator = x => x.OnField() && x.Power < 0 }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (target.Position is GateCard positionGate)
                target.DestroyOnField(positionGate.EnterOrder);
            else if (target.Position is Player)
                target.DestroyInHand();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Worm && user.OnField();
    }
}
