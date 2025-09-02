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

            CondTargetSelectors =
            [
                new AbilitySelector() { ClientType = "A", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Owner == Owner && x.Kind == CardKind.CorrelationAbility }
            ];
        }

        public override int TypeId { get; } = 14;

        public override void TriggerEffect()
        {
            AbilityCard target = (CondTargetSelectors[0] as AbilitySelector)!.SelectedAbility;

            if (!Negated && Owner.BakuganOwned.DistinctBy(x => x.Type).Count() == Owner.BakuganOwned.Count
            && Owner.BakuganOwned.DistinctBy(x => x.BaseAttribute).Count() == Owner.BakuganOwned.Count
            && Owner.BakuganOwned.DistinctBy(x => x.BasePower).Count() == Owner.BakuganOwned.Count)
                target.FromDropToHand();
        }

        public override bool IsOpenable() => game.ActiveZone.Any(x => x is not GateCard && x is not AbilityCard) && base.IsOpenable();
    }
}
