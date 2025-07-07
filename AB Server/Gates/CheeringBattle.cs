namespace AB_Server.Gates
{
    internal class CheeringBattle : GateCard
    {
        public CheeringBattle(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 3;

        public override void Resolve()
        {
            if (!Negated && Owner.Bakugans.Count >= 0 && game.BakuganIndex.Count(x => x.Owner.TeamId != Owner.TeamId) > game.BakuganIndex.Count(x => x.Owner == Owner))
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                    EventBuilder.HandBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, game.Players[Owner.Id].Bakugans)
                ));

                game.OnAnswer[Owner.Id] = Activate;
            }
            else
                game.CheckChain(Owner, this);
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            target.AddFromHand(this);
            var newPower = target.Power - (target.Power / 100 * 100);
            target.Boost(new Boost((short)(newPower - target.Power)), this);

            game.ChainStep();
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Owner.Bakugans.Count > 0;
    }
}
