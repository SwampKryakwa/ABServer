using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class SpiritCanyonEffect : INegatable
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

        public SpiritCanyonEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            int team = user.Owner.SideID;
            boost = (short)(game.GateIndex.Count(x => game.Field.Cast<IGateCard>().Contains(x)) * 50);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 8 },
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
        public void Negate(bool asCounter)
        {
            game.NegatableAbilities.Remove(this);
            if (asCounter) counterNegated = true;
            else if (user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
                user.Boost((short)-boost, this);
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
            if (leaver == user && user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class SpiritCanyon : AbilityCard, IAbilityCard
    {

        public SpiritCanyon(int cID, Player owner)
        {
            CID = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_boost_target" },
                { "Ability", 8 },
                { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Subterra && !x.UsedAbilityThisTurn).Select(x =>
                    new JObject { { "Type", (int)x.Type },
                        { "Attribute", (int)x.Attribute },
                        { "Treatment", (int)x.Treatment },
                        { "Power", x.Power },
                        { "Owner", x.Owner.ID },
                        { "BID", x.BID }
                    }
                )) }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new SpiritCanyonEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["bakugan"]], Game, 0);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return Game.BakuganIndex.Any(x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Subterra && !x.UsedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 8;
        }
    }
}
