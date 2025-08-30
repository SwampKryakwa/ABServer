using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class SlashZero(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
    {
        public override void TriggerEffect() =>
            User.Boost(new Boost((short)(80 * Game.AbilityIndex.Count(x => x.Owner == Owner))), this);

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Shredder && user.InBattle;

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(36, CardKind.NormalAbility, (cID, owner) => new SlashZero(cID, owner, 36));
    }
}
