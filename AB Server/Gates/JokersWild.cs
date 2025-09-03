namespace AB_Server.Gates
{
    internal class JokersWild : GateCard
    {
        public JokersWild(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 5;

        public override bool IsOpenable() =>
            base.IsOpenable() && Bakugans.Any(x => x.Owner == Owner && x.Power < 0 && x.IsAttribute(Attribute.Darkon));

        public override void TriggerEffect()
        {
            foreach (var bakugan in Bakugans.Where(x => !x.IsAttribute(Attribute.Darkon)).ToArray())
            {
                bakugan.MoveFromFieldToDrop(EnterOrder);
            }
        }
    }
}
