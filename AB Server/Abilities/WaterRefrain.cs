using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class WaterRefrainEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        int turnsPassed = 0;
        AbilityCard card;

        public Player Owner { get => User.Owner; }

        public WaterRefrainEffect(Bakugan user, Game game, int typeID, AbilityCard card)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
            this.card = card;
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
            game.Players.ForEach(p => p.AbilityBlockers.Add(this));

            game.TurnEnd += CheckEffectOver;

            User.affectingEffects.Add(this);
        }

        //is not negatable after turn ends
        public void CheckEffectOver()
        {
            if (turnsPassed++ == 1)
            {
                game.Players.ForEach(x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
                game.TurnEnd -= CheckEffectOver;
                card.Dispose();
            }
        }

        public void Negate()
        {
            game.Players.ForEach(x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
            game.TurnEnd -= CheckEffectOver;
            card.Dispose();
        }
    }

    internal class WaterRefrain : AbilityCard, IAbilityCard
    {
        WaterRefrainEffect effect;

        public WaterRefrain(int cID, Player owner, int typeId)
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
                effect.Negate();
        }

        public new void Resolve()
        {
            if (counterNegated)
                Dispose();
            else
            {
                effect = new WaterRefrainEffect(User, Game, TypeId, this);
                effect.Activate();
            }
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Aqua && user.OnField();
    }
}
