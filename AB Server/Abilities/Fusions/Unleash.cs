namespace AB_Server.Abilities
{
    internal class Unleash(int cID, Player owner) : FusionAbility(cID, owner, 0, typeof(AbilityCard))
    {
        public override void TriggerEffect() =>
            User.Boost(50, this);

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.IsPartner && Game.CurrentWindow == ActivationWindow.Normal;
    }
}
