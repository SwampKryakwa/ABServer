namespace AB_Server.Gates
{
    internal class DarkInvitation : GateCard
    {
        public DarkInvitation(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 16;

        Player targetPlayer;
        public override void Resolve()
        {
            targetPlayer = game.Players.First(x => x.TeamId != Owner.TeamId);

            if (!Negated)
            {
                game.ThrowEvent(targetPlayer.Id,EventBuilder.SelectionBundler(false,
                    EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                ));

                game.OnAnswer[Owner.Id] = Confirm;
            }
            else
                game.CheckChain(Owner, this);
        }

        public void Confirm()
        {
            if ((bool)game.PlayerAnswers[Owner.Id]!["array"][0]["answer"])
            {
                game.ThrowEvent(targetPlayer.Id, EventBuilder.SelectionBundler(false,
                    EventBuilder.HandBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, game.Players[targetPlayer.Id].Bakugans)
                ));

                game.OnAnswer[Owner.Id] = Activate;
            }
            else
                game.ChainStep();
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[targetPlayer.Id]!["array"][0]["bakugan"]];

            target.AddFromHandToField(this);

            game.ChainStep();
        }
    }
}
