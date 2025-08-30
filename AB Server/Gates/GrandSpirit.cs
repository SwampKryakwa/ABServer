namespace AB_Server.Gates
{
    internal class GrandSpirit : GateCard
    {
        public GrandSpirit(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 8;

        public override bool IsOpenable() => base.IsOpenable() && Bakugans.Any(x => x.Owner == Owner);

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
                target.Boost(new Boost((short)(50 * game.Field.Cast<GateCard?>().Count(x => x is GateCard gate && gate.Owner == Owner && gate != this))), this);

            game.ChainStep();
        }
    }
}
