using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class SlashZero(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
    {
        public override void TriggerEffect() =>
            User.Boost(new Boost((short)(80 * Game.AbilityIndex.Count(x => x.Kind == CardKind.NormalAbility))), this);

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Shredder && user.InBattle;
    }
}
