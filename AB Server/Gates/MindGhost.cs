using AB_Server.Abilities;

namespace AB_Server.Gates;

internal class MindGhost(int cID, Player owner) : GateCard(cID, owner)
{
    
    public override int TypeId { get; } = 18;

    public override bool IsOpenable() =>
        Game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && !Owner.AbilityDrop.Any(card => card is AbilityCard) && OpenBlocking.Count == 0 && !IsOpen && !Negated;

    public override void Resolve()
    {
        if (!Negated)
            new List<Bakugan>(Bakugans).ForEach(x => x.MoveFromFieldToDrop(EnterOrder));

        Game.ChainStep();
    }
}
