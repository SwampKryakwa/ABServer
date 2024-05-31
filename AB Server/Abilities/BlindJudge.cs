using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class BlindJudgeEffect : INegatable
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan Target;
        IGateCard battle;
        Game Game;
        short Boost;
        bool CounterNegated = false;
        IAbilityCard Card;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public BlindJudgeEffect(Bakugan user, Bakugan target, Game game, int typeID, IAbilityCard card)
        {
            User = user;
            Game = game;
            Target = target;
            battle = game.GateIndex.First(x => x.Bakugans.Contains(user));
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
                    { "Card", 19 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            Boost = (short)(Game.BakuganIndex.Count(x=>x.Attribute == Attribute.Ventus && x.Owner == User.Owner) * -100);

            Target.Boost(Boost, this);

            Game.NegatableAbilities.Add(this);
            Game.TurnEnd += NegatabilityTurnover;
            Game.BakuganReturned += FieldLeaveTurnover;
            Game.BakuganDestroyed += FieldLeaveTurnover;
            
            Game.BattleOver += Trigger;
            Target.affectingEffects.Add(this);
        }

        public void Trigger(IGateCard target, ushort winner)
        {
            if (battle == target)
            {
                User.Owner.AbilityGrave.Remove(Card);
                User.Owner.AbilityHand.Add(Card);
                Game.BattleOver -= Trigger;
            }
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
            if (leaver == Target && Target.affectingEffects.Contains(this))
            {
                Target.affectingEffects.Remove(this);
                Game.BakuganReturned -= FieldLeaveTurnover;
                Game.BakuganDestroyed -= FieldLeaveTurnover;

                Game.BattleOver -= Trigger;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            Target.Boost((short)-Boost, this);

            if (asCounter) CounterNegated = true;
            else if (Target.affectingEffects.Contains(this))
            {
                Target.affectingEffects.Remove(this);
                Game.BakuganReturned -= FieldLeaveTurnover;
                Game.BakuganDestroyed -= FieldLeaveTurnover;

                Game.BattleOver -= Trigger;
            }
            Game.NegatableAbilities.Remove(this);
        }
    }

    internal class BlindJudge : AbilityCard, IAbilityCard
    {

        public BlindJudge(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BakuganIsValid = x => x.InBattle && x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Ventus && !x.UsedAbilityThisTurn;
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
                        { "Ability", 19 },
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
                        { "SelectionType", "B?" },
                        { "Message", "ability_deboost_target" },
                        { "Ability", 19 },
                        { "SelectionRange", "SGE" },
                        { "CompareTo", 0 }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new BlindJudgeEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]], Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][1]["bakugan"]], Game, 0, this);

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

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable(false);
        }

        public new int GetTypeID()
        {
            return 19;
        }
    }
}
