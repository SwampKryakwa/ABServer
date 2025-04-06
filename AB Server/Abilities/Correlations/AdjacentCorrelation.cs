using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Correlations
{
    internal class AdjacentCorrelation : AbilityCard
    {
        public AdjacentCorrelation(int cID, Player owner)
        {
            TypeId = 0;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.OnAnswer[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() || x.InHand() && x != User);

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.AnyBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, validBakugans)
            ));

            Game.OnAnswer[Owner.Id] = Setup3;
        }

        private List<Bakugan> otherBakugans = new();

        public void Setup3()
        {
            var selectedBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            otherBakugans.Add(selectedBakugan);

            var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() || x.InHand() && x != User && !otherBakugans.Contains(x));

            if (validBakugans.Any())
            {
                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.BoolSelectionEvent("INFO_SELECTMORE")
                ));

                Game.OnAnswer[Owner.Id] = HandleAnotherBakuganSelection;
            }
            else
            {
                Activate();
            }
        }

        public void HandleAnotherBakuganSelection()
        {
            if ((bool)Game.IncomingSelection[Owner.Id]["array"][0]["answer"])
            {
                var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() || x.InHand() && x != User && !otherBakugans.Contains(x));

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.AnyBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, validBakugans)
                ));

                Game.OnAnswer[Owner.Id] = Setup3;
            }
            else
            {
                Activate();
            }
        }

        private Bakugan target;

        public new void Activate()
        {
            target = User;

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = IsCopy,
                    ["Id"] = EffectId,
                    ["Card"] = TypeId,
                    ["Kind"] = (int)Kind,
                    ["User"] = User.BID,
                    ["IsCounter"] = asCounter,
                    ["Owner"] = Owner.Id
                });
            }

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new AdjacentCorrelationEffect(User, otherBakugans, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new AdjacentCorrelationEffect(User, otherBakugans, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
            if (otherBakugans.Contains(bakugan))
                otherBakugans.Remove(bakugan);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField();

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.BakuganIndex.Any(x => x.Owner == user.Owner && (x.OnField() || x.InHand()));
    }

    internal class AdjacentCorrelationEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        List<Bakugan> otherBakugans;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public AdjacentCorrelationEffect(Bakugan user, List<Bakugan> otherBakugans, int typeID, bool IsCopy)
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 2 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User = user;
            this.otherBakugans = otherBakugans;
             this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            if (IsValidAdjacent())
            {
                User.Boost(new Boost((short)(100 * otherBakugans.Count)), this);
            }
        }

        private bool IsValidAdjacent()
        {
            var adjacentAttributes = new Dictionary<Attribute, Attribute>
            {
                { Attribute.Nova, Attribute.Subterra },
                { Attribute.Subterra, Attribute.Lumina },
                { Attribute.Lumina, Attribute.Darkon },
                { Attribute.Darkon, Attribute.Aqua },
                { Attribute.Aqua, Attribute.Zephyros },
                { Attribute.Zephyros, Attribute.Nova }
            };

            return otherBakugans.All(b => adjacentAttributes[User.Attribute] == b.Attribute || adjacentAttributes[b.Attribute] == User.Attribute);
        }
    }
}