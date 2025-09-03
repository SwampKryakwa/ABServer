namespace AB_Server.Gates
{
    class QuicksandFreeze : GateCard
    {
        public QuicksandFreeze(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this }
            ];
        }

        public override int TypeId { get; } = 12;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Intermediate && BattleOver && OpenBlocking.Count == 0 && !IsOpen && !Negated;

        Bakugan target;
        bool resolved;
        public override void TriggerEffect()
        {
            target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan; ;
            if (!Negated)
            {
                resolved = false;
            }
        }

        public override void Dispose()
        {
            if (!IsOpen || resolved || Negated)
                base.Dispose();
            else
            {
                BattleDeclaredOver = false;
                resolved = true;
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    if (b == target) continue;
                    b.MoveFromFieldToHand(EnterOrder);
                }
            }
        }
    }
}
