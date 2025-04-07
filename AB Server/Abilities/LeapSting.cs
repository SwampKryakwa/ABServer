using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class LeapStingEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public LeapStingEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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

            if (target.Power < User.Power)
                if (target.Position is GateCard positionGate)
                    target.DestroyOnField(positionGate.EnterOrder);
        }
    }

    internal class LeapSting : AbilityCard
    {
        public LeapSting(int cID, Player owner, int typeId)
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

            Game.OnAnswer[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_ATTACKTARGET", TypeId, (int)Kind, Game.BakuganIndex.Where(x => x.Owner.SideID != Owner.SideID && x.OnField() && x.Position != User.Position))
            ));

            Game.OnAnswer[Owner.Id] = Activate;
        }

        private Bakugan target;

        public new void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

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
                new LeapStingEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void TriggerEffect() =>
            new LeapStingEffect(User, target, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField() && Game.BakuganIndex.Any(x => x.Owner.SideID != Owner.SideID && x.OnField() && x.Position != user.Position);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && x.Position != user.Position && user.IsEnemyOf(x));
    }
}
