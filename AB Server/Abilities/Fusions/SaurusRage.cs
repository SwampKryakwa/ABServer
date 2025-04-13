namespace AB_Server.Abilities.Fusions
{
    internal class SaurusRage : FusionAbility
    {
        public SaurusRage(int cID, Player owner) : base(cID, owner, 4, typeof(SaurusGlow))
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Power > User.Power }
            ];
        }

        public override void TriggerEffect() =>
            new SaurusRageEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(x => x.OnField() && x.Power > user.Power);
    }

    internal class SaurusRageEffect
    {
        public int TypeId { get; }
        public Bakugan user;
        public Bakugan target;
        Game game { get => user.Game; }


        public Player Owner { get => user.Owner; }

        public CardKind Kind { get; } = CardKind.FusionAbility;

        bool IsCopy;

        public SaurusRageEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            this.user = user;
            this.target = target;
            this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));
            }

            user.Boost(new Boost((short)Math.Abs((user.Power - target.Power) * 2)), this);
        }
    }
}
