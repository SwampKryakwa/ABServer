namespace AB_Server.Gates
{
    internal class Transform : GateCard
    {
        public Transform(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 4;

        public override void Resolve()
        {

            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x.Owner == Owner))
            ));

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            if (!Negated && target.Position == this)
                target.Boost(new Boost((short)(Owner.Bakugans.MaxBy(x => x.Power).Power - target.Power)), this);

            game.ChainStep();
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Owner.Bakugans.Count != 0;
    }
}
