namespace AB_Server.Abilities.Fusions
{
    internal class SaurusRage : FusionAbility
    {
        public SaurusRage(int cID, Player owner) : base(cID, owner, 4, typeof(SaurusGlow))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Power > User.Power }
            ];
        }

        public override void TriggerEffect() =>
            new SaurusRageEffect(User, (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(x => x.OnField() && x.Power > user.Power);
    }

    internal class SaurusRageEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan user = user;
        public Bakugan target = target;
        Game game { get => user.Game; }


        public Player Owner { get => user.Owner; }

        public CardKind Kind { get; } = CardKind.FusionAbility;

        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            user.Boost(new Boost((short)Math.Abs((user.Power - target.Power) * 2)), this);
        }
    }
}
