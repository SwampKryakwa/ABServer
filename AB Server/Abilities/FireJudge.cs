using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireJudgeEffect : INegatable
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;


        public Player Owner { get => User.Owner; }

        public FireJudgeEffect(Bakugan user, Game game, int typeID)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
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
            User.PermaBoost(100, this);

            game.NegatableAbilities.Add(this);

            User.affectingEffects.Add(this);
        }

        //remove when negated
        public void Negate()
        {
            game.NegatableAbilities.Remove(this);
            if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                User.PermaBoost(-100, this);
            }
        }
    }

    internal class FireJudge : AbilityCard, IAbilityCard
    {
        public FireJudge(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new FireJudgeEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Nova && user.InBattle;

        public new int TypeId { get; private protected set; }
    }
}
