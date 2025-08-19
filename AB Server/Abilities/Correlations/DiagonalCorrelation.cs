using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Correlations
{
    internal class DiagonalCorrelation : AbilityCard
    {
        public DiagonalCorrelation(int cID, Player owner) : base(cID, owner, 1)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField()}
            ];
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect()
        {
            if (CondTargetSelectors[0] is BakuganSelector targetSelector && Bakugan.IsDiagonal(targetSelector.SelectedBakugan, User))
            {
                User.Boost(100, this);
               targetSelector.SelectedBakugan.Boost(100, this);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => Bakugan.IsDiagonal(x, user) && x.Owner == user.Owner && x.OnField());

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(1, CardKind.CorrelationAbility, (cID, owner) => new DiagonalCorrelation(cID, owner));
    }
}