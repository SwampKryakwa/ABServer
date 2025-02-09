using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override void DoubleEffect() =>
            new PowerChargeEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Saurus && user.OnField();
    }

    internal class PowerChargeEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public PowerChargeEffect(Bakugan user, int typeID, bool IsCopy)
        {
            this.user = user;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Treatment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            game.BattlesStarted += OnBattleStart;
        }

        public void OnBattleStart()
        {
            if (user.Type == BakuganType.Saurus && user.InBattle)
            {
                user.Boost(new Boost(300), this);
                game.BattlesStarted -= OnBattleStart;
            }
        }
    }
}

