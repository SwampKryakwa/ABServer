using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class NoseSlap : AbilityCard
    {
        public NoseSlap(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.OnField() && x.Position != User.Position && x.Owner != Owner}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            Game.OnLongRangeBattleOver = () =>
            {
                if (target.OnField() && User.Position is GateCard gatePosition) User.MoveFromFieldToDrop(gatePosition.EnterOrder);
                else if (target.OnField() && User.Position is Player) User.MoveFromHandToDrop();
            };
            Game.StartLongRangeBattle(User, target);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.Position is GateCard posGate && x.Owner != user.Owner && posGate.IsAdjacent((user.Position as GateCard)!));

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(17, CardKind.NormalAbility, (cID, owner) => new NoseSlap(cID, owner, 17));
    }
}
