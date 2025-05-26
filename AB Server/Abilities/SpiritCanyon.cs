namespace AB_Server.Abilities
{
    internal class SpiritCanyon : AbilityCard
    {
        public SpiritCanyon(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

        public override void TriggerEffect() =>
            new BoostEffect(User, User, (short)(Game.GateIndex.Count(x => x.OnField && x.Owner == Owner) * 50), TypeId, (int)Kind).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Subterra);
    }
}
