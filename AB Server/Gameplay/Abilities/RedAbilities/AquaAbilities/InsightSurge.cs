using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class InsightSurge : AbilityCard
{
    public InsightSurge(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new TypeSelector { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", SelectableKinds = [CardKind.NormalAbility, CardKind.CommandGate, CardKind.SpecialGate] },
            new TypeSelector { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", SelectableKinds = [CardKind.NormalAbility, CardKind.CommandGate, CardKind.SpecialGate] },
            new TypeSelector { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", SelectableKinds = [CardKind.NormalAbility, CardKind.CommandGate, CardKind.SpecialGate] }
        ];
    }

    public override void TriggerEffect()
    {
        var selector1 = (ResTargetSelectors[0] as TypeSelector)!;
        var selector2 = (ResTargetSelectors[1] as TypeSelector)!;
        var selector3 = (ResTargetSelectors[2] as TypeSelector)!;
        (int type, int kind)[] selectedTypes = [(selector1.SelectedType, selector1.SelectedKind), (selector2.SelectedType, selector2.SelectedKind), (selector3.SelectedType, selector3.SelectedKind)];

        User.Boost(100 * (Game.Players.Where(x => x.TeamId != Owner.TeamId).SelectMany(x => x.AbilityHand).Count(x => selectedTypes.Contains((x.TypeId, (int)x.Kind))) + Game.Players.Where(x => x.TeamId != Owner.TeamId).SelectMany(x => x.GateHand).Count(x => selectedTypes.Contains((x.TypeId, (int)x.Kind)))), this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Aqua);

    [ModuleInitializer]
    internal static void Init() => Register(45, CardKind.NormalAbility, (cID, owner) => new InsightSurge(cID, owner, 45));
}
