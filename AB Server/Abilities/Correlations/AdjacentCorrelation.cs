namespace AB_Server.Abilities.Correlations
{
    internal class AdjacentCorrelation : AbilityCard
    {
        public AdjacentCorrelation(int cID, Player owner) : base(cID, owner, 0)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField()}
            ];
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect()
        {
            if (CondTargetSelectors[0] is BakuganSelector targetSelector && Bakugan.IsAdjacent(User, targetSelector.SelectedBakugan))
            {
                if (User.IsOpponentOf(targetSelector.SelectedBakugan))
                    new BoostEffect(User, User, 200, TypeId, (int)Kind).Activate();
                else
                    new BoostEffect(User, User, 100, TypeId, (int)Kind).Activate();
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => Bakugan.IsAdjacent(user, x) && (x.OnField() || (x.Owner == user.Owner && x.InHand())));
    }
}