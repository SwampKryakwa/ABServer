using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class DoubleDimension : FusionAbility
    {
        public DoubleDimension(int cID, Player owner) : base(cID, owner, 1, typeof(Dimension4))
        {
            CondTargetSelectors =
            [
                new ActiveSelector() { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x is AbilityCard && x.User.IsOpponentOf(User) }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as ActiveSelector)!.SelectedActive.User;
            if (target.OnField())
                target.Boost(new Boost((short)-target.Power), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Lucifer && user.InBattle && Game.ActiveZone.Any(x => x is AbilityCard && x.User.OnField() && x.User.IsOpponentOf(user));

        [ModuleInitializer]
        internal static void Init() => FusionAbility.Register(4, (cID, owner) => new DoubleDimension(cID, owner));
    }
}
