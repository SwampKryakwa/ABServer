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
            game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && OpenBlocking.Count == 0 && !IsOpen && !Negated;
            
        public override void Resolve()
        {
            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
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
                target.MoveFromFieldToHand((target.Position as GateCard)!.EnterOrder);

            if (target.Owner.Bakugans.Any(x => x != target))
            {
                game.ThrowEvent(target.Owner.Id, EventBuilder.SelectionBundler(false,
                    EventBuilder.HandBakuganSelection("INFO_GATE_ADDTARGET", TypeId, (int)Kind, target.Owner.Bakugans.Where(x => x != target))
                ));

                game.OnAnswer[target.Owner.Id] = AddOtherBakugan;
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
