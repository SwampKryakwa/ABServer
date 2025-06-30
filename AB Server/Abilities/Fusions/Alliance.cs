namespace AB_Server.Abilities.Fusions
{
    internal class Alliance(int cID, Player owner) : FusionAbility(cID, owner, 9, typeof(Enforcement))
    {
        public override void TriggerEffect()
        {
            new ContinuousBoostMultipleSameUntilDestroyedEffect(User, [.. Owner.BakuganOwned.Where(b => b != User)], 80, TypeId, (int)CardKind.FusionAbility, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.IsPartner && user.Type == BakuganType.Garrison && Game.CurrentWindow == ActivationWindow.Normal;
    }
}
