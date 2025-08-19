using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class IllusiveCurrent : AbilityCard
    {
        public IllusiveCurrent(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InHand() }
            ];
        }

        public override void TriggerEffect()
        {
            var selectedBakugan = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (User.Position is GateCard positionGate && selectedBakugan.InHand())
            {
                User.MoveFromFieldToHand(positionGate.EnterOrder);
                selectedBakugan.AddFromHand(positionGate);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Owner.Bakugans.Any(x => x.IsAttribute(Attribute.Aqua));

        public static new bool HasValidTargets(Bakugan user) => user.OnField();

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(13, CardKind.NormalAbility, (cID, owner) => new IllusiveCurrent(cID, owner, 13));
    }
}

