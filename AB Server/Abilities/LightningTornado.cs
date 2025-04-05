using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class LightningTornado : AbilityCard
    {
        public LightningTornado(int cID, Player owner, int typeId)
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

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = User.Position.Bakugans.Where(x => User.IsEnemyOf(x) && x.Power > User.Power);
            if (validBakugans.Count() != 0)
            {
                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                ));
            }
            else
            {
                Activate();
            }
            Game.AwaitingAnswers[Owner.Id] = HandleOpponentBakuganSelection;
        }

        public void HandleOpponentBakuganSelection()
        {
            if ((bool)Game.IncomingSelection[Owner.Id]["array"][0]["answer"])
            {
                var validBakugans = Game.BakuganIndex.Where(x => x.Position == User.Position && x.Owner != Owner && x.Power > User.Power);

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_ABILITY_DECREASETARGET", TypeId, (int)Kind, validBakugans)
                ));

                Game.AwaitingAnswers[Owner.Id] = Setup3;
            }
            else
            {
                Activate();
            }
        }

        private Bakugan target;

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            Activate();
        }

        public new void Activate()
        {

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
                new LightningTornadoEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new LightningTornadoEffect(User, target, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Attribute == Attribute.Lumina && user.InBattle;

        public static new bool HasValidTargets(Bakugan user) =>
            true;
    }

    internal class LightningTornadoEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public LightningTornadoEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            
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

            // Increase the power of the user Bakugan by 100G
            User.Boost(new Boost(100), this);

            // If a target Bakugan is selected, decrease its power by 100G
            if (target != null)
            {
                target.Boost(new Boost(-100), this);
            }
        }
    }
}
