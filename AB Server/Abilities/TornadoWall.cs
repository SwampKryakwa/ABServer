﻿using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class TornadoWallEffect : INegatable
    {
        public int TypeID { get; }
        public Bakugan User;
        Bakugan target;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public TornadoWallEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
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
                    { "Card", 17 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            foreach (Bakugan b in game.BakuganIndex.Where(x=>x.Position >= 0))
            {
                if (b.Owner == User.Owner & b.Attribute == Attribute.Ventus)
                {
                    b.Boost(80);
                    b.affectingEffects.Add(this);
                    break;
                }
                if (b.Owner.SideID != User.Owner.SideID)
                {
                    b.Boost(-80);
                    b.affectingEffects.Add(this);
                }
            }

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User & User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter) counterNegated = true;
            else if (User.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
                foreach (Bakugan b in game.BakuganIndex)
                {
                    if (b.Owner == User.Owner & b.affectingEffects.Contains(this))
                    {
                        b.Boost(-80);
                        b.affectingEffects.Remove(this);
                        return;
                    }
                    if (b.Owner.SideID != User.Owner.SideID & b.affectingEffects.Contains(this))
                    {
                        b.Boost(80);
                        b.affectingEffects.Remove(this);
                    }
                }
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
            if (leaver.affectingEffects.Contains(this))
            {
                leaver.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class TornadoWall : AbilityCard, IAbilityCard
    {
        public TornadoWall(int cID, Player owner)
        {
            CID = cID;
            this.owner = owner;
            game = owner.game;
        }

        public new void Activate()
        {
            game.NewEvents[owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_boost_target" },
                        { "Ability", 17 },
                        { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => (x.Position >= 0 || (x.Position < 0 & x.InHands)) & x.Owner == owner & x.Attribute == Attribute.Ventus & !x.usedAbilityThisTurn).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                }
                }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new TornadoWallEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][0]["bakugan"]], game, 1);

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
            return game.BakuganIndex.Any(x => (x.Position >= 0 || (x.Position < 0 & x.InHands)) & x.Owner == owner & x.Attribute == Attribute.Ventus & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 17;
        }
    }
}
