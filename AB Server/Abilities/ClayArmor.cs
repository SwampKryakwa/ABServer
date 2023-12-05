using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class ClayArmorEffect : INegatable
    {
        public int TypeID { get; }
        Bakugan user;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return user.Owner;
        }

        public ClayArmorEffect(Bakugan user, Game game, int typeID)
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
                    { "Card", 5 },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            game.NegatableAbilities.Add(this);
            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;

            game.GateAdded += Trigger;
            user.affectingEffects.Add(this);
        }

        public void Trigger(IGateCard target, ushort owner, int pos)
        {
            user.Boost(100);
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

                game.GateAdded -= Trigger;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter) counterNegated = true;
            else if (user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;

                game.GateAdded -= Trigger;
            }
            game.NegatableAbilities.Remove(this);
        }
    }

    internal class ClayArmor : AbilityCard, IAbilityCard
    {

        public ClayArmor(int cID, Player owner)
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
                { "Ability", 5 },
                { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.Position >= 0 && x.Owner == owner && x.Attribute == Attribute.Subterra && !x.usedAbilityThisTurn).Select(x =>
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
            var effect = new ClayArmorEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["bakugan"]], game, 0);

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
            return game.BakuganIndex.Any(x => x.Position >= 0 && x.Owner == owner && x.Attribute == Attribute.Pyrus && !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 5;
        }
    }
}
