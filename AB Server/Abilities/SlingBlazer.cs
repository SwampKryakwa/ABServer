using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SlingBlazerEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan target;
        GateCard moveTarget;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public SlingBlazerEffect(Bakugan user, Bakugan target, GateCard moveTarget, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            this.moveTarget = moveTarget;
            
            this.IsCopy = IsCopy;
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
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            target.Move(moveTarget);
        }
    }

    internal class SlingBlazer : AbilityCard
    {
        public SlingBlazer(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        Bakugan target;
        GateCard moveTarget;

        public override void Setup(bool asCounter)
		{
			this.asCounter = asCounter;
			Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validTargets = Game.BakuganIndex.Where(x => x.InBattle && x.Owner.SideID != Owner.SideID);

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_MOVETARGET", TypeId, (int)Kind, validTargets)
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup3;
        }

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var battleGate = User.Position as GateCard;
            var validGates = Game.GateIndex.Where(x => x.IsAdjacentHorizontally(battleGate));

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(validGates.Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Activate()
        {
            moveTarget = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];

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
                new SlingBlazerEffect(User, target, moveTarget, TypeId, IsCopy).Activate();
            Dispose();
        }

        public override void DoubleEffect() =>
            new SlingBlazerEffect(User, target, moveTarget, TypeId, IsCopy).Activate();

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.InBattle && Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsEnemyOf(possibleTarget));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsEnemyOf(possibleTarget));
    }
}
