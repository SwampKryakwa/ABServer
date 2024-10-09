﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class CyclingMadnessEffect : INegatable
    {
        public int TypeId { get; }
        Bakugan User;
        Game Game;
        IAbilityCard Card;

        public Player Owner { get => User.Owner; }

        public CyclingMadnessEffect(Bakugan user, Game game, int typeID, IAbilityCard card)
        {
            User = user;
            Game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
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
        public void Negate()
        {
            User.Boost(-80, this);

            if (User.affectingEffects.Contains(this))
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
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new CyclingMadnessEffect(User, Game, TypeId, this).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Darkus;

        public new int TypeId { get; private protected set; } = 14;
    }
}
