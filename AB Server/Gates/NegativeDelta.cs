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
            if (Bakugans.Any(x => x.BaseAttribute == Attribute.Darkon || x.BaseAttribute == Attribute.Subterra || x.BaseAttribute == Attribute.Zephyros))
                foreach (var bakugan in Bakugans.Where(x => x.BaseAttribute == Attribute.Darkon || x.BaseAttribute == Attribute.Subterra || x.BaseAttribute == Attribute.Zephyros))
                    bakugan.Boost(new Boost(-200), this);
            else
                foreach (var bakugan in Bakugans)
                    bakugan.Boost(new Boost(-200), this);

            game.ChainStep();
        }
    }
}
