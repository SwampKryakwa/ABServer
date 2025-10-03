namespace AB_Server.Gates;

internal class DarkInvitation : GateCard
{
    public DarkInvitation(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;

        CardId = cID;

        CondTargetSelectors =
        [
            new YesNoSelector { ForPlayer = p => p.TeamId != Owner.TeamId, Message = "INFO_WANTTARGET" },
            new BakuganSelector { ClientType = "BH", ForPlayer = p => p.TeamId != Owner.TeamId, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Owner == Game.Players.First(p => p.TeamId != Owner.TeamId) && x.InHand(), Condition = () => (CondTargetSelectors[0] as YesNoSelector)!.IsYes }
        ];
    }

    public override int TypeId { get; } = 16;

    public override void TriggerEffect()
    {
        if (!(CondTargetSelectors[0] as YesNoSelector)!.IsYes)
            return;

        Bakugan target = (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;

        target.AddFromHandToField(this);
    }
}
