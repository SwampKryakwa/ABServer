using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class StrikeBack : FusionAbility
    {
        public StrikeBack(int cID, Player owner)
        {
            TypeId = 2;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(DefiantCounterattack);
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_ABILITYUSER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.AwaitingAnswers[Owner.Id] = PickTarget;
        }

        public void PickTarget()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITYTARGET", TypeId, (int)Kind, Game.BakuganIndex.Where(x => x.OnField() && x.IsEnemyOf(User)))
                ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        Bakugan target;
        public new void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            FusedTo.Discard();
            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new StrikeBackEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }
        public override void DoubleEffect() =>
                new StrikeBackEffect(User, target, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.InGrave() && Game.BakuganIndex.Any(x => x.OnField() && x.IsEnemyOf(user));
    }

    internal class StrikeBackEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public StrikeBackEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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

            user.FromGrave((target.Position as GateCard));
            user.Boost(new Boost((short)(target.Power - user.Power + 10)), this);
        }
    }
}
