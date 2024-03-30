using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class ChainsDesEffect : INegatable
    {
        public int TypeID { get; }
        public Bakugan User;
        Bakugan target;
        Game game;
        short boost;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public ChainsDesEffect(Bakugan user, Bakugan target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            boost = (short)(game.BakuganIndex.Count(x => x.OnField() && !x.InHands) * -100);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 15 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            target.Boost(boost, this);

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;

            target.affectingEffects.Add(this);
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
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
                target.Boost((short)-boost, this);
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
            if (leaver == target && target.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class ChainsDes : AbilityCard, IAbilityCard
    {
        public ChainsDes(int cID, Player owner)
        {
            CID = cID;
            Owner = owner;
            Game = owner.game;
            BakuganIsValid = x => !x.InHands && x.Owner == Owner && x.Attribute == Attribute.Darkus && !x.UsedAbilityThisTurn;
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 2 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_boost_target" },
                        { "Ability", 15 },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_deboost_target" },
                        { "Ability", 15 },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.OnField()).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new ChainsDesEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]], Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][1]["bakugan"]], Game, 1);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter() => Activate();

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new int GetTypeID()
        {
            return 15;
        }
    }
}
