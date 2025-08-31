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

        Bakugan? target;

        public override void Open()
        {
            // Target the last Bakugan placed on this card (REQ)
            if (EnterOrder.Count == 0 || EnterOrder[^1].Length == 0)
            {
                target = null;
                game.ChainStep();
                return;
            }

            var lastBakugan = EnterOrder[^1][^1];
            if (lastBakugan == null || !lastBakugan.OnField() || lastBakugan.Position != this)
            {
                target = null;
                game.ChainStep();
                return;
            }

            target = lastBakugan;
            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (Negated || target == null || !target.OnField() || target.Position != this || EnterOrder.Count == 0 || EnterOrder[^1].Length == 0)
            {
                game.ChainStep();
                return;
            }

            // Return the target to its owner's hand
            target.MoveFromFieldToHand((target.Position as GateCard)!.EnterOrder);

            var owner = target.Owner;
            var handCandidates = owner.BakuganOwned.Where(x => x.InHand() && x != target).ToArray();
            var gateCandidates = game.GateIndex
                .Where(g => g is GateCard card && card.OnField && card != this)
                .Cast<GateCard>()
                .ToArray();

            if (handCandidates.Length == 0 || gateCandidates.Length == 0)
            {
                game.ChainStep();
                return;
            }

            // Prompt target owner to choose a Bakugan from hand (not the one just returned)
            game.ThrowEvent(owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.HandBakuganSelection("INFO_GATE_ADDTASK_BAKUGAN", TypeId, (int)Kind, handCandidates)
            ));

            game.OnAnswer[owner.Id] = () =>
            {
                var chosenBakugan = game.BakuganIndex[(int)game.PlayerAnswers[owner.Id]!["array"][0]["bakugan"]];

                // Prompt to select a different gate on the field
                game.ThrowEvent(owner.Id, EventBuilder.SelectionBundler(false,
                    EventBuilder.FieldGateSelection("INFO_GATE_ADDTASK_GATE", TypeId, (int)Kind, gateCandidates)
                ));

                game.OnAnswer[owner.Id] = () =>
                {
                    var gateIndex = (int)game.PlayerAnswers[owner.Id]!["array"][0]["gate"];
                    var chosenGate = gateCandidates[gateIndex];
                    chosenBakugan.AddFromHandToField(chosenGate);
                    game.ChainStep();
                };
            };
        }
    }
}
