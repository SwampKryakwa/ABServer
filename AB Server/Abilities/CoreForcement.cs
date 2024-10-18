using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class CoreForcementEffect : IActive
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        Boost currentBoost;
        int effectId;

        public Player Owner { get => User.Owner; }

        public CoreForcementEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
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
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectAddedActiveZone" },
                    { "Card", TypeId },
                    { "Id", effectId },
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

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", effectId }
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
            {
                effect = new CoreForcementEffect(User, Game, TypeId);
                effect.Activate();
            }
			
			Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Garrison && user.OnField();
    }
}
