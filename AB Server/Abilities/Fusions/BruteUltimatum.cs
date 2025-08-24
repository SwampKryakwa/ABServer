using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class BruteUltimatum : FusionAbility
    {
        public BruteUltimatum(int cID, Player owner) : base(cID, owner, 7, typeof(MercilessTriumph))
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p.TeamId != Owner.TeamId, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.InHand() && x.Owner != Owner}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (User.Position is GateCard positionGate)
                target?.AddFromHand(positionGate);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Type == BakuganType.Glorius && user.Position is GateCard posGate && posGate.BattleOver && user.JustEndedBattle && !user.BattleEndedInDraw;

        [ModuleInitializer]
        internal static void Init() => FusionAbility.Register(0, (cID, owner) => new BruteUltimatum(cID, owner));
    }
}
