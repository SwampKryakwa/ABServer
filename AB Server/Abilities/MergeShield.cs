using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class MergeShield : AbilityCard
    {
        public MergeShield(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new YesNoSelector { ForPlayer = (p) => p == Owner, Message = "INFO_WANTTARGET", Condition = () => Owner.Bakugans.Count == 0 && User.OnField() && Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x)) },
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Position == User.Position && x.IsOpponentOf(User), Condition = () => Owner.Bakugans.Count == 0 && User.OnField() && Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x)) }
            ];
        }

        public override void Resolve()
        {
            User.Boost(100, this);
            base.Resolve();
        }

        public override void TriggerEffect()
        {
            (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan?.Boost(-100, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Game.BakuganIndex.Any(x => x.IsOpponentOf(user) && x.OnField());

        [ModuleInitializer]
        internal static void Init() => Register(29, CardKind.NormalAbility, (cID, owner) => new MergeShield(cID, owner, 29));
    }
}
