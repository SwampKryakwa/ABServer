using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{

    internal class SaurusGlow : AbilityCard
    {
        public SaurusGlow(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new SaurusGlowEffect(User, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new SaurusGlowEffect(User, TypeId, IsCopy).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.OnField();

        public static new bool HasValidTargets(Bakugan user) =>
            user.Type == BakuganType.Saurus && user.OnField();
    }

    internal class SaurusGlowEffect : IActive
    {
        public int TypeId { get; }
        public Bakugan User { get; set; }
        Game game { get => User.Game; }


        public Player Owner { get; set; }
        public int EffectId { get; set; }

        public AbilityKind Kind { get; } = AbilityKind.FusionAbility;

        bool IsCopy;

        public SaurusGlowEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
            user.UsedAbilityThisTurn = true;
            this.IsCopy = IsCopy;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            game.ActiveZone.Add(this);
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
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectAddedActiveZone" },
                    { "IsCopy", IsCopy },
                    { "Card", TypeId },
                    { "Kind", (int)Kind },
                    { "Id", EffectId },
                    { "Owner", Owner.Id }
                });
            }

            // Register the effect to boost Saurus when a stronger Bakugan enters the field
            game.BakuganAdded += HandleBakuganAdded;
        }

        private void HandleBakuganAdded(Bakugan target, byte owner, IBakuganContainer pos)
        {
            if (target.Power > User.Power && User.OnField())
            {
                User.Boost(new Boost(50), this);
            }
        }

        public void Negate(bool asCounter = false)
        {
            game.ActiveZone.Remove(this);
            game.BakuganAdded -= HandleBakuganAdded;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", EffectId }
                });
            }
        }
    }
}


