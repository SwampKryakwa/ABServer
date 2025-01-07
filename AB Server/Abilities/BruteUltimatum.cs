using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BruteUltimatumEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;


        public Player Owner { get => User.Owner; }
        bool IsCopy;

        public BruteUltimatumEffect(Bakugan user, Bakugan target, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
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

            if (target.InHands && User.Position is GateCard positionGate)
                target.AddFromHand(positionGate);
        }
    }

    internal class BruteUltimatum : AbilityCard
    {
        public BruteUltimatum(int cID, Player owner, int typeId)
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

        Player target;

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            if (Game.Players.Count(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID) > 1)
            {
                Game.NewEvents[Owner.Id].Add(new JObject
                {
                    { "Type", "StartSelection" },
                    { "Selections", new JArray {
                        new JObject {
                            { "SelectionType", "P" },
                            { "Message", "INFO_PLAYERTARGET" },
                            { "Ability", TypeId },
                            { "SelectionPlayers", new JArray(Game.Players.Where(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID).Select(x =>
                                new JObject {
                                    { "Nickname", x.DisplayName },
                                    { "Side", x.SideID },
                                    { "PID", x.Id }
                                }
                            )) }
                        }
                    } }
                });

                Game.AwaitingAnswers[Owner.Id] = Setup3;
            }
            else
            {
                target = Game.Players.First(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID);

                Game.NewEvents[target.Id].Add(new JObject
                {
                    { "Type", "StartSelection" },
                    { "Selections", new JArray {
                        new JObject {
                            { "SelectionType", "BH" },
                            { "Message", "INFO_ABILITYUSER" },
                            { "Ability", TypeId },
                            { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.Owner == target && x.InHand()).Select(x =>
                                new JObject {
                                    { "Type", (int)x.Type },
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
                Game.NewEvents[Owner.Id].Add(new JObject { { "Type", "OtherPlayerSelects" }, { "PID", target.Id } });

                Game.AwaitingAnswers[target.Id] = Activate;
            }
        }

        public void Setup3()
        {
            target = Game.Players[(int)Game.IncomingSelection[Owner.Id]["array"][0]["player"]];

            Game.NewEvents[target.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BH" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.Owner == target && x.InHand()).Select(x =>
                            new JObject {
                                { "Type", (int)x.Type },
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
            Game.NewEvents[Owner.Id].Add(new JObject { { "Type", "OtherPlayerSelects" }, { "PID", target.Id } });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public override void SetupFusion(AbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            if (Game.Players.Count(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID) > 1)
            {
                Game.NewEvents[Owner.Id].Add(new JObject
                {
                    { "Type", "StartSelection" },
                    { "Selections", new JArray {
                        new JObject {
                            { "SelectionType", "P" },
                            { "Message", "INFO_PLAYERTARGET" },
                            { "Ability", TypeId },
                            { "SelectionPlayers", new JArray(Game.Players.Where(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID).Select(x =>
                                new JObject {
                                    { "Nickname", x.DisplayName },
                                    { "Side", x.SideID },
                                    { "PID", x.Id }
                                }
                            )) }
                        }
                    } }
                });

                Game.AwaitingAnswers[Owner.Id] = Setup3;
            }
            else
            {
                target = Game.Players.First(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID);

                Game.NewEvents[target.Id].Add(new JObject
                {
                    { "Type", "StartSelection" },
                    { "Selections", new JArray {
                        new JObject {
                            { "SelectionType", "BH" },
                            { "Message", "INFO_ABILITYUSER" },
                            { "Ability", TypeId },
                            { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.Owner == target && x.InHand()).Select(x =>
                                new JObject {
                                    { "Type", (int)x.Type },
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
                Game.NewEvents[Owner.Id].Add(new JObject { { "Type", "OtherPlayerSelects" }, { "PID", target.Id } });

                Game.AwaitingAnswers[target.Id] = Activate;
            }
        }

        private Bakugan addTarget;

        public new void Activate()
        {
            addTarget = Game.BakuganIndex[(int)Game.IncomingSelection[target.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new BruteUltimatumEffect(User, addTarget, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new BruteUltimatumEffect(User, addTarget, Game, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (addTarget == bakugan)
                addTarget = Bakugan.GetDummy();
        }

        public override bool IsActivateableFusion(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.Type == BakuganType.Glorius && user.OnField() && Game.Players.Any(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.Players.Any(x => x.Bakugans.Count != 0 && x.SideID != user.Owner.SideID);
    }
}
