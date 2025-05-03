namespace AB_Server.Gates
{
    internal class Transform : GateCard
    {
        public Transform(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 4;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x.Owner == Owner))
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
            if (!Negated && target.Position == this)
                target.Boost(new Boost((short)(Owner.Bakugans.MaxBy(x => x.Power).Power - target.Power)), this);

            game.ChainStep();
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Owner.Bakugans.Count != 0;
    }
}
