using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class CyclingMadnessEffect : INegatable
    {
        public int TypeID { get; }
        Bakugan User;
        Game Game;
        bool counterNegated = false;
        IAbilityCard Card;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public CyclingMadnessEffect(Bakugan user, Game game, int typeID, IAbilityCard card)
        {
            User = user;
            Game = game;
            user.UsedAbilityThisTurn = true;
            TypeID = typeID;
            Card = card;
        }

        public void Activate()
        {
            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 14 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User.Boost(80, this);

            Game.NegatableAbilities.Add(this);
            Game.TurnEnd += NegatabilityTurnover;
            Game.BakuganReturned += FieldLeaveTurnover;
            Game.BakuganDestroyed += FieldLeaveTurnover;
            
            Game.BakuganDestroyed += Trigger;
            User.affectingEffects.Add(this);
        }

        public void Trigger(Bakugan target, ushort owner)
        {
            User.Owner.AbilityGrave.Remove(Card);
            User.Owner.AbilityHand.Add(Card);
            Game.BakuganDestroyed -= Trigger;
        }

        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            Game.NegatableAbilities.Remove(this);
            Game.TurnEnd -= NegatabilityTurnover;
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                Game.BakuganReturned -= FieldLeaveTurnover;
                Game.BakuganDestroyed -= FieldLeaveTurnover;

                Game.BakuganDestroyed -= Trigger;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            User.Boost(-80, this);

            if (asCounter) counterNegated = true;
            else if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                Game.BakuganReturned -= FieldLeaveTurnover;
                Game.BakuganDestroyed -= FieldLeaveTurnover;

                Game.BakuganDestroyed -= Trigger;
            }
            Game.NegatableAbilities.Remove(this);
        }
    }

    internal class CyclingMadness : AbilityCard, IAbilityCard
    {
        public CyclingMadness(int cID, Player owner)
        {
            CID = cID;
            Owner = owner;
            Game = owner.game;
            BakuganIsValid = x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Darkus && !x.UsedAbilityThisTurn;
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_user" },
                { "Ability", 14 },
                { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
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
            var effect = new CyclingMadnessEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["bakugan"]], Game, 0, this);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter() => Activate();

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new int GetTypeID() => 14;
    }
}
