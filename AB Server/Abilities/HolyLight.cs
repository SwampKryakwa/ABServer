namespace AB_Server.Abilities
{
    internal class HolyLight : AbilityCard
    {
        public HolyLight(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BG", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_REVIVETARGET", TargetValidator = x => x.Owner == Owner && x.Power == Game.BakuganIndex.Where(x=>x.InGrave()).Min(x=>x.Power) && x.InGrave()}
            ];
        }

        public override void TriggerEffect() =>
                new HolyLightEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Lumina) && user.OnField() && Owner.BakuganGrave.Bakugans.Count != 0;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Owner.BakuganGrave.Bakugans.Count != 0;
    }

    internal class HolyLightEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan target = target;
        Game game { get => User.Game; }


        public Player Onwer { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            
            target.Revive();
        }
    }
}
