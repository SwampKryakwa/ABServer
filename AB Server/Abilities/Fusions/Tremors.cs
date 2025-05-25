using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
{
    internal class Tremors : FusionAbility
    {
        public Tremors(int cID, Player owner) : base(cID, owner, 5, typeof(NoseSlap))
        {
            CondTargetSelectors =
            [
                new MultiBakuganSelector() { ClientType = "MBF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGETS", TargetValidator = x => x.OnField() && !(x.Position as GateCard).IsTouching(User.Position as GateCard) && x.Position != User.Position && x.IsEnemyOf(User) }
            ];
        }

        public override void TriggerEffect() =>
            new TremorsEffect(User, (CondTargetSelectors[0] as MultiBakuganSelector).SelectedBakugans, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.OnField() && Game.BakuganIndex.Any(x => x.OnField() && !(x.Position as GateCard).IsTouching(user.Position as GateCard) && x.Position != user.Position && x.IsEnemyOf(user));
    }

    internal class TremorsEffect(Bakugan user, Bakugan[] targets, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan user = user;
        Bakugan[] targets = targets;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            foreach (var target in targets)
            {
                if (target.Power < user.Power)
                {
                    // Destroy the target Bakugan if it is on the field
                    if (target.Position is GateCard positionGate)
                        target.DestroyOnField(positionGate.EnterOrder);
                }
            }
        }
    }
}
