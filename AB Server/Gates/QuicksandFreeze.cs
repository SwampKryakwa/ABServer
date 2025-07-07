namespace AB_Server.Gates
{
    class QuicksandFreeze : GateCard
    {
        public QuicksandFreeze(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 12;

        public override void CheckAutoBattleEnd()
        {
            if (OpenBlocking.Count == 0 && !IsOpen && !Negated)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans)
            ));

            game.OnAnswer[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated)
            {
                resolved = false;
            }

            game.ChainStep();
        }

        bool resolved;

        public override void Dispose()
        {
            if (!IsOpen || resolved || Negated)
                base.Dispose();
            else
            {
                resolved = true;
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    if (b == target) continue;
                    b.MoveFromFieldToHand(EnterOrder);
                }
            }
        }

        public override bool IsOpenable() =>
            false;
    }
}
