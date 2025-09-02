namespace AB_Server.Gates
{
    internal class LevelDown : GateCard
    {
        public LevelDown(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_DECREASETARGET", TargetValidator = x => x.Position == this }
            ];
        }

        public override int TypeId { get; } = 0;

        public override void TriggerEffect()
        {
            Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

            if (!Negated && target.Power >= 400 && target.Position == this)
                target.Boost(new Boost(-100), this);
        }
    }
}
