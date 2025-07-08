namespace AB_Server.Abilities
{
    internal class BloomOfAgony : AbilityCard
    {
        public BloomOfAgony(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

        public override void TriggerEffect()
        {
            foreach (Bakugan target in Game.BakuganIndex.Where(x => x.OnField()))
                target.Boost(-300, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.BattleStart && user.OnField() && user.IsAttribute(Attribute.Darkon);
    }
}
