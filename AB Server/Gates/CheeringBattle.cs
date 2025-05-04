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

        int selectingPlayer;
        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));
            selectingPlayer = game.TurnPlayer;
            game.NewEvents[selectingPlayer].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, game.Players[selectingPlayer].Bakugans)
            ));

            game.OnAnswer[selectingPlayer] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.PlayerAnswers[selectingPlayer]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated && target.InHand())
            {
                target.AddFromHand(this);
                var newPower = int.Parse(target.Power.ToString().Substring(1));
                target.Boost(new Boost((short)(newPower - target.Power)), this);
            }

            game.ChainStep();
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Owner.Bakugans.Count > 0;
    }
}
