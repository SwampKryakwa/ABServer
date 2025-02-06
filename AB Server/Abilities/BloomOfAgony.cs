using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BloomOfAgonyEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game;
        Boost boost;

        public Player Owner { get; set; }
        bool IsCopy;

        public BloomOfAgonyEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            this.user = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" }, { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            foreach (var bakugan in game.BakuganIndex)
            {
                if (bakugan.OnField())
                {
                    bakugan.Boost(new Boost(-200), this);
                }
            }
        }
    }

    internal class BloomOfAgony : AbilityCard
    {
        public BloomOfAgony(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Resolve()
        {
            if (!counterNegated || Fusion != null)
                new BloomOfAgonyEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new BloomOfAgonyEffect(User, Game, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.OnField() && user.Attribute == Attribute.Darkon;
    }
}
