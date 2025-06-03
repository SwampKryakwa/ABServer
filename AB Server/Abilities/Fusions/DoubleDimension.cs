namespace AB_Server.Abilities
{
    internal class DoubleDimension : FusionAbility
    {
        public DoubleDimension(int cID, Player owner) : base(cID, owner, 1, typeof(Dimension4))
        {
            CondTargetSelectors =
            [
                new ActiveSelector() { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x is AbilityCard && x.User.IsEnemyOf(User) }
            ];
        }

        public override void TriggerEffect() =>
            new DoubleDimensionEffect(User, (CondTargetSelectors[0] as ActiveSelector).SelectedActive.User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Lucifer && user.InBattle && Game.ActiveZone.Any(x => x is AbilityCard && x.User.OnField() && x.User.IsEnemyOf(user));
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
            this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (target.OnField())
                target.Boost(new Boost((short)-target.Power), this);
        }
    }
}
