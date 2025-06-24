namespace AB_Server.Gates
{
    internal class NormalGate : GateCard
    {
        public NormalGate(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = -1;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));


            game.CheckChain(Owner, this);
        }

        public void Setup()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            if (!Negated && target.Position == this)
                target.Boost(new Boost((short)(new Random().Next(1, 10) * 10)), this);

            game.ChainStep();
        }

        public override void Resolve()
        {
            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, 4, Bakugans.Where(x => x.Owner == Owner))
            ));

            game.OnAnswer[Owner.Id] = Setup;
        }
    }
}
