using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class LightSpiralEffect : INegatable
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        IAbilityCard card;

        public Player Owner { get => User.Owner; }

        public LightSpiralEffect(Bakugan user, Game game, int typeID, IAbilityCard card)
        {
            this.User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
            this.card = card;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 9 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User.Boost(50, this);

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;
            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;

            game.TurnEnd += Trigger;
            User.affectingEffects.Add(this);
        }

        public void Trigger()
        {
            User.Owner.AbilityGrave.Remove(card);
            User.Owner.AbilityHand.Add(card);
            game.TurnEnd -= Trigger;
        }

        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            game.NegatableAbilities.Remove(this);
            game.TurnEnd -= NegatabilityTurnover;
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;

                game.TurnEnd -= Trigger;
            }
        }

        //remove when negated
        public void Negate()
        {
            User.Boost(-50, this);

            if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;

                game.TurnEnd -= Trigger;
            }
            game.NegatableAbilities.Remove(this);
        }
    }

    internal class LightSpiral : AbilityCard, IAbilityCard
    {

        public LightSpiral(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new LightSpiralEffect(User, Game, 0, this).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Lumina;

        public new int TypeId { get; private protected set; } = 9;
    }
}
