namespace AB_Server.Gates
{
    internal class EnergyMerge : GateCard
    {
        public EnergyMerge(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => EnterOrder[0].Contains(x) },
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this && x != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
            ];
        }

        public override int TypeId { get; } = 22;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !IsOpen && !Negated && Bakugans.Count >= 2 && Bakugans.Any(x => x.Owner == Owner && x.InBattle);

        public override void TriggerEffect()
        {
            Bakugan target1 = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            Bakugan target2 = (CondTargetSelectors[1] as BakuganSelector)!.SelectedBakugan;

            if (target1.Position == this)
            {
                target1.Boost(-100, this);
                if (target2.Position == this)
                    target2.Boost(100, this);
            }
        }
    }
}
