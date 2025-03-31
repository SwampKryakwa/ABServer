using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;

namespace AB_Server.Abilities
{
    internal class BruteUltimatumEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }


        public Player Onwer { get; set; }
        bool IsCopy;

        public BruteUltimatumEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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
                    { "Type", "AbilityActivateEffect" }, { "Kind", 0 },
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
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

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

                Game.NewEvents[target.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.HandBakuganSelection("INFO_ABILITY_ADDTARGET", TypeId, (int)Kind, Game.BakuganIndex.Where(x => x.Owner == target && x.InHand()))
                ));

                Game.NewEvents[Owner.Id].Add(new JObject { { "Type", "OtherPlayerSelects" }, { "PID", target.Id } });

                Game.AwaitingAnswers[target.Id] = Activate;
            }
        }

        public void Setup3()
        {
            target = Game.Players[(int)Game.IncomingSelection[Owner.Id]["array"][0]["player"]];

            Game.NewEvents[target.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_ABILITY_ADDTARGET", TypeId, (int)Kind, Game.BakuganIndex.Where(x => x.Owner == target && x.InHand()))
            ));

            Game.NewEvents[Owner.Id].Add(new JObject { { "Type", "OtherPlayerSelects" }, { "PID", target.Id } });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        private Bakugan addTarget;

        public new void Activate()
        {
            addTarget = Game.BakuganIndex[(int)Game.IncomingSelection[target.Id]["array"][0]["bakugan"]];

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = IsCopy,
                    ["Id"] = EffectId,
                    ["Card"] = TypeId,
                    ["Kind"] = (int)Kind,
                    ["User"] = User.BID,
                    ["IsCounter"] = asCounter,
                    ["Owner"] = Owner.Id
                });
            }

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new BruteUltimatumEffect(User, addTarget, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new BruteUltimatumEffect(User, addTarget, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (addTarget == bakugan)
                addTarget = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.Type == BakuganType.Glorius && user.OnField() && user.JustEndedBattle && Game.Players.Any(x=>x.Bakugans.Count != 0 && x.SideID != Owner.SideID);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.Players.Any(x => x.Bakugans.Count != 0 && x.SideID != user.Owner.SideID);
    }
}
