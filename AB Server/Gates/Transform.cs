namespace AB_Server.Gates
{
    internal class Transform : GateCard
    {
        public Transform(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this && x.Owner == Owner }
            ];
        }

        public override int TypeId { get; } = 4;

        public override void TriggerEffect()
        {
            Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

            if (!Negated && target.Position == this && Owner.Bakugans.Count != 0)
                target.Boost(new Boost((short)(Owner.Bakugans.MaxBy(x => x.Power).Power - target.Power)), this);
        }

        public override bool IsOpenable() =>
            base.IsOpenable() && Owner.Bakugans.Count != 0;
    }
}
