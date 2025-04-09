using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireJudge : AbilityCard
    {
        public FireJudge(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new FireJudgeEffect(User, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void TriggerEffect() =>
            new FireJudgeEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Nova) && user.OnField();
    }
    internal class FireJudgeEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public CardKind Kind { get; } = CardKind.NormalAbility;
        public Bakugan User { get; set; }
        Game game { get => User.Game; }
        Boost currentBoost;

        public Player Owner { get; set; }
        bool IsCopy;

        public FireJudgeEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
            this.IsCopy = IsCopy; Owner = user.Owner;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;
            game.ActiveZone.Add(this);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" }, { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
                game.NewEvents[i].Add(EventBuilder.AddEffectToActiveZone(this, IsCopy));
            }

            currentBoost = new Boost(100);
            User.Boost(currentBoost, this);

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;
        }

        private void OnBakuganLeaveField(Bakugan target, byte owner)
        {
            if (target == User)
            {
                currentBoost = new Boost(100);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganDestroyed -= OnBakuganLeaveField;
            game.BakuganReturned -= OnBakuganLeaveField;

            if (currentBoost.Active)
            {
                currentBoost.Active = false;
                User.RemoveBoost(currentBoost, this);
            }

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", EffectId }
                });
            }
        }
    }
}
