using AB_Server.Abilities;

namespace AB_Server.Gates.SpecialGates;

internal class GateOfNova120(int cID, Player owner) : AttributeGate(120, Attribute.Darkon, cID, owner)
{
    
    public override int TypeId { get; } = 3;
    public override CardKind Kind { get; } = CardKind.SpecialGate;

    public override void Open()
    {
        IsOpen = true;
        Game.ActiveZone.Add(this);
        Game.CardChain.Push(this);
        EffectId = Game.NextEffectId++;
        Game.ThrowEvent(EventBuilder.GateOpen(this));


        Game.CheckChain(Owner, this);
    }

    public override void Resolve()
    {
        foreach (var bakugan in Bakugans.Where(x => x.IsAttribute(Attribute.Nova)))
        {
            bakugan.Boost(new Boost(120), this);
        }

        Game.ChainStep();
    }
}