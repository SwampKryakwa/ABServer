namespace AB_Server.Gates
{
    internal class AdditionalTask : GateCard
    {
        public AdditionalTask(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 11;

        public override bool IsOpenable() => 
            false;

        public override void CheckAutoBattleStart()
        {
            if (OpenBlocking.Count == 0 && !IsOpen && !Negated)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, EnterOrder[^1])
            ));

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            if (!Negated && target.Position == this)
                target.ToHand(EnterOrder);

            game.ChainStep();
        }
    }
}
