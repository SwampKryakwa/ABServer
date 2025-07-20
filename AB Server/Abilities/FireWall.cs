namespace AB_Server.Abilities
{
    internal class FireWall : AbilityCard
    {
        public FireWall(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.InBattle && x.Owner != Owner}
            ];

            ResTargetSelectors =
            [
                new OptionSelector() { Condition = () => User.IsAttribute(Attribute.Nova), Message = "INFO_PICKER_FIREWALL", ForPlayer = (p) => p == Owner, OptionCount = 2, SelectedOption = 1}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if ((ResTargetSelectors[0] as OptionSelector)!.SelectedOption == 0 && User.IsAttribute(Attribute.Nova))
                target.Boost(new Boost((short)-target.AdditionalPower), this);
            else
                target.Boost(new Boost(-50), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.InBattle && user.Position.Bakugans.Any(x => x.Owner != Owner);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => x.Owner != user.Owner);
    }
}

