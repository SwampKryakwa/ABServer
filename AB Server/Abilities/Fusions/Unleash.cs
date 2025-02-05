using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class UnleashEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game;

        public Player Onwer { get; set; }
        bool IsCopy;

        public UnleashEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            this.user = user;
            this.game = game;
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
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }
            user.Boost(new Boost(50), this);
        }
    }
    internal class Unleash : FusionAbility
    {
        public Unleash(int cID, Player owner)
        {
            TypeId = 0;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(AbilityCard);
        }

        public override void Resolve()
        {
            if (!counterNegated || Fusion != null)
                new UnleashEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override bool IsActivateable() =>
            Owner.AbilityHand.Any(x => x is not FusionAbility && x.IsActivateable();
    }
}
