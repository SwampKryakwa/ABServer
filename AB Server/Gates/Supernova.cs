namespace AB_Server.Gates;

internal class Supernova : GateCard
{
    public Supernova(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;
        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this },
            new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this && x != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
        ];
    }

    public override int TypeId { get; } = 9;

    public override bool IsOpenable() =>
        Game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && OpenBlocking.Count == 0 && !IsOpen && !Negated;

    public override void TriggerEffect()
    {
        var target1 = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        var target2 = (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;

        if (!Negated && target1.Position == this && target2.Position == this)
        {
            var boost = target1.Power - target2.Power;

            target1.Boost(new Boost((short)-boost), this);
            target2.Boost(new Boost((short)boost), this);
        }
    }
}