namespace AB_Server.Abilities
{
    internal class LightningTornado : AbilityCard
    {
        public LightningTornado(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        { }

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
            User = Game.BakuganIndex[(int)Game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = User.Position.Bakugans.Where(x => x.Position == User.Position && User.IsEnemyOf(x) && x.BasePower > User.BasePower);
            if (validBakugans.Count() != 0)
            {
                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                ));
            }
            else
            {
                Activate();
            }
            Game.OnAnswer[Owner.Id] = HandleOpponentBakuganSelection;
        }

        public void HandleOpponentBakuganSelection()
        {
            if ((bool)Game.PlayerAnswers[Owner.Id]["array"][0]["answer"])
            {
                var validBakugans = Game.BakuganIndex.Where(x => x.Position == User.Position && User.IsEnemyOf(x) && x.BasePower > User.BasePower);

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_ABILITY_DECREASETARGET", TypeId, (int)Kind, validBakugans)
                ));

                Game.OnAnswer[Owner.Id] = Setup3;
            }
            else
            {
                Activate();
            }
        }

        private Bakugan target;

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];
            Activate();
        }

        public override void TriggerEffect() =>
            new LightningTornadoEffect(User, target, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.IsAttribute(Attribute.Lumina) && user.InBattle;
    }

    internal class LightningTornadoEffect(Bakugan user, Bakugan target, int typeID, bool isCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan target = target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = isCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            // Increase the power of the user Bakugan by 100G
            User.Boost(new Boost(100), this);

            // If a target Bakugan is selected, decrease its power by 100G
            target?.Boost(new Boost(-100), this);
        }
    }
}
