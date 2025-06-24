using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class VicariousVictim : AbilityCard
    {
        public VicariousVictim(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BG", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InGrave()}
            ];
        }

        public override void TriggerEffect() => new IllusiveCurrentEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && Owner.BakuganGrave.Bakugans.Count != 0 && user.Type == BakuganType.Griffon;
    }

    internal class VicariousVictimEffect(Bakugan user, Bakugan selectedBakugan, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan selectedBakugan = selectedBakugan;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            

            if (User.Position is GateCard positionGate && selectedBakugan.Position is BakuganGrave)
            {
                selectedBakugan.FromGrave(positionGate);
                User.DestroyOnField(positionGate.EnterOrder);
            }
        }
    }
}