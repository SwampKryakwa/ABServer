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

    internal class SpiritCanyonEffect(Bakugan user, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan User = user;
        Game game { get => User.Game; }

        public Player Owner { get; set; } = user.Owner;
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));
            User.Boost(new Boost((short)(game.GateIndex.Count(x => x.OnField && x.Owner == Owner) * 50)), this);
        }
    }
}
