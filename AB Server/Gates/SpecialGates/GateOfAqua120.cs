namespace AB_Server.Gates.SpecialGates;

internal class GateOfAqua120(int cID, Player owner) : AttributeGate(120, Attribute.Aqua, cID, owner)
{
    public override int TypeId { get; } = 4;
}