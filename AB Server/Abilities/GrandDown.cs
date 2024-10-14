using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class GrandDownEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        IGateCard target;
        Game game;


        public Player Owner { get => User.Owner; }

        public GrandDownEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            target = User.Position as IGateCard;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 2 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            if (target.IsOpen)
                target.Negate();
        }

        //remove when negated
        public void Negate() { }
    }

    internal class GrandDown : AbilityCard, IAbilityCard
    {
        public GrandDown(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new GrandDownEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) => user.OnField() && user.Attribute == Attribute.Darkon && user.InBattle;

        public new int TypeId { get; private protected set; } = 2;
    }
}
