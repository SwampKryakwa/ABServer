using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class DoubleDimension : FusionAbility
    {
        public DoubleDimension(int cID, Player owner)
        {
            TypeId = 1;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(Dimension4);
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITYUSER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = PickTarget;
        }

        public void PickTarget()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.ActiveSelection("INFO_ABILITYTARGET", Game.ActiveZone.Where(x => x is AbilityCard && User?.OnField() == true))
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        Bakugan target;
        public new void Activate()
        {
            target = Game.ActiveZone[(int)Game.IncomingSelection[Owner.Id]["array"][0]["active"]].User;

            FusedTo.Discard();
            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new DoubleDimensionEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new DoubleDimensionEffect(User, target, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Lucifer && user.InBattle && user.IsPartner && Game.ActiveZone.Any(x => x is AbilityCard);
    }

    internal class DoubleDimensionEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public DoubleDimensionEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            this.user = user;
            this.target = target;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Treatment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            target.Boost(new Boost((short)-target.Power), this);
        }
    }
}
