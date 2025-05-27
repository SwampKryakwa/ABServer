namespace AB_Server.Abilities
{
    internal class BloomOfAgony : AbilityCard
    {
        public BloomOfAgony(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

        public override void TriggerEffect() => new BoostAllFieldEffect(User, -300, TypeId, (int)Kind).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.BattleStart && user.OnField() && user.IsAttribute(Attribute.Darkon);
    }
}
