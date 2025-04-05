using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SaurusGlow : AbilityCard
    {
        public SaurusGlow(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new SaurusGlowEffect(User, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new SaurusGlowEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.OnField();
    }

    internal class SaurusGlowEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public CardKind Kind { get; } = CardKind.NormalAbility;
        public Bakugan User { get; set; }
        Game game { get => User.Game; }
        Boost currentBoost;

        public Player Owner { get; set; }
        bool IsCopy;

        public SaurusGlowEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
             this.IsCopy = IsCopy; Owner = user.Owner;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            game.ActiveZone.Add(this);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
                game.NewEvents[i].Add(EventBuilder.AddEffectToActiveZone(this, IsCopy));
            }

            game.BakuganAdded += OnBakuganAdded;
        }

        private void OnBakuganAdded(Bakugan target, byte owner, IBakuganContainer pos)
        {
            if (User.OnField() && target.Power > User.Power)
            {
                currentBoost = new Boost(50);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganAdded -= OnBakuganAdded;

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
