using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class FireTornado : AbilityCard
    {
        public FireTornado(int cId, Player owner, int typeId) : base(cId, owner, typeId)
        {
            ResTargetSelectors =
            [
                new YesNoSelector { ForPlayer = (p) => p == Owner, Message = "INFO_WANTTARGET", Condition = () => Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x)) && User.Position is GateCard positionGate && positionGate.Owner.TeamId != Owner.TeamId },
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DECREASETARGET", TargetValidator = x => x.Position == User.Position && User.IsOpponentOf(x), Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
            ];
        }

        public override void TriggerEffect()
        {
            User.Boost(100, this);

            if ((ResTargetSelectors[0] as YesNoSelector)!.IsYes && ResTargetSelectors[1] is BakuganSelector targetSelector)
                targetSelector?.SelectedBakugan.Boost(-100, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.InBattle && user.IsAttribute(Attribute.Nova);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => user.IsOpponentOf(x) && x.Power > user.Power);

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(22, CardKind.NormalAbility, (cID, owner) => new FireTornado(cID, owner, 22));
    }
}
