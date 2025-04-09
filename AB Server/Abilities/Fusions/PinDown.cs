using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Fusions
{
    internal class PinDown : FusionAbility
    {
        public PinDown(int cID, Player owner) : base(cID, owner, 8, typeof(LeapSting))
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField()}
            ];
        }

        public override void TriggerEffect() =>
            new PinDownEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField();
    }

    internal class PinDownEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public PinDownEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            this.user = user;
            this.target = target;
            
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (target.Position is GateCard targetGate)
            {
                foreach (var bakugan in targetGate.Bakugans.Where(b => b != target))
                {
                    int powerDifference = 400 - bakugan.Power;
                    bakugan.Boost(new Boost((short)powerDifference), this);
                }
            }
        }
    }
}
