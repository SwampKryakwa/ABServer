namespace AB_Server.Gates
{
    internal class Warlock : GateCard
    {
        public Warlock(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Owner.TeamId != Owner.TeamId && x.Position == this }
            ];

            ResTargetSelectors =
            [
                new OptionSelector() { Message = "INFO_PICKER_WARLOCK", ForPlayer = (p) => p == Owner, OptionCount = 2 }
            ];
        }

        public override int TypeId { get; } = 2;

        public override void TriggerEffect()
        {
            int option = (ResTargetSelectors[0] as OptionSelector)!.SelectedOption;
            Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (option == 0)
            {
                // Set power to base power
                target.Boost((short)(target.BasePower - target.Power), this);
            }
            else
            {
                // Remove all markers created by the target
                foreach (var effect in game.ActiveZone.Where(x => x.User == target))
                {
                    effect.Negate();
                    game.ActiveZone.Remove(effect);
                }
            }
        }

        public override bool IsOpenable() => Bakugans.Any(x => x.Owner != Owner) && base.IsOpenable();
    }
}
