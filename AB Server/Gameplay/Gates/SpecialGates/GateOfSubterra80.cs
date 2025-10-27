namespace AB_Server.Gates.SpecialGates;

internal class GateOfSubterra80(int cID, Player owner) : AttributeGate(80, Attribute.Subterra, cID, owner)
{
    public override int TypeId { get; } = 1;
}