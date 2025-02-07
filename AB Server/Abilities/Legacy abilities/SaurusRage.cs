using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SaurusRageEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public SaurusRageEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
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
                    { "Type", "AbilityActivateEffect" },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            User.Boost(new Boost(200), this);
        }
    }

    internal class SaurusRage : AbilityCard
    {
        public SaurusRage(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new SaurusRageEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new SaurusRageEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && user.Type == BakuganType.Saurus && user.Power < user.BasePower;


    }
}
