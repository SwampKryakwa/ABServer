using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class ShiningBrillianceEffect : INegatable
    {
        public int TypeID { get; }
        Bakugan user;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return user.Owner;
        }

        public ShiningBrillianceEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
            this.game = game;
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 12 },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            foreach (Bakugan b in game.BakuganIndex.Where(x => x.Position >= 0 & x.Owner == user.Owner && x.Attribute == Attribute.Haos))
            {
                b.PermaBoost(50);
                user.affectingEffects.Add(this);
            }

            game.NegatableAbilities.Add(this);

        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            game.NegatableAbilities.Remove(this);
            if (asCounter) counterNegated = true;
            else if (user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                user.PermaBoost(-50);
            }
        }
    }

    internal class ShiningBrilliance : AbilityCard, IAbilityCard
    {

        public ShiningBrilliance(int cID, Player owner)
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
                { "Message", "ability_user" },
                { "Ability", 12 },
                { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Haos & !x.usedAbilityThisTurn).Select(x =>
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
            var effect = new ShiningBrillianceEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["bakugan"]], game, 0);

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
            return game.BakuganIndex.Any(x => x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Haos & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 12;
        }
    }
}
