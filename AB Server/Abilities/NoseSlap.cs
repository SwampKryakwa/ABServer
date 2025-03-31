using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class NoseSlap : AbilityCard
    {
        public NoseSlap(int cID, Player owner, int typeId)
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

            var validBakugans = Game.BakuganIndex.Where(x => x.OnField() && x.Owner != Owner && IsVerticallyAdjacent(x.Position as GateCard, User.Position as GateCard));

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_ATTACKTARGET", TypeId, (int)Kind, validBakugans)
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
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
                new NoseSlapEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new NoseSlapEffect(User, target, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Type == BakuganType.Elephant && user.OnField() && Game.GateIndex.Any(x => x.OnField && x.IsTouching(user.Position as GateCard) && x.Bakugans.Any(user.IsEnemyOf));

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.GateIndex.Any(x => x.OnField && x.IsTouching(user.Position as GateCard) && x.Bakugans.Any(user.IsEnemyOf));

        private bool IsVerticallyAdjacent(GateCard card1, GateCard card2)
        {
            if (card1 == null || card2 == null)
                return false;

            return card1.IsAdjacentVertically(card2);
        }
    }

    internal class NoseSlapEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public NoseSlapEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            user.UsedAbilityThisTurn = true;
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

            // Compare the powers of the user and the target Bakugan
            if (target.Power < User.Power)
            {
                // Destroy the target Bakugan if it is on the field
                if (target.Position is GateCard positionGate)
                    target.DestroyOnField(positionGate.EnterOrder);
            }
        }
    }
}
