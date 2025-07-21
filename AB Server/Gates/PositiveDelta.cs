namespace AB_Server.Gates
{
    internal class PositiveDelta : GateCard
    {
        public PositiveDelta(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 6;

        public override void Resolve()
        {
            if (Bakugans.Any(x => x.BaseAttribute == Attribute.Nova || x.BaseAttribute == Attribute.Aqua || x.BaseAttribute == Attribute.Lumina))
                foreach (var bakugan in Bakugans.Where(x => x.BaseAttribute == Attribute.Nova || x.BaseAttribute == Attribute.Aqua || x.BaseAttribute == Attribute.Lumina))
                    bakugan.Boost(new Boost(-200), this);
            else
                foreach (var bakugan in Bakugans)
                    bakugan.Boost(new Boost(-200), this);

            game.ChainStep();
        }
    }
}
