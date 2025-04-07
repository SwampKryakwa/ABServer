using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Fusions
{
    internal class SaurusRage : FusionAbility
    {
        public SaurusRage(int cID, Player owner) : base(cID, owner, 4)
        {
            BaseAbilityType = typeof(SaurusGlow);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new SaurusRageEffect(User, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void TriggerEffect() =>
            new SaurusRageEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.OnField() && Game.BakuganIndex.Any(b => b.Power > user.Power);
    }

    internal class SaurusRageEffect : IActive
    {
        public int TypeId { get; }
        public Bakugan User { get; set; }
        Game game { get => User.Game; }

        public Player Owner { get => User.Owner; set; }
        public int EffectId { get; set; }

        public CardKind Kind { get; } = CardKind.FusionAbility;

        bool IsCopy;

        public SaurusRageEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
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
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
                game.NewEvents[i].Add(EventBuilder.AddEffectToActiveZone(this, IsCopy));
            }

            game.BakuganAdded += OnBakuganAdded;
        }

        private void OnBakuganAdded(Bakugan target, byte owner, IBakuganContainer pos)
        {
            if (User.OnField() && target.Power > User.Power)
            {
                int powerDifference = target.Power - User.Power;
                int boostAmount = powerDifference * 2;
                User.Boost(new Boost((short)boostAmount), this);
            }
        }

        public void Negate(bool asCounter = false)
        {
            game.BakuganAdded -= OnBakuganAdded;

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
