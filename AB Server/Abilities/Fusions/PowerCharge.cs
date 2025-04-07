using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Fusions
{
    internal class PowerCharge : FusionAbility
    {
        public PowerCharge(int cID, Player owner)
        {
            TypeId = 4; // Assign a unique TypeId for this ability
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(SaurusGlow);
        }
        public override void Resolve()
        {
            if (!counterNegated)
                new PowerChargeEffect(User, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void TriggerEffect() =>
            new PowerChargeEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.IsPartner && user.OnField();
    }

    internal class PowerChargeEffect : IActive
    {
        public int TypeId { get; }
        public Bakugan User { get; set; }
        Game game { get => User.Game; }


        public Player Owner { get => User.Owner; set; }
        public int EffectId { get; set; }

        public CardKind Kind { get; } = CardKind.FusionAbility;

        bool IsCopy;

        public PowerChargeEffect(Bakugan user, int typeID, bool IsCopy)
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

            game.BattlesStarted += OnBattleStart;
        }

        public void OnBattleStart()
        {
            if (User.Type == BakuganType.Saurus && User.Position is GateCard gatePosition && gatePosition.Bakugans.Count >= 2 && gatePosition.Freezing.Count == 0)
            {
                User.Boost(new Boost(300), this);
                game.BattlesStarted -= OnBattleStart;
                game.ActiveZone.Remove(this);

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

        public void Negate(bool asCounter = false)
        {
            game.BattlesStarted -= OnBattleStart;

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

