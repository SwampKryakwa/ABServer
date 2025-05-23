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
        Boost boost;

        public Player Owner { get; set; } = user.Owner;
        bool IsCopy = IsCopy;

        public void Activate()
        {
            int team = User.Owner.TeamId;
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            boost = new Boost(100);
            User.ContinuousBoost(boost, this);

            game.BakuganDestroyed += CheckExpiration;
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });

            if (boost.Active)
            {
                boost.Active = false;
                User.RemoveBoost(boost, this);
            }
        }

        public void CheckExpiration(Bakugan target, byte owner)
        {
            if (target != User) return;

            game.ActiveZone.Remove(this);

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });

            if (boost.Active)
            {
                boost.Active = false;
                User.RemoveBoost(boost, this);
            }

            game.BakuganDestroyed -= CheckExpiration;
        }
    }
}
