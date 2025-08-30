namespace AB_Server.Gates
{
    internal class Reloaded : GateCard
    {
        public Reloaded(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;
            CardId = cID;
        }

        public override int TypeId { get; } = 10;

        Bakugan target1;
        Bakugan target2;

        public override void Open()
        {
            // Target 1 of your bakugan on this card
            var ownOnThisCard = Bakugans.Where(x => x.Owner == Owner).ToArray();
            if (ownOnThisCard.Length == 0)
            {
                game.ChainStep();
                return;
            }

            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_RELOADED_SELECT_SELF", TypeId, (int)Kind, ownOnThisCard)
            ));

            game.OnAnswer[Owner.Id] = () =>
            {
                target1 = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

                // Target 1 of your or allied bakugan on a different gate card
                var alliedBakugansOnOtherGates = game.GateIndex
                    .Where(g => g is GateCard card && card.OnField && card != this)
                    .Cast<GateCard>()
                    .SelectMany(card => card.Bakugans.Where(b => b.Owner.TeamId == Owner.TeamId))
                    .ToArray();

                if (alliedBakugansOnOtherGates.Length == 0)
                {
                    // If there's no valid second target, chain step and do nothing
                    target1 = null;
                    game.ChainStep();
                    return;
                }

                game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                    EventBuilder.FieldBakuganSelection("INFO_GATE_RELOADED_SELECT_ALLIED_OTHERGATE", TypeId, (int)Kind, alliedBakugansOnOtherGates)
                ));

                game.OnAnswer[Owner.Id] = () =>
                {
                    target2 = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];
                    game.CheckChain(Owner, this);
                };
            };
        }

        public override void Resolve()
        {
            if (Negated || target1 == null || target2 == null)
            {
                game.ChainStep();
                return;
            }

            if (target1.OnField())
            {
                target1.Boost(new Boost(100), this);
                if (target2.OnField())
                    target2.Boost(new Boost(-100), this);
            }
            game.ChainStep();
        }

        public override bool IsOpenable() =>
            !IsOpen && Bakugans.Any(x => x.Owner == Owner) && base.IsOpenable() && game.GateIndex.Count(x => x.Bakugans.Any(y => y.Owner == Owner)) >= 2 && game.BakuganIndex.Any(x => x.OnField() && x.Position != this);
    }
}
