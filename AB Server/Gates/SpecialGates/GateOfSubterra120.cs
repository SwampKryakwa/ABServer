namespace AB_Server.Gates.SpecialGates;

internal class GateOfSubterra120(int cID, Player owner) : AttributeGate(120, Attribute.Subterra, cID, owner)
{
    public override int TypeId { get; } = 2;
}