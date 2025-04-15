namespace AB_Server.Abilities.Correlations
{
    internal class AdjacentCorrelation(int cID, Player owner) : AbilityCard(cID, owner, 0)
    {
        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect() =>
            new BoostEffect(User, User, 100, TypeId, (int)Kind).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => Bakugan.IsAdjacent(user, x) && (x.OnField() || (x.Owner == user.Owner && x.InHand())));
    }
}