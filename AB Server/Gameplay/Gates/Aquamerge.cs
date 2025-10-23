namespace AB_Server.Gates;

internal class Aquamerge(int cID, Player owner) : GateCard(cID, owner)
{
    public override int TypeId { get; } = 7;

    public override void Resolve()
    {
        foreach (var bakugan in Game.BakuganIndex.Where(x => x.OnField() && !x.IsAttribute(Attribute.Subterra)))
            bakugan.ChangeAttribute(Attribute.Aqua, this);

        Game.ChainStep();
    }
}
