using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks.Dataflow;

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
        IAbilityCard Card;

        public Player Owner { get => User.Owner; }

        public BlindJudgeEffect(Bakugan user, Bakugan target, Game game, int typeID, IAbilityCard card)
        {
            User = user;
            Game = game;
            Target = target;
            Console.WriteLine(user);
            Console.WriteLine(user.Position);
            battle = (IGateCard)user.Position;
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
            Boost = (short)(Game.BakuganIndex.Count(x => x.Attribute == Attribute.Zephyros && x.Owner == User.Owner) * -100);

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
        public void Negate()
        {
            Target.Boost((short)-Boost, this);

            if (Target.affectingEffects.Contains(this))
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
        }

        public void Setup(bool asFusion)
        {
            IAbilityCard ability = this;
            
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_BOOSTTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Setup2;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;

            
            
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_DECREASETARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(User.Position.Bakugans.Where(x=>x.Owner.SideID != Owner.SideID).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];
            Console.WriteLine(User);
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_DECREASETARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(User.Position.Bakugans.Where(x=>x.Owner.SideID != Owner.SideID).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        Bakugan target;

        public void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new BlindJudgeEffect(User, target, Game, TypeId, this).Activate();
            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && user.OnField() && user.Attribute == Attribute.Zephyros;

        public new int TypeId { get; private protected set; } = 19;
    }
}
