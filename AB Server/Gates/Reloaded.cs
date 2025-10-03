namespace AB_Server.Gates;

internal class Reloaded : GateCard
{
    public Reloaded(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;
        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this && x.Owner == Owner },
            new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position != this && x.Owner.TeamId == Owner.TeamId && x != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
        ];
    }

    public override int TypeId { get; } = 10;

    public override void TriggerEffect()
    {
        var target1 = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        var target2 = (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;

        if (target1.OnField())
        {
            target1.Boost(new Boost(100), this);
            if (target2.OnField())
                target2.Boost(new Boost(-100), this);
        }
    }

    public override bool IsOpenable() =>
        !IsOpen && Bakugans.Any(x => x.Owner == Owner) && base.IsOpenable() && Game.GateIndex.Count(x => x.Bakugans.Any(y => y.Owner == Owner)) >= 2 && Game.BakuganIndex.Any(x => x.OnField() && x.Position != this);
}
