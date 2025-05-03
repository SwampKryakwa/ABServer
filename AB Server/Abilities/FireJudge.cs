namespace AB_Server.Abilities
{
    internal class FireJudge(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect() =>
            new FireJudgeEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Nova) && user.OnField();
    }
    internal class FireJudgeEffect(Bakugan user, int typeID, bool IsCopy) : IActive
    {
        public int TypeId { get; } = typeID;
        public int EffectId { get; set; } = user.Game.NextEffectId++;
        public CardKind Kind { get; } = CardKind.NormalAbility;
        public Bakugan User { get; set; } = user;
        Game game { get => User.Game; }
        Boost currentBoost;

        public Player Owner { get; set; } = user.Owner;
        bool IsCopy = IsCopy;

        public void Activate()
        {
            int team = User.Owner.TeamId;
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            currentBoost = new Boost(100);
            User.ContinuousBoost(currentBoost, this);
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            if (currentBoost.Active)
            {
                currentBoost.Active = false;
                User.RemoveBoost(currentBoost, this);
            }

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
        }
    }
}
