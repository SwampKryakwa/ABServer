namespace AB_Server.Abilities
{
    internal class SpiritCanyon(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect()
        {
            User.Boost(Game.GateIndex.Count(x => x.OnField && x.Owner == Owner) * 50, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Subterra);
    }
}
