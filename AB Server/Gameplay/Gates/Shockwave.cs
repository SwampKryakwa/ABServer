namespace AB_Server.Gates;

internal class Shockwave(int cID, Player owner) : GateCard(cID, owner)
{

    public override int TypeId { get; } = 15;

    public override void Resolve()
    {
        if (!Negated)
        {
            foreach (var gate in Game.GateIndex.Where(x => x.OnField && x != this))
                gate.Bakugans.ForEach(x => x.Boost(new Boost(-100), this));
            foreach (var gate in Game.GateIndex.Where(x => x.OnField && x.IsAdjacent(this)))
                gate.Bakugans.ForEach(x => x.Boost(new Boost(-100), this));
        }
        Game.ChainStep();
    }
}
