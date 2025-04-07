using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BruteUltimatum : FusionAbility
    {
        public BruteUltimatum(int cID, Player owner) : base(cID, owner, 7)
        {
            BaseAbilityType = typeof(MercilessTriumph);
        }

        Bakugan user;
        Bakugan target;

        public override void PickUser()
        {
            var validUsers = Owner.BakuganOwned.Where(b => b.Type == BakuganType.Glorius && b.OnField() && b.JustEndedBattle && !b.BattleEndedInDraw);
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_BRUTE_ULTIMATUM_USER", TypeId, (int)Kind, validUsers)
            ));
            Game.OnAnswer[Owner.Id] = PickOpponentBakugan;
        }

        public void PickOpponentBakugan()
        {
            user = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            var validTargets = user.Owner.BakuganOwned.Where(b => !b.OnField());
            Game.NewEvents[user.Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_BRUTE_ULTIMATUM_TARGET", TypeId, (int)Kind, validTargets)
            ));
            Game.OnAnswer[user.Owner.Id] = Activate;
        }

        public new void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[user.Owner.Id]["array"][0]["bakugan"]];

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = IsCopy,
                    ["Id"] = EffectId,
                    ["Card"] = TypeId,
                    ["Kind"] = (int)Kind,
                    ["User"] = user.BID,
                    ["IsCounter"] = asCounter,
                    ["Owner"] = Owner.Id
                });
            }

            FusedTo.Discard();
            Game.CheckChain(Owner, this, user);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new BruteUltimatumEffect(user, target, TypeId).Activate();
            Dispose();
        }

        public override void TriggerEffect() =>
            new BruteUltimatumEffect(user, target, TypeId).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.Type == BakuganType.Glorius && user.OnField() && user.JustEndedBattle && !user.BattleEndedInDraw;
    }

    internal class BruteUltimatumEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public BruteUltimatumEffect(Bakugan user, Bakugan target, int typeID)
        {
            this.user = user;
            this.target = target;
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

            target.AddFromHand(user.Position as GateCard);
        }
    }
}
