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
                    b.MoveFromFieldToHand(EnterOrder);
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
                b.MoveFromFieldToHand(EnterOrder);
            }

            IsOpen = false;
            OnField = false;
            Owner.GateDrop.Add(this);

            game.Field[Position.X, Position.Y] = null;

            game.ThrowEvent(EventBuilder.RemoveGate(this));
            game.ThrowEvent(EventBuilder.SendGateToDrop(this));
        }
    }
}
