namespace AB_Server.Gates
{
    internal class CheeringBattle : GateCard
    {
        public CheeringBattle(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Owner == Owner && x.Position == this },
                new BakuganSelector { ClientType = "BH", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Owner == Owner && x.InHand() && x.BasePower < (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.BasePower }
            ];
        }

        public override int TypeId { get; } = 3;

        public override void TriggerEffect()
        {
            var handBakugan = (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;
            // Place the second target onto this card
            handBakugan.AddFromHandToField(this);

            // Then it gets -100G for each 100G in its power (after placement)
            int penalty = handBakugan.Power / 100 * 100;
            handBakugan.Boost((short)-penalty, this);
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Bakugans.Any(x => x.Owner == Owner && Owner.Bakugans.Any(y => y.BasePower < x.BasePower));
    }
}
