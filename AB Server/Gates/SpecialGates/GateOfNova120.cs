namespace AB_Server.Gates.SpecialGates;

internal class GateOfNova120(int cID, Player owner) : AttributeGate(120, Attribute.Darkon, cID, owner)
{
    public override int TypeId { get; } = 3;
}