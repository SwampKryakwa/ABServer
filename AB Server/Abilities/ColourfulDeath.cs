using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class ColourfulDeathEffect : INegatable
    {
        public int TypeID { get; }
        Bakugan user;
        Game game;
        short boost;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return user.Owner;
        }

        public ColourfulDeathEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
            this.game = game;
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            boost = (short)(game.BakuganIndex.Count(x => x.Position >= 0 & x != user) * 100);

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
            user.Boost(boost);

            foreach (Bakugan b in game.BakuganIndex.Where(x => x.Position >= 0 & x != user))
            {
                b.Boost(-100);
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
            if (leaver == user & user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            game.NegatableAbilities.Remove(this);
            if (asCounter) counterNegated = true;
            else if (user.affectingEffects.Contains(this))
            {
                game.BakuganIndex.ForEach(x => x.affectingEffects.Remove(this));
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
                user.Boost((short)-boost);
                foreach (Bakugan b in game.BakuganIndex.Where(x => x.affectingEffects.Contains(this))) b.Boost(100);
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

    internal class ColourfulDeath : AbilityCard, IAbilityCard
    {

        public ColourfulDeath(int cID, Player owner)
        {
            CID = cID;
            this.owner = owner;
            game = owner.game;
        }

        public new void Activate()
        {
            game.NewEvents[owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_boost_target" },
                { "Ability", 13 },
                { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.InBattle & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Darkus & !x.usedAbilityThisTurn).Select(x =>
                    new JObject { { "Type", (int)x.Type },
                        { "Attribute", (int)x.Attribute },
                        { "Treatment", (int)x.Treatment },
                        { "Power", x.Power },
                        { "Owner", x.Owner.ID },
                        { "BID", x.BID }
                    }
                )) }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new ColourfulDeathEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["bakugan"]], game, 0);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion()
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return game.BakuganIndex.Any(x => x.InBattle & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Darkus & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 13;
        }
    }
}
