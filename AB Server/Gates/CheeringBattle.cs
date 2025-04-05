namespace AB_Server.Gates
{
    internal class CheeringBattle : GateCard
    {
        public CheeringBattle(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 3;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Owner.Bakugans)
            ));

            game.AwaitingAnswers[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.ResolveChain();
        }

        public override void Resolve()
        {
            if (!counterNegated && target.InHands)
            {
                target.AddFromHand(this);
                var newPower = int.Parse(target.Power.ToString().Substring(1));
                target.Boost(new Boost((short)(newPower - target.Power)), this);
            }

            game.ContinueGame();
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Owner.Bakugans.Count > 0;
    }
}
