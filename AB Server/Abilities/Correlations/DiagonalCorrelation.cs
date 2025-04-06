using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Correlations
{
    internal class DiagonalCorrelation : AbilityCard
    {
        public DiagonalCorrelation(int cID, Player owner)
        {
            TypeId = 1;
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

            var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() && x != User);

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, validBakugans)
            ));

            Game.OnAnswer[Owner.Id] = Setup3;
        }

        private Bakugan target;

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Activate();
        }

        public new void Activate()
        {
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
                new DiagonalCorrelationEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new DiagonalCorrelationEffect(User, target, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField();

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.BakuganIndex.Any(x => x.Owner == user.Owner && x.OnField());
    }

    internal class DiagonalCorrelationEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public DiagonalCorrelationEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
             this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
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

            if (IsValidDiagonal())
            {
                User.Boost(new Boost(100), this);
                target.Boost(new Boost(100), this);
            }
        }

        private bool IsValidDiagonal()
        {
            return (User.Attribute == Attribute.Nova && target.Attribute == Attribute.Darkon) ||
                   (User.Attribute == Attribute.Darkon && target.Attribute == Attribute.Nova) ||
                   (User.Attribute == Attribute.Subterra && target.Attribute == Attribute.Aqua) ||
                   (User.Attribute == Attribute.Aqua && target.Attribute == Attribute.Subterra) ||
                   (User.Attribute == Attribute.Zephyros && target.Attribute == Attribute.Lumina) ||
                   (User.Attribute == Attribute.Lumina && target.Attribute == Attribute.Zephyros);
        }
    }
}