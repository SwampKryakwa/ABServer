using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class Slipstream(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect()
        {
            Owner.RemainingThrows++;
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Zephyros);

        [ModuleInitializer]
        internal static void Init() => Register(41, CardKind.NormalAbility, (cID, owner) => new Slipstream(cID, owner, 41));
    }
}
