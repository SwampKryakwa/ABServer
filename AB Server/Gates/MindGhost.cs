using AB_Server.Abilities;

namespace AB_Server.Gates
{
    internal class MindGhost : GateCard
    {
        public MindGhost(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 18;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && !Owner.AbilityDrop.Any(card => card is AbilityCard) && OpenBlocking.Count == 0 && !IsOpen && !Negated;

        public override void Resolve()
        {
            if (!Negated)
                new List<Bakugan>(Bakugans).ForEach(x => x.MoveFromFieldToDrop(EnterOrder));

            game.ChainStep();
        }
    }
}
