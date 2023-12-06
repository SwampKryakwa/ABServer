using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class BlindJudgeEffect : INegatable
    {
        public int TypeID { get; }
        Bakugan user;
        Bakugan target;
        IGateCard battle;
        Game game;
        short boost;
        bool counterNegated = false;
        IAbilityCard card;

        public Player GetOwner()
        {
            return user.Owner;
        }

        public BlindJudgeEffect(Bakugan user, Bakugan target, Game game, int typeID, IAbilityCard card)
        {
            this.user = user;
            this.game = game;
            this.target = target;
            battle = game.GateIndex.First(x => x.Bakugans.Contains(user));
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
            this.card = card;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 19 },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }
            boost = (short)(game.BakuganIndex.Count(x=>x.Attribute == Attribute.Ventus & x.Owner == user.Owner) * -100);

            target.Boost(boost);

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;
            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            
            game.BattleOver += Trigger;
            target.affectingEffects.Add(this);
        }

        public void Trigger(IGateCard target, ushort winner)
        {
            if (battle == target)
            {
                user.Owner.AbilityGrave.Remove(card);
                user.Owner.AbilityHand.Add(card);
                game.BattleOver -= Trigger;
            }
        }

        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            game.NegatableAbilities.Remove(this);
            game.TurnEnd -= NegatabilityTurnover;
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == target & target.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;

                game.BattleOver -= Trigger;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            target.Boost((short)-boost);

            if (asCounter) counterNegated = true;
            else if (target.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;

                game.BattleOver -= Trigger;
            }
            game.NegatableAbilities.Remove(this);
        }
    }

    internal class BlindJudge : AbilityCard, IAbilityCard
    {

        public BlindJudge(int cID, Player owner)
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
                        { "Ability", 19 },
                        { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.InBattle & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Ventus & !x.usedAbilityThisTurn).Select(x =>
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
                        { "Ability", 19 },
                        { "SelectionRange", "SGE" },
                        { "CompareTo", 0 }
                    } }
                }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new BlindJudgeEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][0]["bakugan"]], game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][1]["bakugan"]], game, 0, this);

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
            return game.BakuganIndex.Any(x => x.InBattle & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Ventus & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 19;
        }
    }
}
