using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Groundshift : AbilityCard
{
    public Groundshift(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATION", TargetValidator = g => User.Position is GateCard userPosGate && g.Owner == userPosGate.Owner }
        ];
    }

    public override void TriggerEffect()
    {
        if (CondTargetSelectors[0] is GateSelector gateSelector)
            User.MoveOnField(gateSelector.SelectedGate, new() { ["MoveEffect"] = "Submerge" });
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Subterra);

    [ModuleInitializer]
    internal static void Init() =>
        Register(15, CardKind.NormalAbility, (cID, owner) => new Groundshift(cID, owner, 15));
}
