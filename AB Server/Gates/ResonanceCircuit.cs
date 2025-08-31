using AB_Server.Abilities;

namespace AB_Server.Gates
{
    internal class ResonanceCircuit : GateCard
    {
        public ResonanceCircuit(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 14;

        public override void Resolve()
        {

            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                    EventBuilder.AbilitySelection("INFO_GATE_ABILITYTARGET", game.AbilityIndex.Where(x => x.Owner == Owner && x.Kind == CardKind.CorrelationAbility))
                ));

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            AbilityCard target = game.AbilityIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            if (!Negated && Owner.BakuganOwned.DistinctBy(x => x.Type).Count() == Owner.BakuganOwned.Count
            && Owner.BakuganOwned.DistinctBy(x => x.BaseAttribute).Count() == Owner.BakuganOwned.Count
            && Owner.BakuganOwned.DistinctBy(x => x.BasePower).Count() == Owner.BakuganOwned.Count)
                target.FromDropToHand();

            game.ChainStep();
        }

        public override bool IsOpenable() => game.ActiveZone.Any(x => x is not GateCard && x is not AbilityCard) && base.IsOpenable();
    }
}
