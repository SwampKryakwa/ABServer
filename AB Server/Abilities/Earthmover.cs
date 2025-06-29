using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class Earthmover : AbilityCard
    {
        public Earthmover(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => !x.Bakugans.Any(x=>x.Owner != Owner) }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as GateSelector)!.SelectedGate;

            new List<Bakugan>(target.Bakugans).ForEach(x => x.ToHand(target.EnterOrder));

            target.ToDrop();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Worm && user.OnField() && Game.GateIndex.Any(x => !x.Bakugans.Any(x => x.Owner != Owner));
    }
}
