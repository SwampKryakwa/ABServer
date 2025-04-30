using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class GrandDown : AbilityCard
    {
        public GrandDown(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_GATENEGATETARGET", TargetValidator = x => x.OnField}
            ];
        }

        public override void TriggerEffect() =>
                new GrandDownEffect(User, (TargetSelectors[0] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Game.GateIndex.Any(x => x.OnField && x.IsOpen);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(x => x.OnField && x.IsOpen);
    }
    internal class GrandDownEffect(Bakugan user, GateCard target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        GateCard target = target;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            target.IsOpen = true;
            target.Negate();
        }
    }
}
