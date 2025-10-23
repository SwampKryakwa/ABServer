using AB_Server.Abilities;

namespace AB_Server.Gates.SpecialGates;

internal class AttributeGate(int power, Attribute attribute, int cID, Player owner) : GateCard(cID, owner)
{
    public override CardKind Kind { get; } = CardKind.SpecialGate;

    public override void TriggerEffect()
    {
        foreach (var bakugan in Game.BakuganIndex.Where(x => x.OnField() && x.IsAttribute(attribute)))
            bakugan.Boost(power, this);
    }
}