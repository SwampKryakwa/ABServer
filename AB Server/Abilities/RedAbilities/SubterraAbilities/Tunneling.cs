using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Tunneling : AbilityCard
{
    /*
     * REQUIREMENT: Choose your SUBTERRA bakugan on the field to use. 
     * EFFECT: Target 1 gate card adjacent to the one user is on. Swaps it with user gate card. Bakugan remain in the same field sectors. 
     */
    public Tunneling(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATION", TargetValidator = g => User.Position is GateCard posGate && g.IsAdjacent(posGate) }
        ];
    }

    public override void TriggerEffect()
    {
        if (CondTargetSelectors[0] is GateSelector gateSelector && User.Position is GateCard posGate)
        {
            GateCard targetGate = gateSelector.SelectedGate;
            GateCard userGate = posGate;
            // Swap the gates' positions
            var tempPosition = targetGate.Position;
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Subterra);

    [ModuleInitializer]
    internal static void Init() => Register(56, CardKind.NormalAbility, (cID, owner) => new Tunneling(cID, owner, 56));
}
