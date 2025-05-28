using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class Blowback(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect() =>
            new RetractBakuganEffect(User, User, TypeId, (int)Kind, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Owner.BakuganOwned.Any(b => b.IsAttribute(Attribute.Zephyros)) && user.OnField() && Game.CurrentWindow == ActivationWindow.Normal;
    }
}
