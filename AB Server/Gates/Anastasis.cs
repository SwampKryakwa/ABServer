namespace AB_Server.Gates;

internal class Anastasis : GateCard
{
    public Anastasis(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;

        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BG", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.InDrop() }
        ];
    }

    public override int TypeId { get; } = 19;

    public override bool IsOpenable() =>
        Game.CurrentWindow == ActivationWindow.Intermediate && BattleOver && OpenBlocking.Count == 0 && !IsOpen && !Negated && Game.BakuganIndex.Any(x => x.InDrop());

    public override void TriggerEffect()
    {
        Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

        target.MoveFromDropToHand();
    }
}
