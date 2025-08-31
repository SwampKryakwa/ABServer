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

        Bakugan fieldBakugan;
        Bakugan handBakugan;

        public override int TypeId { get; } = 3;
        
        public override void Open()
        {
            // Target 1 of your bakugan on this card
            var ownBakugans = Bakugans.Where(x => x.Owner == Owner).ToArray();
            if (ownBakugans.Length == 0)
            {
                game.ChainStep();
                return;
            }

            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, ownBakugans)
            ));

            game.OnAnswer[Owner.Id] = () =>
            {
                fieldBakugan = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

                // Target 1 bakugan with lower base power in your hand
                var handCandidates = Owner.BakuganOwned
                    .Where(x => x.InHand() && x.BasePower < fieldBakugan.BasePower).ToArray();

                if (handCandidates.Length == 0)
                {
                    game.ChainStep();
                    return;
                }

                game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                    EventBuilder.HandBakuganSelection("INFO_GATE_ADDTARGET", TypeId, (int)Kind, handCandidates)
                ));

                game.OnAnswer[Owner.Id] = () =>
                {
                    handBakugan = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];
                    game.CheckChain(Owner, this);
                };
            };
        }

        public override void Resolve()
        {
            if (Negated || fieldBakugan == null || handBakugan == null || !fieldBakugan.OnField() || !handBakugan.InHand())
            {
                game.ChainStep();
                return;
            }

            // Place the second target onto this card
            handBakugan.AddFromHandToField(this);

            // Then it gets -100G for each 100G in its power (after placement)
            int penalty = handBakugan.Power / 100 * 100;
            handBakugan.Boost((short)-penalty, this);

            game.ChainStep();
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Bakugans.Any(x => x.Owner == Owner && Owner.Bakugans.Any(y => y.BasePower < x.BasePower));
    }
}
