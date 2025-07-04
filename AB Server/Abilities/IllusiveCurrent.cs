using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class IllusiveCurrent : AbilityCard
    {
        public IllusiveCurrent(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InHand() }
            ];
        }

        public override void TriggerEffect() => new IllusiveCurrentEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Owner.Bakugans.Any(x => x.IsAttribute(Attribute.Aqua));

        public static new bool HasValidTargets(Bakugan user) => user.OnField();
    }

    internal class IllusiveCurrentEffect(Bakugan user, Bakugan selectedBakugan, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan selectedBakugan = selectedBakugan;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            

            if (User.Position is GateCard positionGate && selectedBakugan.InHand())
            {
                User.ToHand(positionGate.EnterOrder);
                selectedBakugan.AddFromHand(positionGate);
            }
        }
    }
}

