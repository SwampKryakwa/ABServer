namespace AB_Server.Abilities
{
    internal class Enforcement(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect()
        {
            new ContinuousBoostEffect(User, User, 50, TypeId, CardKind.NormalAbility, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Owner.BakuganOwned.Any(x => x.Type == BakuganType.Garrison) && user.OnField();
    }
}
