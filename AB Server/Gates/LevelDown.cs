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

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_DECREASETARGET", TypeId, (int)Kind, Bakugans)
            ));

            game.OnAnswer[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated && target.Power >= 400 && target.Position == this)
                target.Boost(new Boost(-100), this);

            game.ChainStep();
        }
    }
}
