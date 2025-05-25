namespace AB_Server.Abilities
{
    internal class FireWall : AbilityCard
    {
        public FireWall(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.InBattle && x.Owner != Owner},
                new OptionSelector() { Condition = () => User.IsAttribute(Attribute.Nova), Message = "INFO_PICKER_FIREWALL", ForPlayer = (p) => p == Owner, OptionCount = 2, SelectedOption = 1}
            ];
        }

        public override void TriggerEffect() =>
            new FireWallEffect(User, (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy, (CondTargetSelectors[1] as OptionSelector).SelectedOption).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.Position.Bakugans.Any(x => x.Owner != Owner);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => x.Owner != user.Owner);
    }

    internal class FireWallEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;
        int selectedOption;

        public FireWallEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy, int selectedOption)
        {
            User = user;
            this.target = target;

            this.IsCopy = IsCopy;
            this.selectedOption = selectedOption;
            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            if (selectedOption == 0 && User.IsAttribute(Attribute.Nova))
                target.Boost(new Boost((short)-target.AdditionalPower), this);
            else
                target.Boost(new Boost(-50), this);
        }
    }
}

