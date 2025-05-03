using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class LeapSting : AbilityCard
    {
        public LeapSting(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.Owner.TeamId != Owner.TeamId && x.OnField() && x.Position != User.Position}
            ];
        }

        public override void TriggerEffect() =>
            new LeapStingEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField() && Game.BakuganIndex.Any(x => x.Owner.TeamId != Owner.TeamId && x.OnField() && x.Position != user.Position);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && x.Position != user.Position && user.IsEnemyOf(x));
    }

    internal class LeapStingEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan target = target;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            if (target.Power < User.Power)
                if (target.Position is GateCard positionGate)
                    target.DestroyOnField(positionGate.EnterOrder);
        }
    }
}
