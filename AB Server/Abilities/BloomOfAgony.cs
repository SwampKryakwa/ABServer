using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BloomOfAgony : AbilityCard
    {
        public BloomOfAgony(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
        }

        public override void TriggerEffect() =>
            new BloomOfAgonyEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.OnField() && user.IsAttribute(Attribute.Darkon);
    }

    internal class BloomOfAgonyEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public BloomOfAgonyEffect(Bakugan user, int typeID, bool IsCopy)
        {
            this.User = user;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" }, { "Kind", 0 },
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

            foreach (var bakugan in game.BakuganIndex)
            {
                if (bakugan.OnField())
                {
                    bakugan.Boost(new Boost(-200), this);
                }
            }
        }
    }
}
