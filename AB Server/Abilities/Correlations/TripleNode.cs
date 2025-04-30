namespace AB_Server.Abilities.Correlations
{
    internal class TripleNode : AbilityCard
    {
        public TripleNode(int cID, Player owner) : base(cID, owner, 2)
        {
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

        private List<Bakugan> otherBakugans = new();

        public void Setup3()
        {
            var selectedBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            otherBakugans.Add(selectedBakugan);

            var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() && x != User && !otherBakugans.Contains(x));

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
                var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() && x != User && !otherBakugans.Contains(x));

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, validBakugans)
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

            Game.ThrowEvent(new()
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

            Game.CheckChain(Owner, this, User);
        }

        public override void TriggerEffect() =>
            new TripleNodeEffect(User, otherBakugans, TypeId, IsCopy).Activate();

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
            user.OnField() && user.Game.BakuganIndex.Any(x => x.Owner == user.Owner && x.OnField());
    }

    internal class TripleNodeEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        List<Bakugan> otherBakugans;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public TripleNodeEffect(Bakugan user, List<Bakugan> otherBakugans, int typeID, bool IsCopy)
        {
            User = user;
            this.otherBakugans = otherBakugans;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 2, User));

            if (Bakugan.IsTripleNode([User, .. otherBakugans]))
            {
                User.Boost(new Boost(200), this);
                foreach (var bakugan in otherBakugans)
                {
                    bakugan.Boost(new Boost(200), this);
                }
            }
        }
    }
}