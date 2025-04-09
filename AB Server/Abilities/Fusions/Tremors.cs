using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Fusions
{
    internal class Tremors : FusionAbility
    {
        public Tremors(int cID, Player owner) : base(cID, owner, 5, typeof(NoseSlap))
        {
            TargetSelectors =
            [
                new MultiBakuganSelector() { ClientType = "MBF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGETS", TargetValidator = x => x.OnField() && !(x.Position as GateCard).IsTouching(User.Position as GateCard) && x.Position != User.Position && x.IsEnemyOf(User) }
            ];
        }

        public override void TriggerEffect() =>
            new TremorsEffect(User, (TargetSelectors[0] as MultiBakuganSelector).SelectedBakugans, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.OnField();
    }

    internal class TremorsEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan[] targets;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public TremorsEffect(Bakugan user, Bakugan[] targets, int typeID, bool IsCopy)
        {
            this.user = user;
            this.targets = targets;

            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

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
