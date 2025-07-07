namespace AB_Server.Gates
{
    internal class Aquamerge : GateCard
    {
        public Aquamerge(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 7;

        public override void Resolve()
        {
            foreach (var bakugan in game.BakuganIndex.Where(x => x.OnField() && !x.IsAttribute(Attribute.Subterra)))
                bakugan.ChangeAttribute(Attribute.Aqua, this);

            game.ChainStep();
        }
    }
}
