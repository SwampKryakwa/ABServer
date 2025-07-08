namespace AB_Server.Abilities
{
    internal class GrandDown : AbilityCard
    {
        public GrandDown(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATENEGATETARGET", TargetValidator = x => x.OnField }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as GateSelector)!.SelectedGate;
            Game.ThrowEvent(EventBuilder.GateRevealed(target));
            target.Negate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Game.GateIndex.Any(x => x.OnField && x.IsOpen);

        public static new bool HasValidTargets(Bakugan user) => user.Game.GateIndex.Any(x => x.OnField && x.IsOpen);
    }
}
