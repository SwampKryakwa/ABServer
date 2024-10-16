using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireJudgeEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        Boost currentBoost;

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
            currentBoost = new Boost(100);
            User.Boost(currentBoost, this);

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;

            User.affectingEffects.Add(this);
        }

        private void OnBakuganLeaveField(Bakugan target, ushort owner)
        {
            if (target == User)
            {
                currentBoost = new Boost(100);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(AbilityCard card)
        {
            User.affectingEffects.Remove(this);

            if (currentBoost.Active)
            {
                currentBoost.Active = false;
                User.RemoveBoost(currentBoost, this);
            }

            card.Dispose();
        }
    }

    internal class FireJudge : AbilityCard, IAbilityCard
    {
        FireJudgeEffect effect;

        public FireJudge(int cID, Player owner, int typeId)
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
            else
                effect.Negate(this);
        }

        public new void Resolve()
        {
            if (counterNegated)
                Dispose();
            else
            {
                effect = new FireJudgeEffect(User, Game, TypeId);
                effect.Activate();
            }
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Nova && user.InBattle;
    }
}
