using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Correlations
{
    internal class ElementalFlash(int cID, Player owner) : AbilityCard(cID, owner, 3)
    {
        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect() =>
            User.Boost(50, this);

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Owner.BakuganOwned.Select(x => x.BaseAttribute).Distinct().Count() == 1;

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(3, CardKind.CorrelationAbility, (cID, owner) => new ElementalFlash(cID, owner));
    }
}
