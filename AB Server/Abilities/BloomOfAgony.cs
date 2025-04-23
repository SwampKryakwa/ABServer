using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BloomOfAgony : AbilityCard
    {
        public BloomOfAgony(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

        public override void TriggerEffect() => new BloomOfAgonyEffect(User, TypeId).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.BattleStart && user.OnField() && user.IsAttribute(Attribute.Darkon);
    }

    internal class BloomOfAgonyEffect(Bakugan user, int typeID)
    {
        public int TypeId { get; } = typeID;
        Bakugan User = user;
        Game game { get => User.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            foreach (var bakugan in game.BakuganIndex)
                if (bakugan.OnField())
                    bakugan.Boost(new Boost(-300), this);
        }
    }
}
