using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class ShiningBrillianceEffect : INegatable
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;


        public Player Owner { get => User.Owner; }

        public ShiningBrillianceEffect(Bakugan user, Game game, int typeID)
        {
            this.User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 12 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            foreach (Bakugan b in game.BakuganIndex.Where(x => x.OnField() && x.Owner == User.Owner && x.Attribute == Attribute.Haos))
            {
                b.PermaBoost(50, this);
                User.affectingEffects.Add(this);
            }

            game.NegatableAbilities.Add(this);

        }

        //remove when negated
        public void Negate()
        {
            game.NegatableAbilities.Remove(this);
            if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                User.PermaBoost(-50, this);
            }
        }
    }

    internal class ShiningBrilliance : AbilityCard, IAbilityCard
    {
        public ShiningBrilliance(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new ShiningBrillianceEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Haos;

        public new int TypeId { get; private protected set; } = 12;
    }
}
