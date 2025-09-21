using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class DarkBargain : AbilityCard
    {
        public DarkBargain(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_BOOSTTARGET", TargetValidator = x => x.IsOpponentOf(User) }
            ];
        }

        public override void TriggerEffect()
        {
            User.Boost(300, this);
            (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Boost(300, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Game.BakuganIndex.Any(x => x.OnField() && x.Owner.TeamId != user.Owner.TeamId);

        [ModuleInitializer]
        internal static void Init() => Register(48, CardKind.NormalAbility, (cID, owner) => new DarkBargain(cID, owner, 48));
    }
}
