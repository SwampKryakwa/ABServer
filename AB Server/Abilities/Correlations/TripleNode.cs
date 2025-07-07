namespace AB_Server.Abilities.Correlations
{
    internal class TripleNode : AbilityCard
    {
        public TripleNode(int cID, Player owner) : base(cID, owner, 2)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField() && target.Owner == owner},
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField() && target.Owner == owner && target != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
            ];
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect()
        {
            bool isPositive;
            if (isFusion && CondTargetSelectors[0] is BakuganSelector targetSelector1 && CondTargetSelectors[1] is BakuganSelector targetSelector2 && Bakugan.IsTripleNode(out isPositive, targetSelector1.SelectedBakugan, targetSelector2.SelectedBakugan, User))
            {
                User.Boost(200, this);
                targetSelector1.SelectedBakugan.Boost(200, this);
                targetSelector2.SelectedBakugan.Boost(200, this);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField();

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.BakuganIndex.Any(x => x.Owner == user.Owner && x.OnField());
    }
}