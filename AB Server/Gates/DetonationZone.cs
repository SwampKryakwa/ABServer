namespace AB_Server.Gates
{
    internal class DetonationZone : GateCard
    {
        public DetonationZone(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 23;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !IsOpen && !Negated && Bakugans.Any(x => x.Owner == Owner && x.InBattle);

        public override void TriggerEffect()
        {
            foreach (var b in Bakugans)
                b.Boost(-200, this);
        }
    }
}
