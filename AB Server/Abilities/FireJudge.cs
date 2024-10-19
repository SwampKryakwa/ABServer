using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireJudgeEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        Bakugan User;
        Game game;
        Boost currentBoost;

        public Player Owner { get => User.Owner; }

        public FireJudgeEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;
            game.ActiveZone.Add(this);

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
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectAddedActiveZone" },
                    { "Card", TypeId },
                    { "Id", EffectId },
                    { "Owner", Owner.Id }
                });
            }

            currentBoost = new Boost(100);
            User.Boost(currentBoost, this);

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;
        }

        private void OnBakuganLeaveField(Bakugan target, ushort owner)
        {
            if (target == User)
            {
                currentBoost = new Boost(100);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganDestroyed -= OnBakuganLeaveField;
            game.BakuganReturned -= OnBakuganLeaveField;

            if (currentBoost.Active)
            {
                currentBoost.Active = false;
                User.RemoveBoost(currentBoost, this);
            }

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", EffectId }
                });
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

        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new FireJudgeEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
            new FireJudgeEffect(User, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Nova && user.InBattle;
    }
}
