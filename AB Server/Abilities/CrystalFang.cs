namespace AB_Server.Abilities
{
    internal class CrystalFang : AbilityCard
    {
        public CrystalFang(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.IsOpponentOf(User) && target.OnField()}
            ];
        }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.AnyBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
            new BoostEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, -100, TypeId, (int)Kind).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            (user.OnField() || user.InHand()) && user.Type == BakuganType.Tigress && Game.CurrentWindow == ActivationWindow.BattleStart;
    }
}


