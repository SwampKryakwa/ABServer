using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SpiritCanyonEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game;
        Boost boost;

        public Player Onwer { get; set; }
        bool IsCopy;

        public SpiritCanyonEffect(Bakugan user, Game game, int typeID, bool IsCopy)
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
                    { "Type", "AbilityActivateEffect" },
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
            user.Boost(new Boost((short)(game.GateIndex.Count(x => x.OnField) * 50)), this);
        }
    }

    internal class SpiritCanyon : AbilityCard
    {
        public SpiritCanyon(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new SpiritCanyonEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
                new SpiritCanyonEffect(User, Game, TypeId, IsCopy).Activate();

        public override bool IsActivateableFusion(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Attribute == Attribute.Subterra;


    }
}
