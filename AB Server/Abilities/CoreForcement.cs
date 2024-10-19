using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class CoreForcementEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        Bakugan User;
        Game game;
        Boost currentBoost;

        public Player Owner { get => User.Owner; }

        public CoreForcementEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
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

            currentBoost = new Boost(150);
            User.Boost(currentBoost, this);

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;

            User.affectingEffects.Add(this);
        }

        private void OnBakuganLeaveField(Bakugan target, ushort owner)
        {
            if (target == User)
            {
                currentBoost = new Boost(150);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            User.affectingEffects.Remove(this);
            game.ActiveZone.Remove(this);

            if (currentBoost.Active)
            {
                currentBoost.Active = false;
                User.RemoveBoost(currentBoost, this);
            }

            game.BakuganDestroyed -= OnBakuganLeaveField;
            game.BakuganReturned -= OnBakuganLeaveField;

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

    internal class CoreForcement : AbilityCard, IAbilityCard
    {
        public CoreForcement(int cID, Player owner, int typeId)
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
                new CoreForcementEffect(User, Game, TypeId).Activate();
			
			Dispose();
        }

        public new void DoubleEffect() =>
                new CoreForcementEffect(User, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Garrison && user.OnField();
    }
}
