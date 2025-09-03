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
            var target = (CondTargetSelectors[0] as GateSelector)!.SelectedGate;
            if (!target.Negated)
            {
                Game.ThrowEvent(EventBuilder.GateReveal(target));
                target.Negate();
            }
            else
            {
                foreach (var bak in target.Bakugans)
                    bak.Boost(-bak.Power, this);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Game.GateIndex.Any(x => x.OnField && x.IsOpen);

        [ModuleInitializer]
        internal static void Init() => Register(3, CardKind.NormalAbility, (cID, owner) => new GrandDown(cID, owner, 3));
    }
}
