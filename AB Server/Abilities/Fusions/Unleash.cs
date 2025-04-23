using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Unleash : FusionAbility
    {
        public Unleash(int cID, Player owner) : base(cID, owner, 0, typeof(AbilityCard))
        { }

        public override void TriggerEffect() =>
            new UnleashEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.IsPartner && Game.CurrentWindow == ActivationWindow.Normal;
    }

    internal class UnleashEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game { get => user.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public UnleashEffect(Bakugan user, int typeID, bool IsCopy)
        {
            this.user = user;
            this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            user.Boost(new Boost(50), this);
        }
    }
}
