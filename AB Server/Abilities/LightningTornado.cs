using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class LightningTornado : AbilityCard
    {
        public LightningTornado(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        { }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.OnAnswer[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = User.Position.Bakugans.Where(x => x.Position == User.Position && User.IsEnemyOf(x) && x.BasePower > User.BasePower);
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
            Game.OnAnswer[Owner.Id] = HandleOpponentBakuganSelection;
        }

        public void HandleOpponentBakuganSelection()
        {
            if ((bool)Game.IncomingSelection[Owner.Id]["array"][0]["answer"])
            {
                var validBakugans = Game.BakuganIndex.Where(x => x.Position == User.Position && User.IsEnemyOf(x) && x.BasePower > User.BasePower);

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_ABILITY_DECREASETARGET", TypeId, (int)Kind, validBakugans)
                ));

                Game.OnAnswer[Owner.Id] = Setup3;
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

        public override void TriggerEffect() =>
            new LightningTornadoEffect(User, target, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.IsAttribute(Attribute.Lumina) && user.InBattle;
    }

    internal class LightningTornadoEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public LightningTornadoEffect(Bakugan user, Bakugan target, int typeID, bool isCopy)
        {
            User = user;
            this.target = target;
            IsCopy = isCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });

            // Increase the power of the user Bakugan by 100G
            User.Boost(new Boost(100), this);

            // If a target Bakugan is selected, decrease its power by 100G
            if (target != null)
                target.Boost(new Boost(-100), this);
        }
    }
}
