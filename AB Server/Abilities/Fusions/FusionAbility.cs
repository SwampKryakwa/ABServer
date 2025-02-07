using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public Type BaseAbilityType;
        public AbilityCard FusedTo;

        public void Setup(AbilityCard @base, Bakugan user)
        {
            @base.Fusion = this;
            User = user;

            Game.NewEvents[Owner.Id].Add(EventBuilder.AbilitySelection("INFO_FUSIONBASE", Owner.AbilityHand.Where(x => BaseAbilityType.IsInstanceOfType(x) && x.IsActivateable()).ToArray()));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new virtual void Activate()
        {
            FusedTo = (AbilityCard)Game.IncomingSelection[Owner.Id]["array"][0]["ability"];

            Game.CheckChain(Owner, this, User);
        }
    }
}
