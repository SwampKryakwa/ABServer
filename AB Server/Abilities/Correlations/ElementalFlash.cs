using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Correlations
{
    internal class ElementalFlash(int cID, Player owner) : AbilityCard(cID, owner, 3)
    {
        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect() =>
            User.Boost(50, this);
        public override bool BakuganIsValid(Bakugan user) =>
            Owner.AbilityBlockers.Count == 0 && Owner.BlueAbilityBlockers.Count == 0 && !user.Frenzied && IsActivateableByBakugan(user) && user.Owner == Owner;

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && (user.Owner.BakuganOwned.All(x=>x.IsAttribute(Attribute.Aqua)) || user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Nova)) || user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Subterra)) || user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Lumina)) || user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Darkon)) || user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Zephyros)));

        [ModuleInitializer]
        internal static void Init() => Register(3, CardKind.CorrelationAbility, (cID, owner) => new ElementalFlash(cID, owner));
    }
}
