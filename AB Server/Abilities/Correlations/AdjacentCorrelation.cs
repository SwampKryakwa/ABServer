using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Correlations
{
    internal class AdjacentCorrelation : AbilityCard
    {
        public AdjacentCorrelation(int cID, Player owner) : base(cID, owner, 0)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField() && target != User }
            ];
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect()
        {
            if (CondTargetSelectors[0] is BakuganSelector targetSelector && Bakugan.IsAdjacent(User, targetSelector.SelectedBakugan))
            {
                if (User.IsOpponentOf(targetSelector.SelectedBakugan))
                    User.Boost(200, this);
                else
                    User.Boost(100, this);
            }
        }
        public override bool BakuganIsValid(Bakugan user) =>
            Owner.AbilityBlockers.Count == 0 && Owner.BlueAbilityBlockers.Count == 0 && !user.Frenzied && IsActivateableByBakugan(user) && user.Owner == Owner;

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && Game.BakuganIndex.Count(x=>x.OnField()) >= 2;

        [ModuleInitializer]
        internal static void Init() => Register(0, CardKind.CorrelationAbility, (cID, owner) => new AdjacentCorrelation(cID, owner));
    }
}