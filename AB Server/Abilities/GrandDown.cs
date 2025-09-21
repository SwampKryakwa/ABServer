using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class GrandDown : AbilityCard
    {
        public GrandDown(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATENEGATETARGET", TargetValidator = x => x.OnField }
            ];
        }

        public override void TriggerEffect()
        {
            foreach (var target in Game.GateIndex.Where(x => x.OnField))
                if (!target.Negated)
                {
                    Game.ThrowEvent(EventBuilder.GateReveal(target));
                    target.Negate();
                }
        }

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon);

        [ModuleInitializer]
        internal static void Init() => Register(3, CardKind.NormalAbility, (cID, owner) => new GrandDown(cID, owner, 3));
    }
}
