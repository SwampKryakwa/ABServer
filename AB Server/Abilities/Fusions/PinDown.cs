using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
{
    internal class PinDown : FusionAbility
    {
        public PinDown(int cID, Player owner) : base(cID, owner, 8, typeof(LeapSting))
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField()}
            ];
        }

        public override void TriggerEffect() =>
            new PinDownEffect(User, (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField();
    }

    internal class PinDownEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan user = user;
        Bakugan target = target;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            

            if (target.Position is GateCard targetGate)
                foreach (var bakugan in targetGate.Bakugans.Where(b => b != target))
                {
                    int powerDifference = 400 - bakugan.Power;
                    bakugan.Boost(new Boost((short)powerDifference), this);
                }
        }
    }
}
