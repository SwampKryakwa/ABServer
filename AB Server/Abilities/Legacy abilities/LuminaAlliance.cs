using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks.Dataflow;

namespace AB_Server.Abilities
{
    internal class LuminaAllianceEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan target;
        int power;
        Game game;

        public Player Owner { get => User.Owner; }
        bool IsCopy;

        public LuminaAllianceEffect(Bakugan user, Bakugan target, int power, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            this.target = target;
            this.power = power;
            Console.WriteLine(user);
            Console.WriteLine(user.Position);
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User.Boost(new Boost(-power), this);
            target.Boost(new Boost(+power), this);
        }
    }

    internal class LuminaAlliance : AbilityCard
    {
        public LuminaAlliance(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
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
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.OnField() && x != User).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Setup3;
        }

        Bakugan target;

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "PW" },
                        { "Message", "INFO_AMOUNTTRANSFER" },
                        { "MinPower", 0 },
                        { "MaxPower", User.Power }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(AbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.OnField() && x != User).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Setup3;
        }

        int boost;

        public void Activate()
        {
            boost = (int)Game.IncomingSelection[Owner.Id]["array"][0]["power"];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new LuminaAllianceEffect(User, target, boost, Game, TypeId, IsCopy).Activate();
            Dispose();
        }

        public new void DoubleEffect() =>
                new LuminaAllianceEffect(User, target, boost, Game, TypeId, IsCopy).Activate();

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Lumina && user.Type == BakuganType.Garrison && user.OnField();

        public static bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x != user && x.OnField() && x.Position != user.Position && x.Owner.SideID == user.Owner.SideID);
    }
}
