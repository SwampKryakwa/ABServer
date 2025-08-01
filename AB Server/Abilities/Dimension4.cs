namespace AB_Server.Abilities
{
    internal class Dimension4 : AbilityCard
    {
        public Dimension4(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

            target?.Boost(new Boost((short)-target.AdditionalPower), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Lucifer && user.InBattle && user.Position.Bakugans.Count >= 2;
    }
}


