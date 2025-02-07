using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SaurusGlowEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public AbilityKind Kind { get; } = AbilityKind.NormalAbility;
        Bakugan User;
        Game game;

        public Player Owner { get; set; }
        bool IsCopy;

        public SaurusGlowEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            this.IsCopy = IsCopy;
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
                    { "Type", "AbilityActivateEffect" }, { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectAddedActiveZone" },
                    { "IsCopy", IsCopy },
                    { "Card", TypeId },
                    { "Kind", (int)Kind },
                    { "Id", EffectId },
                    { "Owner", Owner.Id }
                });
            }

            game.BakuganThrown += OnBakuganEnteredField;
            game.BakuganAdded += OnBakuganEnteredField;
        }

        private void OnBakuganEnteredField(Bakugan bakugan, byte owner, IBakuganContainer pos)
        {
            if (bakugan.Power > User.Power && User.OnField())
            {
                Boost currentBoost = new Boost(50);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganThrown -= OnBakuganEnteredField;
            game.BakuganAdded -= OnBakuganEnteredField;
        }
    }

    internal class SaurusGlow : AbilityCard, IAbilityCard
    {
        public SaurusGlow(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new SaurusGlowEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
            new SaurusGlowEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Saurus && user.OnField();
    }
}