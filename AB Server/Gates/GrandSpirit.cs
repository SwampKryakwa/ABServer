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

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x.Owner == Owner))
            ));

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            if (!Negated && target.Position == this)
                target.Boost(new Boost((short)(50 * game.Field.Cast<GateCard?>().Count(x => x is GateCard gate && gate.Owner.TeamId != Owner.TeamId && gate != this))), this);

            game.ChainStep();
        }
    }
}
