namespace AB_Server.Gates;

internal class Peacemaker(int cID, Player owner) : GateCard(cID, owner)
{
    public override int TypeId { get; } = 1;

    public override void DetermineWinner()
    {
        if (IsOpen)
        {
            bakugansDefeatedThisBattle.Clear();
            BattleOver = true;
            BattleStarted = false;

            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.JustEndedBattle = false;
                b.MoveFromFieldToHand(EnterOrder);
            }
            Game.BattlesToEnd.Add(this);
        }
        else
            base.DetermineWinner();
    }

    public override void Dispose()
    {
        if (IsOpen)
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.JustEndedBattle = false;
                b.MoveFromFieldToHand(EnterOrder);
            }

            IsOpen = false;
            OnField = false;
            Owner.GateDrop.Add(this);

            Game.Field[Position.X, Position.Y] = null;

            Game.ThrowEvent(EventBuilder.RemoveGate(this));
            Game.ThrowEvent(EventBuilder.SendGateToDrop(this));
        }
        else
            base.DetermineWinner();
    }
}
