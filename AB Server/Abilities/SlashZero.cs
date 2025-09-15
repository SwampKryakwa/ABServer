using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class SlashZero(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
    {
        public override void TriggerEffect()
        {
            if (User.Owner.AbilityDrop.Any(x => x.Kind == CardKind.FusionAbility))
                User.Boost(80, this);
            if (User.Owner.AbilityDrop.Any(x => x.Kind == CardKind.CorrelationAbility))
                User.Boost(120, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Shredder && user.InBattle;

        [ModuleInitializer]
        internal static void Init() => Register(36, CardKind.NormalAbility, (cID, owner) => new SlashZero(cID, owner, 36));
    }
}
