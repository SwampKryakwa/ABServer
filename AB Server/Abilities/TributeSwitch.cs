using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TributeSwitchEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;

        public Player Owner { get => User.Owner; }

        public TributeSwitchEffect(Bakugan user, Bakugan target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", TypeID },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            target.AddFromHand(User.Position as GateCard);
            User.Destroy(User.Position.EnterOrder);
        }
    }

    internal class TributeSwitch : AbilityCard, IAbilityCard
    {
        public TributeSwitch(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asCounter)
        {
            IAbilityCard ability = this;
            
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
                        )) } },
                    new JObject {
                        { "SelectionType", "BH" },
                        { "Message", "INFO_ADDTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Owner.BakuganGrave.Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;

            
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BH" },
                        { "Message", "INFO_ADDTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Owner.BakuganGrave.Select(x =>
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

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        private Bakugan target;

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][1]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new TributeSwitchEffect(User, target, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Lumina && user.Type == BakuganType.Griffon && user.Owner.BakuganGrave.Bakugans.Count > 0;
    }
}
