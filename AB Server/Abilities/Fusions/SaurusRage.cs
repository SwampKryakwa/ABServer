namespace AB_Server.Abilities.Fusions
{
    internal class SaurusRage : FusionAbility
    {
        public SaurusRage(int cID, Player owner) : base(cID, owner, 4, typeof(SaurusGlow))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Power > User.Power }
            ];
        }

        public override void TriggerEffect()
        {
            User.Boost(new Boost((short)Math.Abs((User.Power - (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Power) * 2)), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(x => x.OnField() && x.Power > user.Power);
    }
}
