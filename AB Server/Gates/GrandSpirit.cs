namespace AB_Server.Gates;

internal class GrandSpirit : GateCard
{
    public GrandSpirit(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;

        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this && x.Owner == Owner }
        ];
    }

    public override int TypeId { get; } = 8;

    public override bool IsOpenable() => base.IsOpenable() && Bakugans.Any(x => x.Owner == Owner);

    public override void TriggerEffect()
    {
        Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

        if (!Negated && target.Position == this)
            target.Boost(new Boost((short)(50 * Game.Field.Cast<GateCard?>().Count(x => x is GateCard gate && gate.Owner == Owner))), this);
    }
}
