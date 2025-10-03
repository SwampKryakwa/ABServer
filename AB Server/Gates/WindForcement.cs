namespace AB_Server.Gates;

internal class WindForcement : GateCard
{
    public WindForcement(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;

        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this }
        ];
    }

    public override int TypeId { get; } = 21;

    public override bool IsOpenable() =>
        Game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !IsOpen && !Negated && Bakugans.Any(x => x.Owner == Owner && x.InBattle);

    public override void TriggerEffect()
    {
        Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

        if (target.Position != this) return;

        Attribute[] targetAttrs = (target.attributeChanges.Count > 0) ? [.. target.attributeChanges[^1].Attributes] : [target.BaseAttribute];

        int count = 0;
        foreach (var bakugan in Game.BakuganIndex.Where(x => x != target && x.OnField()))
            foreach (var attr in targetAttrs)
                if (bakugan.IsAttribute(attr))
                {
                    count++;
                    break;
                }

        target.Boost(count * 100, this);
    }
}
