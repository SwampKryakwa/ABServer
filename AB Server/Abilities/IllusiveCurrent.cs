using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class IllusiveCurrentEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan selectedBakugan;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public IllusiveCurrentEffect(Bakugan user, Bakugan selectedBakugan, int typeID, bool IsCopy)
        {
            User = user;
            this.selectedBakugan = selectedBakugan;
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

            var position = User.Position;
            // Return the user to hand

            // Add the selected Bakugan to the GateCard where the user was
            if (position is GateCard positionGate)
            {
                User.ToHand(positionGate.EnterOrder);
                selectedBakugan.AddFromHand(positionGate);
            }
        }
    }

    internal class IllusiveCurrent : AbilityCard
    {
        public IllusiveCurrent(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITYUSER", TypeId, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = User.Attribute == Attribute.Aqua
                ? Owner.BakuganOwned.Where(x => x.InHand())
                : Owner.BakuganOwned.Where(x => x.InHand() && x.Attribute == Attribute.Aqua);

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_SELECT_BAKUGAN", TypeId, validBakugans)
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        private Bakugan selectedBakugan;

        public new void Activate()
        {
            selectedBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new IllusiveCurrentEffect(User, selectedBakugan, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new IllusiveCurrentEffect(User, selectedBakugan, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (selectedBakugan == bakugan)
                selectedBakugan = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField();

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField();
    }
}

