using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class FireTornadoEffect : INegatable
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

        public FireTornadoEffect(Bakugan user, Bakugan target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 1 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            User.Boost(100);
            target.Boost(-100);

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;

            User.affectingEffects.Add(this);
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
                game.BakuganPowerReset -= ResetTurnover;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter) counterNegated = true;
            else if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
                User.Boost(-100);
                target.Boost(100);
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
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class FireTornado : AbilityCard, IAbilityCard
    {
        public FireTornado(int cID, Player owner)
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
                { "Count", 2 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_boost_target" },
                        { "Ability", 1 },
                        { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.InBattle && x.Position >= 0 && x.Owner == owner && x.Attribute == Attribute.Pyrus && !x.usedAbilityThisTurn).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "B?" },
                        { "Message", "ability_deboost_target" },
                        { "Ability", 1 },
                        { "SelectionRange", "SGE" },
                        { "CompareTo", 0 }
                    } } 
                }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new FireTornadoEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][0]["bakugan"]], game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][1]["bakugan"]], game, 1);

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
            return game.BakuganIndex.Any(x => x.InBattle && x.Position >= 0 && x.Owner == owner && x.Attribute == Attribute.Pyrus && !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 1;
        }
    }
}
