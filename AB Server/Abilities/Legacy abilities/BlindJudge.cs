using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks.Dataflow;

namespace AB_Server.Abilities
{
    internal class BlindJudgeEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan Target;
        IGateCard battle;
        Game game;
        Boost boost;
        AbilityCard Card;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public BlindJudgeEffect(Bakugan user, Bakugan target, Game game, int typeID, AbilityCard card)
        {
            User = user;
            this.game = game;
            Target = target;
            Console.WriteLine(user);
            Console.WriteLine(user.Position);
            battle = (IGateCard)user.Position;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
            Card = card;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
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
            boost = new Boost(game.BakuganIndex.Count(x => x.Attribute == Attribute.Zephyros && x.Owner == User.Owner) * -100);

            Target.Boost(boost, this);

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;

            game.BattleOver += Trigger;
            Target.affectingEffects.Add(this);
        }

        public void Trigger(IGateCard target)
        {
            if (battle == target)
            {
                User.Owner.AbilityGrave.Remove(Card);
                User.Owner.AbilityHand.Add(Card);
                game.BattleOver -= Trigger;
            }
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == Target && Target.affectingEffects.Contains(this))
            {
                Target.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;

                game.BattleOver -= Trigger;
            }
        }
    }

    internal class BlindJudge : AbilityCard
    {
        public BlindJudge(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asFusion)
        {
            AbilityCard ability = this;

            Game.NewEvents[Owner.Id].Add(new JObject
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
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) } }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void SetupFusion(AbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
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
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            Console.WriteLine(User);
            Game.NewEvents[Owner.Id].Add(new JObject
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
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        Bakugan target;

        public void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new BlindJudgeEffect(User, target, Game, TypeId, this).Activate();
            Dispose();
        }

        public new void DoubleEffect() =>
            new BlindJudgeEffect(User, target, Game, TypeId, this).Activate();
    }
}
