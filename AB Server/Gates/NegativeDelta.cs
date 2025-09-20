namespace AB_Server.Gates
{
    internal class NegativeDelta : GateCard
    {
        public NegativeDelta(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 13;

        public override void Resolve()
        {
            if (Bakugans.Any(x => x.IsAttribute(Attribute.Nova) || x.IsAttribute(Attribute.Aqua) || x.IsAttribute(Attribute.Lumina)))
                foreach (var bakugan in Bakugans.Where(x => x.IsAttribute(Attribute.Nova) || x.IsAttribute(Attribute.Aqua) || x.IsAttribute(Attribute.Lumina)))
                    bakugan.Boost(new Boost(-200), this);
            else
                foreach (var bakugan in Bakugans)
                    bakugan.Boost(new Boost(-200), this);

            game.ChainStep();
        }
    }
}
