using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BurstReturnEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; }

        public BurstReturnEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

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

            User.Revive();

            foreach (var bakugan in game.BakuganIndex.Where(x => x.OnField()))
                bakugan.Boost(new Boost(-50), this);
        }
    }

    internal class BurstReturn : AbilityCard, IAbilityCard
    {
        public BurstReturn(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new BurstReturnEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new BurstReturnEffect(User, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Raptor && user.InGrave();
    }
}
