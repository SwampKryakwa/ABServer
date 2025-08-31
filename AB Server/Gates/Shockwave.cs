namespace AB_Server.Gates
{
    internal class Shockwave : GateCard
    {
        public Shockwave(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 15;

        public override void Resolve()
        {
            if (!Negated)
            {
                foreach (var gate in game.GateIndex.Where(x => x.OnField))
                    gate.Bakugans.ForEach(x => x.Boost(new Boost(-100), this));
                foreach (var gate in game.GateIndex.Where(x => x.OnField && x.IsAdjacent(this)))
                    gate.Bakugans.ForEach(x => x.Boost(new Boost(-100), this));
            }
            game.ChainStep();
        }
    }
}
