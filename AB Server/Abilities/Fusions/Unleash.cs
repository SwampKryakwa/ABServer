using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Unleash : FusionAbility
    {
        public Unleash(int cID, Player owner)
        {
            TypeId = 0;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(AbilityCard);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new UnleashEffect(User, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new UnleashEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.IsPartner && Game.CurrentWindow == ActivationWindow.Normal;
    }

    internal class UnleashEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game { get => user.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public UnleashEffect(Bakugan user, int typeID, bool IsCopy)
        {
            this.user = user;
             this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Treatment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }
            user.Boost(new Boost(50), this);
        }
    }
}
