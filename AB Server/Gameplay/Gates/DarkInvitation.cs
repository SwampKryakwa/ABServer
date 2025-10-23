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
            new PlayerSelector { ForPlayer = p => p == Owner, Message = "INFO_PICK_PLAYER", TargetValidator = (p) => p.TeamId != Owner.TeamId },
            new YesNoSelector { ForPlayer = p => p == (CondTargetSelectors[0] as PlayerSelector)!.SelectedPlayer, Message = "INFO_WANTTARGET", IsYes = false },
            new BakuganSelector { ClientType = "BH", ForPlayer = p => p.TeamId != Owner.TeamId, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Owner == Game.Players.First(p => p.TeamId != Owner.TeamId) && x.InHand(), Condition = () => (CondTargetSelectors[1] as YesNoSelector)!.IsYes }
        ];
    }

    public override int TypeId { get; } = 16;

    public override void TriggerEffect()
    {
        if (!(CondTargetSelectors[1] as YesNoSelector)!.IsYes)
            return;

        Bakugan target = (CondTargetSelectors[2] as BakuganSelector)!.SelectedBakugan;

        target.AddFromHandToField(this);
    }
}
