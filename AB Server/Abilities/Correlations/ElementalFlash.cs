using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Correlations;

internal class ElementalFlash(int cID, Player owner) : AbilityCard(cID, owner, 3)
{
    public override CardKind Kind { get; } = CardKind.CorrelationAbility;

    public override void TriggerEffect() =>
        User.Boost(50, this);

    public override bool IsActivateable() =>
        Owner.AbilityBlockers.Count == 0 && Owner.BlueAbilityBlockers.Count == 0 && Owner.BakuganOwned.Any(IsActivateableByBakugan);

    public override bool UserValidator(Bakugan user) => user.OnField();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Normal && (Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Aqua)) || Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Nova)) || Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Subterra)) || Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Lumina)) || Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Darkon)) || Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Zephyros)));

    [ModuleInitializer]
    internal static void Init() => Register(3, CardKind.CorrelationAbility, (cID, owner) => new ElementalFlash(cID, owner));
}
