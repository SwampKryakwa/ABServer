using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class FusionAbility : AbilityCard
    {
        public void Setup(AbilityCard @base, Bakugan user)
        {
            @base.Fusion = this;
            User = user;

            Activate();
        }

        public new void Activate()
        {
            Game.CheckChain(Owner, this, User);
        }
    }
}
