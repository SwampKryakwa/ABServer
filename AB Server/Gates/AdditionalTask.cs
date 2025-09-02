namespace AB_Server.Gates
{
    internal class AdditionalTask : GateCard
    {
        public AdditionalTask(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => EnterOrder[^1].Contains(x) }
            ];

            ResTargetSelectors =
            [
                new BakuganSelector { ClientType = "BH", ForPlayer = x => x == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner, Message = "INFO_GATE_ADDTARGET", TargetValidator = x => x != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan && (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner.Bakugans.Contains(x) }
            ];
        }

        public override int TypeId { get; } = 11;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && OpenBlocking.Count == 0 && !IsOpen && !Negated;

        public override void Resolve()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (target.Position != this)
            {
                game.ChainStep();
                return;
            }
            target.MoveFromFieldToHand((target.Position as GateCard)!.EnterOrder);
            base.Resolve();
        }

        public override void TriggerEffect()
        {
            var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            target.AddFromHandToField(this);
        }
    }
}
