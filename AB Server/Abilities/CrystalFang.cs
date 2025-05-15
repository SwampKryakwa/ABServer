namespace AB_Server.Abilities
{
    internal class CrystalFang : AbilityCard
    {
        public CrystalFang(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.IsEnemyOf(User) && target.OnField()}
            ];
        }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.AnyBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
            new CrystalFangEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            (user.OnField() || user.InHand()) && user.Type == BakuganType.Tigress && Game.CurrentWindow == ActivationWindow.BattleStart;
    }

    internal class CrystalFangEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public CrystalFangEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            target.Boost(new Boost(-100), this);
        }
    }
}


