using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class AirBattleEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public AirBattleEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
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
                    { "Kind", 0 },
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

            // Perform the attack
            if (target.Power < User.Power)
            {
                if (target.Position is GateCard positionGate)
                {
                    target.DestroyOnField(positionGate.EnterOrder);
                }
            }
        }
    }

    internal class AirBattle : AbilityCard
    {
        public AirBattle(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
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
                        { "Message", "INFO_SELECT_OPPONENT_BAKUGAN" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.Owner.SideID != Owner.SideID && x.OnField() && IsAdjacent(x.Position, User.Position)).Select(x =>
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

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        private Bakugan target;

        public new void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new AirBattleEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new AirBattleEffect(User, target, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Attribute == Attribute.Zephyros && user.OnField() && Game.BakuganIndex.Any(x => x.Owner.SideID != Owner.SideID && x.OnField() && IsAdjacent(x.Position, user.Position));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Attribute == Attribute.Zephyros && user.OnField() && user.Game.BakuganIndex.Any(x => x.OnField() && IsAdjacent(x.Position, user.Position) && user.IsEnemyOf(x));

        private static bool IsAdjacent(IBakuganContainer pos1, IBakuganContainer pos2)
        {
            if (pos1 is GateCard gate1 && pos2 is GateCard gate2)
            {
                return GateCard.AreTouching(gate1, gate2);
            }
            return false;
        }
    }
}
