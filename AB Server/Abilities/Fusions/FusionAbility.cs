using Newtonsoft.Json.Linq;
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
            (cID, owner) => new Unleash(cID, owner),
            (cID, owner) => new Unleash(cID, owner),
            (cID, owner) => new DoubleDimension(cID, owner),
            (cID, owner) => new Unleash(cID, owner),
            (cID, owner) => new Unleash(cID, owner),
            (cID, owner) => new Unleash(cID, owner)
        ];
        public override AbilityKind Kind { get; } = AbilityKind.FusionAbility;
        public Type BaseAbilityType;
        public AbilityCard FusedTo;

        public override void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.AbilitySelection("INFO_FUSIONBASE", Owner.AbilityHand.Where(x => BaseAbilityType.IsInstanceOfType(x)).ToArray())
                ));
            Game.AwaitingAnswers[Owner.Id] = PickUser;
        }

        public virtual void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITYUSER", TypeId, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            FusedTo.Dispose();
            Game.CheckChain(Owner, this, User);
        }

        public override bool IsActivateable() =>
            Owner.BakuganOwned.Any(IsActivateableByBakugan) && Owner.AbilityHand.Any(BaseAbilityType.IsInstanceOfType);
    }
}
