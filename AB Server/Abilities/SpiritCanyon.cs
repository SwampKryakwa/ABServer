using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SpiritCanyon : AbilityCard
    {
        public SpiritCanyon(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
        }

        public override void TriggerEffect() =>
                new SpiritCanyonEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Subterra);
    }

    internal class SpiritCanyonEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public SpiritCanyonEffect(Bakugan user, int typeID, bool IsCopy)
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
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            User.Boost(new Boost((short)(game.GateIndex.Count(x => x.OnField) * 50)), this);
        }
    }
}
