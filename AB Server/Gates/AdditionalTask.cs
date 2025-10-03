namespace AB_Server.Gates;

internal class AdditionalTask : GateCard
{
    public AdditionalTask(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;

        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => EnterOrder[^1].Contains(x) }
        ];
    }

    public override int TypeId { get; } = 11;

    public override bool IsOpenable() =>
        Game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && OpenBlocking.Count == 0 && !IsOpen && !Negated;

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        target.MoveFromFieldToHand(EnterOrder);
        if (!target.Owner.BakuganOwned.Any(x => x.OnField()))
            target.Owner.AllowedThrows++;
    }
}
