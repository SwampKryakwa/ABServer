using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class FusionAbility : AbilityCard
    {
        public static Func<int, Player, FusionAbility>[] FusionCtrs =
        [
            (cID, owner) => new Unleash(cID, owner),
            (cID, owner) => new Unleash(cID, owner),
            (cID, owner) => new Unleash(cID, owner)
        ];
        public override AbilityKind Kind { get; } = AbilityKind.FusionAbility;

        public void Setup(AbilityCard @base, Bakugan user)
        {
            @base.Fusion = this;
            User = user;

            Activate();
        }

        public new virtual void Activate()
        {
            Game.CheckChain(Owner, this, User);
        }

        public override bool IsActivateable() => false;
    }
}
