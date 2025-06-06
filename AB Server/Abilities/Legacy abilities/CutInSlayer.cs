using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class CutInSlayer : AbilityCard
    {
        public CutInSlayer(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.IsEnemyOf(User) && target.OnField()}
            ];
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() && x != User);

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, validBakugans)
            ));

            Game.OnAnswer[Owner.Id] = Setup3;
        }

        private List<Bakugan> otherBakugans = new();

        public void Setup3()
        {
            var selectedBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            otherBakugans.Add(selectedBakugan);

            var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() && x != User && !otherBakugans.Contains(x));

            if (validBakugans.Any())
            {
                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.BoolSelectionEvent("INFO_SELECTMORE")
                ));

                Game.OnAnswer[Owner.Id] = HandleAnotherBakuganSelection;
            }
            else
            {
                Activate();
            }
        }

        public void HandleAnotherBakuganSelection()
        {
            if ((bool)Game.IncomingSelection[Owner.Id]["array"][0]["answer"])
            {
                var validBakugans = Owner.BakuganOwned.Where(x => x.OnField() && x != User && !otherBakugans.Contains(x));

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, validBakugans)
                ));

                Game.OnAnswer[Owner.Id] = Setup3;
            }
            else
            {
                Activate();
            }
        }

        private Bakugan target;

        public new void Activate()
        {
            target = User;

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
                new CutInSlayerEffect(User, target, otherBakugans, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void TriggerEffect() =>
            new CutInSlayerEffect(User, target, otherBakugans, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
            if (otherBakugans.Contains(bakugan))
                otherBakugans.Remove(bakugan);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.Type == BakuganType.Tigress && user.OnField() && Game.BakuganIndex.Any(x => x.Owner == Owner && x.InBattle);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Type == BakuganType.Tigress && user.OnField() && user.Game.BakuganIndex.Any(x => x.Owner == user.Owner && x.InBattle);
    }

    internal class CutInSlayerEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        List<Bakugan> otherBakugans;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public CutInSlayerEffect(Bakugan user, Bakugan target, List<Bakugan> otherBakugans, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            this.otherBakugans = otherBakugans;
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
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            // Increase the power of the target Bakugan by the sum of the power of the other Bakugans
            int totalPower = otherBakugans.Sum(b => b.Power);
            target.Boost(new Boost((short)totalPower), this);

            // Move the other Bakugans to the drop
            foreach (var bakugan in otherBakugans)
            {
                if (bakugan.Position is GateCard positionGate)
                    bakugan.DestroyOnField(positionGate.EnterOrder);
                else if (bakugan.Position is Player player)
                    bakugan.DestroyInHand();
            }
        }
    }
}


