namespace AB_Server.Gates
{
    internal class Peacemaker : GateCard
    {
        public Peacemaker(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 1;

        public override void DetermineWinner()
        {
            if (IsOpen)
            {
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    b.ToHand(EnterOrder);
                }
                game.BattlesToEnd.Add(this);
            }
            else
                base.DetermineWinner();
        }

        public override void Dispose()
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.JustEndedBattle = false;
                b.ToHand(EnterOrder);
            }

            IsOpen = false;
            OnField = false;
            Owner.GateGrave.Add(this);

            game.Field[Position.X, Position.Y] = null;

            game.ThrowEvent(EventBuilder.RemoveGate(this));
            game.ThrowEvent(EventBuilder.SendGateToGrave(this));
        }
    }
}
