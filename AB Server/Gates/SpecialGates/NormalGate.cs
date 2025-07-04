using AB_Server.Abilities;

namespace AB_Server.Gates.SpecialGates
{
    internal class NormalGate : GateCard
    {
        public NormalGate(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 0;
        public override CardKind Kind { get; } = CardKind.SpecialGate;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Push(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, 4, Bakugans.Where(x => x.Owner == Owner))
            ));

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            if (!Negated && target.Position == this)
                target.Boost(new Boost((short)(new Random().Next(1, 10) * 10)), this);

            game.ChainStep();
        }
    }
}
