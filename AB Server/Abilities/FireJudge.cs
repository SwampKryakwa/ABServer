using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireJudgeEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        Boost currentBoost;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public FireJudgeEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
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

            User.Boost(new Boost(100), this);
        }
    }

    internal class FireJudge : AbilityCard
    {
        public FireJudge(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new FireJudgeEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new FireJudgeEffect(User, Game, TypeId, IsCopy).Activate();

        public override bool IsActivateableFusion(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Attribute == Attribute.Nova && user.OnField();
    }
}
