using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities.Correlations
{
    internal class ElementResonance : AbilityCard
    {

        public ElementResonance(int cID, Player owner) : base(cID, owner, 3)
        { }

        public override void TriggerEffect() =>
            new ElementResonanceEffect(User, TypeId, IsCopy).Activate();

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Owner.BakuganOwned.Select(x => x.MainAttribute).Distinct().Count() == 1;

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.BakuganIndex.Any(x => x.Owner == user.Owner && (x.OnField() || x.InHand()));
    }

    internal class ElementResonanceEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public ElementResonanceEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
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
                    { "Kind", 2 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User.Boost(new Boost(50), this);
        }
    }
}
