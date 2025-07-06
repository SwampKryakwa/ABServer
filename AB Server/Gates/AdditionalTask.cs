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

            game.OnAnswer[Owner.Id] = ReturnTargetToHand;
        }

        Bakugan target;

        public void ReturnTargetToHand()
        {
            target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            if (Negated)
            {
                game.ChainStep();
                return;
            }

            if (target.OnField())
                target.ToHand((target.Position as GateCard)!.EnterOrder);

            if (target.Owner.Bakugans.Any(x => x != target))
            {
                game.NewEvents[target.Owner.Id].Add(EventBuilder.SelectionBundler(false,
                    EventBuilder.FieldBakuganSelection("INFO_GATE_ADDTARGET", TypeId, (int)Kind, target.Owner.Bakugans.Where(x => x != target))
                ));

                game.OnAnswer[Owner.Id] = AddOtherBakugan;
            }
            else
                game.ChainStep();
        }

        public void AddOtherBakugan()
        {
            game.BakuganIndex[(int)game.PlayerAnswers[target.Owner.Id]!["array"][0]["bakugan"]].AddFromHand(this);

            game.ChainStep();
        }
    }
}
