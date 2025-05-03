namespace AB_Server.Gates
{
    internal class Supernova : GateCard
    {
        public Supernova(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            CardId = cID;
        }

        public override int TypeId { get; } = 9;

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

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans)
            ));

            game.OnAnswer[Owner.Id] = Setup1;
        }

        Bakugan target1;
        Bakugan target2;

        public void Setup1()
        {
            target1 = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x != target1))
            ));

            game.OnAnswer[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            target2 = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated && target1.Position == this && target2.Position == this)
            {
                var boost = target1.Power - target2.Power;

                target1.Boost(new Boost((short)-boost), this);
                target2.Boost(new Boost((short)boost), this);
            }

            game.ChainStep();
        }
    }
}