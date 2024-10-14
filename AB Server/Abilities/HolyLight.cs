using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class HolyLightEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;


        public Player Owner { get => User.Owner; }

        public HolyLightEffect(Bakugan user, Bakugan target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true;
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
            target.Revive();
        }
    }

    internal class HolyLight : AbilityCard, IAbilityCard
    {
        public HolyLight(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asCounter)
        {
            IAbilityCard ability = this;

            Game.NewEvents[Owner.ID].Add(new JObject
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
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];
            IAbilityCard ability = this;

            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.Owner == Owner && x.OnField()).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;

            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.Owner == Owner && x.OnField()).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        private Bakugan target;

        public void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new HolyLightEffect(User, target, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Lumina && user.OnField();

        public new int TypeId { get; private protected set; }
    }
}
