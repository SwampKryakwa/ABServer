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

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

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
