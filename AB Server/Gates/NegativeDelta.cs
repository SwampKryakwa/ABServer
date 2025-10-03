namespace AB_Server.Gates;

internal class NegativeDelta(int cID, Player owner) : GateCard(cID, owner)
{
    
    public override int TypeId { get; } = 13;

    public override void Resolve()
    {
        if (Bakugans.Any(x => x.IsAttribute(Attribute.Darkon) || x.IsAttribute(Attribute.Subterra) || x.IsAttribute(Attribute.Zephyros)))
            foreach (var bakugan in Bakugans.Where(x => x.IsAttribute(Attribute.Darkon) || x.IsAttribute(Attribute.Subterra) || x.IsAttribute(Attribute.Zephyros)))
                bakugan.Boost(new Boost(-200), this);
        else
            foreach (var bakugan in Bakugans)
                bakugan.Boost(new Boost(-200), this);

        Game.ChainStep();
    }
}
