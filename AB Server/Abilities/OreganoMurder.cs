using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class ColourfulDeathEffect : INegatable
    {
        public int TypeId { get; }
        Bakugan user;
        Game game;
        short boost;

        public Player Owner { get => user.Owner; }

        public ColourfulDeathEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            boost = (short)(game.BakuganIndex.Count(x => x.OnField() && x != user) * 100);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 13 },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }
            user.Boost(boost, this);

            foreach (Bakugan b in game.BakuganIndex.Where(x => x.OnField() && x != user))
            {
                b.Boost(-100, this);
                b.affectingEffects.Add(this);
            }

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;

            user.affectingEffects.Add(this);
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == user && user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }

        //remove when negated
        public void Negate()
        {
            game.NegatableAbilities.Remove(this);
            if (user.affectingEffects.Contains(this))
            {
                game.BakuganIndex.ForEach(x => x.affectingEffects.Remove(this));
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
                user.Boost((short)-boost, this);
                foreach (Bakugan b in game.BakuganIndex.Where(x => x.affectingEffects.Contains(this))) b.Boost(100, this);
            }
        }

        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            game.NegatableAbilities.Remove(this);
            game.TurnEnd -= NegatabilityTurnover;
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class OreganoMurder : AbilityCard, IAbilityCard, INegatable
    {
        public new int TypeId { get; } = 13;

        public OreganoMurder(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new ColourfulDeathEffect(User, Game, TypeId).Activate();
            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && user.OnField() && user.Attribute == Attribute.Darkus;
    }
}
