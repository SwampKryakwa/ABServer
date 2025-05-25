using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class NoseSlap : AbilityCard
    {
        public NoseSlap(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.OnField() && x.Owner != Owner && (x.Position as GateCard).IsAdjacentVertically(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect() =>
            new NoseSlapEffect(User, (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(x => x.OnField && x.IsAdjacentVertically(user.Position as GateCard) && x.Bakugans.Any(user.IsEnemyOf));
    }

    internal class NoseSlapEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan target = target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            game.StartLongRangeBattle(User, target);
        }
    }
}
