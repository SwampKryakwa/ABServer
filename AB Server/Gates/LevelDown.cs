namespace AB_Server.Gates
{
    internal class LevelDown : GateCard
    {
        public LevelDown(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 0;

        public override void Resolve()
        {
            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_DECREASETARGET", TypeId, (int)Kind, Bakugans)
            ));

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            if (!Negated && target.Power >= 400 && target.Position == this)
                target.Boost(new Boost(-100), this);

            game.ChainStep();
        }
    }
}
